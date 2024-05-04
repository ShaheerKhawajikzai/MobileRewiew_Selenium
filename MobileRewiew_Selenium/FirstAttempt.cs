using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileRewiew_Selenium.Models;

namespace MobileRewiew_Selenium
{
    internal class FirstAttempt
    {
        public void FetchData()
        {
            IWebDriver driver = new ChromeDriver();


            driver.Navigate().GoToUrl("https://www.whatmobile.com.pk/");

            IWebElement verticalMenu = driver.FindElement(By.ClassName("verticalMenu"));

            IWebElement secondSection = verticalMenu.FindElements(By.TagName("section"))[1];

            IReadOnlyList<IWebElement> anchorTags = secondSection.FindElements(By.TagName("a"));

            Dictionary<string, int> brandLinks = new Dictionary<string, int>();
            List<string> brandNames = new List<string>();

            var id = 0;

            foreach (IWebElement anchorTag in anchorTags)
            {
                brandLinks.Add(anchorTag.GetAttribute("href"), ++id);
                brandNames.Add(anchorTag.Text);
            }

            foreach (var hrefValue in brandLinks)
            {
                Console.WriteLine(hrefValue);
            }

            List<List<string>> anchorHrefs = new List<List<string>>();

            brandLinks = brandLinks.Take(2).ToDictionary<string, int>();
            foreach (var kvp in brandLinks)
            {
                string mainLink = kvp.Key;
                driver.Navigate().GoToUrl(mainLink);

                IReadOnlyList<IWebElement> anchorElements = driver.FindElements(By.CssSelector("ul.nav-tabs li a"));

                List<string> hrefs = new List<string>();

                foreach (IWebElement anchorElement in anchorElements)
                {
                    hrefs.Add(anchorElement.GetAttribute("href"));
                }

                anchorHrefs.Add(hrefs);
            }

            // Output the hrefs for each main link
            for (int i = 0; i < brandLinks.Count; i++)
            {
                Console.WriteLine($"Main Link: {brandLinks.Keys.ElementAt(i)}");
                Console.WriteLine("Hrefs:");
                foreach (var href in anchorHrefs[i])
                {
                    Console.WriteLine(href);
                }
                Console.WriteLine();
            }

            Dictionary<string, int> allDetailPageLinks = new Dictionary<string, int>();
            int index = 0;

            foreach (var links in anchorHrefs)
            {
                index++;
                foreach (var link in links)
                {
                    driver.Navigate().GoToUrl(link);

                    IReadOnlyList<IWebElement> itemDivs = driver.FindElements(By.CssSelector("div.item"));

                    foreach (var item in itemDivs)
                    {
                        var anchorTag = item.FindElement(By.CssSelector("div a"));
                        string hrefAttributeValue = anchorTag.GetAttribute("href");

                        if (!allDetailPageLinks.ContainsKey(hrefAttributeValue))
                        {
                            allDetailPageLinks.Add(hrefAttributeValue, index);
                        }
                    }
                }
            }

            allDetailPageLinks = allDetailPageLinks.Take(10).ToDictionary<string, int>();

            foreach (var item in allDetailPageLinks)
            {
                Console.WriteLine(item);
            }

            List<Device> listofDevices = new List<Device>();
            List<List<string>> ListOfcol = new List<List<string>>();
            List<string> listOfModelNames = new List<string>();
            var textToRemove = "Price in Rs:";

            // Visit Detail pages 
            foreach (var item in allDetailPageLinks)
            {
                var link = item.Key;
                var value = item.Value;

                driver.Navigate().GoToUrl(link);

                IWebElement modelName = driver.FindElement(By.CssSelector("p > b"));
                IWebElement modelDescription = driver.FindElement(By.CssSelector("p:nth-child(5)"));

                listOfModelNames.Add(modelName.Text.Split('-')[0]);


                IReadOnlyList<IWebElement> rows = driver.FindElements(By.CssSelector("tr.RowBG1, tr.RowBG2"));

                List<string> lastColumnTexts = new List<string>();

                foreach (IWebElement row in rows)
                {
                    IReadOnlyList<IWebElement> cells = row.FindElements(By.TagName("td"));
                    bool isPrices = false;

                    string lastCellText = cells.Last().Text;

                    if (lastCellText.StartsWith(textToRemove))
                    {
                        string[] prices = lastCellText.Split("  ");


                        lastColumnTexts.Add(prices[0].Substring(textToRemove.Length));
                        lastColumnTexts.Add(prices[2].Substring(textToRemove.Length + 4));
                        lastColumnTexts.Add(modelDescription.Text);

                        isPrices = true;
                    }
                    if (!isPrices)
                        lastColumnTexts.Add(lastCellText);
                }

                ListOfcol.Add(lastColumnTexts);

                foreach (var text in lastColumnTexts)
                {

                    Console.WriteLine(text);
                }
            }

            //Mapping
            var k = -1;
            foreach (var item in ListOfcol)
            {
                var j = 0;
                k++;

                Device device = new Device()
                {
                    Model = listOfModelNames[k],
                    OperatinSystem = item[j],//0
                    UserInterface = item[++j],
                    Dimensions = item[++j],
                    Weight = item[++j],
                    Sim = item[++j],
                    Colors = item[++j],
                    TwoGBand = item[++j],
                    ThreeGBand = item[++j],
                    FourGBand = item[++j],
                    FiveGBand = item[++j],
                    CPU = item[++j],
                    Chipset = item[++j],
                    GPU = item[++j],
                    Technology = item[++j],
                    Size = item[++j],
                    Resolution = item[++j],
                    Protection = item[++j],
                    ExtraFeatures = item[++j],
                    BuiltIn = item[++j],
                    Card = item[++j],
                    Main = item[++j],
                    Features = item[++j],
                    Front = item[++j],
                    WLAN = item[++j],
                    Bluetooth = item[++j],
                    GPS = item[++j],
                    USB = item[++j],
                    NFC = item[++j],
                    Data = item[++j],
                    Sensors = item[++j],
                    Audio = item[++j], //30
                    Browser = item[++j],
                    Messaging = item[++j],
                    Games = item[++j],
                    Torch = item[++j],
                    Extra = item[++j],
                    Capacity = item[++j] + item[++j],
                    PriceInPKR = item[++j].Trim(), //38
                    PriceInUSD = item[++j].Trim(), //39
                    Description = item[++j] //40
                };
                listofDevices.Add(device);


            }
        }
    }
}
