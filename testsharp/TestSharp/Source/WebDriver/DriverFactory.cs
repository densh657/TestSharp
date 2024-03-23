using System;
using AventStack.ExtentReports.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using TestSharp.Source.Utilities;
using WebDriverManager.DriverConfigs.Impl;

namespace TestSharp.Source.WebDriver
{
    public class DriverFactory
    {
        private IWebDriver Driver;
        private int timeout;
        
        public DriverFactory()
        {
            timeout = Parameters.wait.IsNullOrEmpty() ? 5 : Int32.Parse(Parameters.wait);
        }

        public IWebDriver InitDriver(Browser browser)
        {
            if (Parameters.browser.IsNullOrEmpty())
            {
                switch (browser)
                {
                    case Browser.Chrome:
                        Driver = GetChromeDriver();
                        break;
                    case Browser.Edge:
                        Driver = GetEdgeDriver();
                        break;
                    default:
                        Driver = GetChromeDriver();
                        break;
                }
            }
            else
            {
                Driver = InitDriver(Parameters.browser);
            }

            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(timeout);
            Driver.Manage().Timeouts().PageLoad.Add(TimeSpan.FromSeconds(30));
            
            return Driver;
        }
        
        public IWebDriver InitDriver(string browser)
        {
            switch (browser)
            {
                case "Chrome":
                    Driver = GetChromeDriver();
                    break;
                case "Edge":
                    Driver = GetEdgeDriver();
                    break;
                default:
                    Driver = GetChromeDriver();
                    break;
            }

            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(timeout);
            Driver.Manage().Timeouts().PageLoad.Add(TimeSpan.FromSeconds(30));
            
            return Driver;
        }
        
        private IWebDriver GetChromeDriver()
        {
            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
            var options = new ChromeOptions();
            if (Parameters.headless == "true")
            {
                options.AddArgument("--headless");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1980,1080");
            }
            else
            {
                options.AddArgument("start-maximized");
            }

            Driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), 
                options, TimeSpan.FromMinutes(3));
            return Driver;
        }
        
        private IWebDriver GetEdgeDriver()
        {
            new WebDriverManager.DriverManager().SetUpDriver(new EdgeConfig());
            var options = new EdgeOptions();
            if (Parameters.headless == "true")
            {
                options.AddArgument("--headless");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1980,1080");
            }
            else
            {
                options.AddArgument("start-maximized");
            }

            Driver = new EdgeDriver(EdgeDriverService.CreateDefaultService(), 
                options, TimeSpan.FromMinutes(3));
            
            return Driver;
            
        }
    }
}