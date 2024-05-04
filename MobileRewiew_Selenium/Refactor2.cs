using MobileRewiew_Selenium.Models;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MobileRewiew_Selenium
{
    public class Refactor2
    {
        public void FetchData()
        {

            string ImagesPath = "C:\\Users\\Shaheer Khawjikzai\\" +
                                "OneDrive\\Desktop\\Technologia\\" +
                                "MobileReviewsProject\\Images\\";

            if (Directory.Exists(ImagesPath))
            {
                var files = Directory.GetFiles(ImagesPath);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }

            IWebDriver driver = new ChromeDriver();

            driver.Navigate().GoToUrl("https://www.whatmobile.com.pk/");

            IWebElement verticalMenu = driver.FindElement(By.ClassName("verticalMenu"));

            IWebElement secondSection = verticalMenu.FindElements(By.TagName("section"))[1];

            IReadOnlyList<IWebElement> anchorTags = secondSection.FindElements(By.TagName("a"));

            List<Brand> brandInfo = new List<Brand>(); //Stores BrandInfo

            List<DeviceLink> listOfDevicesLinks = new List<DeviceLink>(); //Stores detail page links of devices.

            var id = 0;
            foreach (IWebElement anchorTag in anchorTags)
            {
                Brand brand = new Brand()
                {
                    BrandId = ++id,
                    BrandName = anchorTag.Text,
                    BrandUrl = anchorTag.GetAttribute("href")
                };
                brandInfo.Add(brand);
            }


            foreach (var item in brandInfo)
            {
                var brandSlug = item.BrandName + "-" + item.BrandName;

                var query = $"Insert into Brands (Id, Name,Slug) Values( {item.BrandId}, '{item.BrandName}', '{brandSlug}') ";
                Console.WriteLine(query);

            }


            brandInfo = brandInfo.Take(1).ToList<Brand>();

            foreach (var brand in brandInfo)
            {
                string mainLink = brand.BrandUrl;

                driver.Navigate().GoToUrl(mainLink);

                //PricesELement, LatestElement , ComingSoonElement
                IReadOnlyList<IWebElement> anchorElements = driver.FindElements(By.CssSelector("ul.nav-tabs li a"));

                //PriceLink , LatestLink , ComingSoonLink
                List<string> threeAnchorTags = new List<string>();

                foreach (var anchor in anchorElements)
                {
                    threeAnchorTags.Add(anchor.GetAttribute("href"));
                }

                foreach (var anchorElement in threeAnchorTags)
                {
                    //var link = anchorElement.GetAttribute("href");

                    driver.Navigate().GoToUrl(anchorElement);

                    IReadOnlyList<IWebElement> itemDivs = driver.FindElements(By.CssSelector("div.item"));


                    foreach (var item in itemDivs)
                    {
                        var deviceUrl = item.FindElement(By.CssSelector("div a")).GetAttribute("href");
                        var imageUrl = item.FindElement(By.CssSelector("div a img")).GetAttribute("src");

                        //Add detail page links,ImageUrl and brand-id.
                        DeviceLink deviceLink = new DeviceLink()
                        {
                            BrandId = brand.BrandId,
                            DeviceUrl = deviceUrl,
                            ImageUrl = imageUrl
                        };

                        listOfDevicesLinks.Add(deviceLink);
                    }
                }
            }


            List<List<string>> tempListOfData = new List<List<string>>(); //Just to hold Data.

            listOfDevicesLinks = listOfDevicesLinks.Take(10).ToList();

            //Loop over the detail page links 
            int maxRetries = 3;
            int currentTry = 0;

            foreach (var item in listOfDevicesLinks)
            {
                var pageLoad = false;

                while (currentTry < maxRetries)
                {
                    try
                    {
                        driver.Navigate().GoToUrl(item.DeviceUrl);
                        pageLoad = true;
                        break;
                    }
                    catch (Exception)
                    {
                        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));

                        var isLoaded = wait.Until(driver =>
                        driver.FindElements(By.CssSelector("p > b, .Heading1 > .google-anno:nth-child(1) > .google-anno-t ,div:nth-child(3) > .Heading1,.RowBG1 > .Heading1 ")));

                        if (isLoaded.Any())
                        {
                            pageLoad = true;
                            break;
                        }

                        Console.WriteLine("Cannot Load Page");
                        currentTry++;
                        Thread.Sleep(1000); // Add a delay between retries to avoid overwhelming the server
                    }
                }

                List<string> list = new List<string>();

                if (!pageLoad)
                {
                    //Store fail data
                    list.Add("Failed Data");
                    continue;
                }

                //Model + Description
                string modelName = driver.FindElement(By.CssSelector("p > b, .Heading1 > .google-anno:nth-child(1) > .google-anno-t ,div:nth-child(3) > .Heading1,.RowBG1 > .Heading1 ")).Text;
                string modelDescription = driver.FindElement(By.CssSelector("table:nth-child(3) > tbody > .RowBG1:nth-child(7) > td:nth-child(2),p:nth-child(5),.RowBG2:nth-child(1) > .fasla ")).Text;


                //Table row where all the data is present 
                IReadOnlyList<IWebElement> rows = driver.FindElements(By.CssSelector("tr.RowBG1, tr.RowBG2"));

                var textToRemove = "Price in Rs:";

                list.Add(modelName.Split('-')[0]);

                list.Add(modelDescription.Replace("'", "''"));

                list.Add(item.BrandId.ToString());
                list.Add(item.ImageUrl);

                foreach (IWebElement row in rows)
                {
                    IReadOnlyList<IWebElement> cells = row.FindElements(By.TagName("td"));
                    bool isPrices = false;

                    string lastCellText = cells.Last().Text;

                    if (lastCellText.StartsWith(textToRemove))
                    {
                        string[] prices = lastCellText.Split("  ");

                        var charToBeRemovedIndex = prices[0].Substring(textToRemove.Length).IndexOf(',');

                        if (charToBeRemovedIndex >= 0)
                        {
                            list.Add(prices[0].Substring(textToRemove.Length).Remove(charToBeRemovedIndex, 1));
                        }
                        else
                        {
                            list.Add("0");
                        }

                        list.Add(prices[2].Substring(textToRemove.Length + 4));

                        isPrices = true;
                    }

                    if (!isPrices)
                        list.Add(lastCellText);
                }

                tempListOfData.Add(list);
            }

            var deviceQueries = new List<string>();

            foreach (var item in tempListOfData)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    var query = $"Insert into Devices Values ()";

                    Console.WriteLine(query);

                    deviceQueries.Add(query);
                }
            }

            string deviceQueryPath = "C:\\Users\\Shaheer Khawjikzai\\OneDrive\\Desktop\\" +
                                       "Technologia\\MobileReviewsProject\\TestQuery\\";

            File.WriteAllLines(deviceQueries + "FirstTwo.txt", deviceQueries);

            static void DownloadImage(string imageUrl, string destinationPath)
            {
                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        webClient.DownloadFile(imageUrl, destinationPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error downloading image: {ex.Message}");
                    }
                }
            }


        }
    }
}

