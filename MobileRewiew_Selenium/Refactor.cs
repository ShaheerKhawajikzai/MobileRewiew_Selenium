using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileRewiew_Selenium.Models;
using OpenQA.Selenium.DevTools.V121.Browser;
using System.Net;
using OpenQA.Selenium.DevTools.V121.Page;
using OpenQA.Selenium.Support.UI;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.InteropServices;

namespace MobileRewiew_Selenium
{
    internal class Refactor
    {
        public void FetchData()
        {

            string ImagesPath = "C:\\Users\\Shaheer Khawjikzai\\" +
                                "OneDrive\\Desktop\\Technologia\\" +
                                "MobileReviewsProject\\TestImages\\";

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

            List<DeviceLink> listOfDevicesLinks = new List<DeviceLink>(); //Stores detail pages links of devices.

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

            listOfDevicesLinks = listOfDevicesLinks.Skip(10).Take(10).ToList();

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
                        Thread.Sleep(2000); // Add a delay between retries to avoid overwhelming the server
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

                //List<string> list = new List<string>();
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

            List<Device> listOfDevices = new List<Device>(); // Where the actual data will get stored.

            //1: Coloum oper neechay hain  Check first mobile of samsung and 50th.
            //2: galaxy a05s 6gb UI,5gBrand,Protection,ExtraFeatures col missing hai.
            //3: Radio col extra hai.

            //Mapping
            foreach (var item in tempListOfData)
            {
                Device dev = new Device();
                var j = 0;

                dev.Model = item[j]; //0
                dev.Description = item[++j];
                dev.BrandId = item[++j];
                dev.ImageUrl = item[++j]; //4

                dev.OperatinSystem = item[++j];
                dev.UserInterface = item[++j];
                dev.Dimensions = item[++j];
                dev.Weight = item[++j];
                dev.Sim = item[++j];
                dev.Colors = item[++j];
                dev.TwoGBand = item[++j];
                dev.ThreeGBand = item[++j];
                dev.FourGBand = item[++j];
                if (item.Count >= 46)                
                    dev.FiveGBand = item[++j];
               
                dev.CPU = item[++j];
                dev.Chipset = item[++j];
                dev.GPU = item[++j];
                dev.Technology = item[++j];
                dev.Size = item[++j];
                dev.Resolution = item[++j];
                if (item.Count > 44)
                {

                dev.Protection = item[++j];
                }
                dev.ExtraFeatures = item[++j];
                dev.BuiltIn = item[++j];
                dev.Card = item[++j];
                dev.Main = item[++j];
                dev.Features = item[++j];
                dev.Front = item[++j];
                dev.WLAN = item[++j];
                dev.Bluetooth = item[++j];
                dev.GPS = item[++j];

                if (item.Count > 46)
                    dev.Radio = item[++j]; //30

                dev.USB = item[++j];
                dev.NFC = item[++j];
                dev.Data = item[++j];
                dev.Sensors = item[++j];
                dev.Audio = item[++j];
                dev.Browser = item[++j];
                dev.Messaging = item[++j];
                dev.Games = item[++j];
                dev.Torch = item[++j];
                dev.Extra = item[++j];
                dev.Capacity = item[++j] + item[++j]; //41
                dev.PriceInPKR = item[++j]; //42
                dev.PriceInUSD = item[++j].Trim();


                listOfDevices.Add(dev);
            }

            var deviceQueries = new List<string>();

            //Download Images
            foreach (var item in listOfDevices)
            {
                var fileName = Guid.NewGuid().ToString() + ".jpg";
                var destination = ImagesPath + fileName;

                var index = item.Weight.IndexOf(' ');
                var weight = item.Weight.Substring(0, index);


                string imageUrlInDB = @"\images\" + fileName;

                Console.WriteLine();

                var query = $"Insert into Devices Values ('{item.Model}', '{item.Description}','{imageUrlInDB}'," +

                    $"{item.PriceInPKR},{item.PriceInUSD},'{item.OperatinSystem}','{item.UserInterface}','{item.Dimensions}',{weight}," +

                    $"'{item.Sim}','{item.Colors}','{item.TwoGBand}','{item.ThreeGBand}','{item.FourGBand}','{item.FiveGBand}'," +

                    $"'{item.CPU}','{item.Chipset}','{item.GPU}','{item.Technology}','{item.Resolution}','{item.Protection}'," +

                    $"'{item.ExtraFeatures}','{item.BuiltIn}','{item.Card}','{item.Main}','{item.Features}','{item.Front}'," +

                    $"'{item.WLAN}','{item.Bluetooth}','{item.GPS}','{item.USB}','{item.NFC}','{item.Data}', " +

                    $"'{item.Sensors}','{item.Audio}','{item.Browser}','{item.Messaging}','{item.Games}','{item.Torch}', " +

                    $"'{item.Extra}','{item.Capacity}',{item.BrandId},'{item.Slug}',{item.View} ,'{item.ReleaseDate.ToShortDateString()}','{item.Radio}' )";

                Console.WriteLine();

                Console.WriteLine(query);

                deviceQueries.Add(query);

                DownloadImage(item.ImageUrl, destination);
            }

            string deviceQueryPath = "C:\\Users\\Shaheer Khawjikzai\\OneDrive\\Desktop\\" +
                                        "Technologia\\MobileReviewsProject\\TestQuery\\";

            File.WriteAllLines(deviceQueryPath + "SamSkip10Take10.txt", deviceQueries);

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

            driver.Quit();

        }
    }
}

//Device device = new Device()
//{
//    Model = item[j], //0
//    Description = item[++j],
//    BrandId = item[++j],
//    ImageUrl = item[++j],
//    OperatinSystem = item[++j],
//    UserInterface = item[++j],
//    Dimensions = item[++j],
//    Weight = item[++j],
//    Sim = item[++j],
//    Colors = item[++j],
//    TwoGBand = item[++j],
//    ThreeGBand = item[++j],
//    FourGBand = item[++j],
//    FiveGBand = item[++j],
//    CPU = item[++j],
//    Chipset = item[++j],
//    GPU = item[++j],
//    Technology = item[++j],
//    Size = item[++j],
//    Resolution = item[++j],
//    Protection = item[++j],
//    ExtraFeatures = item[++j],
//    BuiltIn = item[++j],
//    Card = item[++j],
//    Main = item[++j],
//    Features = item[++j],
//    Front = item[++j],
//    WLAN = item[++j],
//    Bluetooth = item[++j],
//    GPS = item[++j],
//    Radio = item[++j],//30
//    USB = item[++j],
//    NFC = item[++j],
//    Data = item[++j],
//    Sensors = item[++j],
//    Audio = item[++j],
//    Browser = item[++j],
//    Messaging = item[++j],
//    Games = item[++j],
//    Torch = item[++j],
//    Extra = item[++j],
//    Capacity = item[++j] + item[++j], //41
//    PriceInPKR = item[++j], //42
//    PriceInUSD = item[++j].Trim(), //43
//    View = 0,
//};