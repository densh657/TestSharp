using System;
using NUnit.Framework;
using OpenQA.Selenium;
using TestSharp.Source.Reporter;
using TestSharp.Source.Reports;
using TestSharp.Source.Utilities;

namespace TestSharp.Source.WebDriver
{
    public class TestBase
    {
        protected IWebDriver driver;
        private Browser browser;

        public TestBase(Browser browser = Browser.Chrome)
        {
            this.browser = browser;
        }

        [OneTimeSetUp]
        public void SetUpEnv()
        {
            DirectoryManager.CheckReportFolders();
            ExtentTestManager.CreateParentTest(GetType().Name);
            driver = new DriverFactory().InitDriver(browser);
        }

        [SetUp]
        public void SetUpTest()
        {
            ExtentTestManager.CreateTest(TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public void TearDownTest()
        {
            ExtentManager.FinishReport(driver);
        }

        [OneTimeTearDown]
        public void TearDownEnv()
        {
            driver?.Quit();
            ExtentManager.Instance.Flush();
            TestContext.AddTestAttachment(ExtentManager.reportPath + @"\index.html");
        }
    }
}