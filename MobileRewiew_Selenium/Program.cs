using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V121.CSS;
using System.Text;

namespace MobileRewiew_Selenium
{
    public class Program
    {
        static void Main(string[] args)
        {

            Refactor refactor = new Refactor();
            refactor.FetchData();


        }
    }
}
