using NUnit.Framework;
using OpenQA.Selenium;
using TestSharp.Source.Extensions;
using TestSharp.Source.WebDriver;
using TestSharp.TestCases.Pages;

namespace TestSharp.TestCases.Tests
{
    [Category("SmokeTest")]
    public class SearchTest : TestBase
    {
        public SearchTest() : base(Browser.Chrome)
        {
            
        }

        [Test]
        public void PublixSearch()
        {
            DataSetup td = new DataSetup("TestCases/TestData/propertyAddress");
            driver.GoToSite("https://www.publix.com");
            var SearchTest = new Pages.SearchTest(driver);
            SearchTest.SearchFor("Bakery cakes");
            SearchTest.SelectProduct("Cookies & Cream Hero");
            SearchTest.EnterDesignMessage("Test Message");
            SearchTest.AddToCart();
            SearchTest.ReviewYourStore();
            SearchTest.Checkout();
            SearchTest.AssertSelectedStore("North Pointe Plaza", "15151 N Dale Mabry Hwy, Tampa, FL 33618");
            SearchTest.WhoIsPickingUpInfo(td.firstName, td.lastName, td.phoneNumber,td.emailAddress);
        }
    }
}