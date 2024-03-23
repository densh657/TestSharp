using NUnit.Framework;
using OpenQA.Selenium;
using TestSharp.Source.Extensions;
using TestSharp.Source.WebDriver;
using TestSharp.TestCases.Pages;

namespace TestSharp.TestCases.Tests
{
    [Category("SmokeTest")]
    public class ContactUsTest : TestBase
    {
        public ContactUsTest() : base(Browser.Chrome)
        {
            
        }

        [Test]
        public void ContactUs()
        {
            DataSetup td = new DataSetup("TestCases/TestData/propertyAddress");
            driver.GoToSite("https://nuqleous.com/contact/");
            var ContactPage = new Pages.ContactPage(driver);
            ContactPage.AssertFieldsRequired("AAAPlease complete all required fields.");
            ContactPage.CompleteForm(td.fullName, td.company, td.emailAddress, td.phoneNumber, "message text");
        }
    }
}