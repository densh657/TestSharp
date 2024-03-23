using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using TestSharp.Source.Extensions;

namespace TestSharp.TestCases.Pages
{
    public class ContactPage
    {
        private IWebDriver driver;

        public ContactPage(IWebDriver driver)
        {
            this.driver = driver;
        }
        
        public string yourNameField = "//input[@name='firstname']";
        public string companyField = "//input[@name='company']";
        public string emailField = "//input[@name='email']";
        public string phoneNumberField = "//input[@name='0-2/phone']";
        public string messageField = "//textarea[@name='message']";
        public string contactUsBtn = "//input[@value='Contact Us']";

        public string yourNameRequired = "";
        public string emailRequired = "";
        public string requiredFields = "//label[normalize-space()='Please complete all required fields.']";

        public void AssertFieldsRequired(string required)
        {
            driver.Click(By.XPath(contactUsBtn));
            Thread.Sleep(3000);
            driver.AssertElementText(By.XPath(requiredFields), required);
        }

        public void CompleteForm(string yourName, string company, string emailAddress, string phoneNumber,
            string message)
        {
            driver.SendKeys(By.XPath(yourNameField), yourName);
            driver.SendKeys(By.XPath(companyField), company);
            driver.SendKeys(By.XPath(emailField), emailAddress);
            driver.SendKeys(By.XPath(phoneNumberField), phoneNumber);
            driver.SendKeys(By.XPath(messageField), message);
            //driver.Click(By.XPath(contactUsBtn));
        }
    }
}