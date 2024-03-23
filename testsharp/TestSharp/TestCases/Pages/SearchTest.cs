using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using TestSharp.Source.Extensions;

namespace TestSharp.TestCases.Pages
{
    public class SearchTest
    {
        private IWebDriver driver;

        public SearchTest(IWebDriver driver)
        {
            this.driver = driver;
        }

        public string searchBar = "//input[@placeholder=\"Search products, savings, or recipes\"]";
        public string searchBarBtn =
            "//input[@placeholder=\"Search products, savings, or recipes\"]/following-sibling::button[@data-qa-automation=\"search-button\"]";
       
        public string productOption = "//div[@data-qa-automation='prod-title']/child::a[contains(text(), 'replaceValue')]";
        public string designOptionsFlavor = "//p[contains(text(),'Vanilla-Chocolate Combo')]";
        public string designOptionsMessage = "//textarea[@data-qa-automation=\"configurator-section-message\"]";
        public string icingCOlor = "//p[contains(text(),'Green')]";

        public string addToOrderBtn = "(//button[@data-qa=\"add-to-order-btn\"])[1]";
        public string reviewOrderBtn = "//span[normalize-space()=\"Review order\"]";
        public string checkout = "//div[@data-qa-automation=\"summary-footer\"]//span[contains(text(),'Checkout')]";

        public string reviewYourStore = "//span[contains(text(),'Pick up at this store')]";
        
        public string firstNameField = "//label[contains(text(),'First Name')]/following::input[@name='FirstName']";
        public string lastNameField = "//label[contains(text(),'Last Name')]/following::input[@name='LastName']";
        public string phoneNumberField = "//label[contains(text(),'Phone Number')]/following::input[@name='ContactPhone']";
        public string emailAddressField = "//label[contains(text(),'Email Address')]/following::input[@name='Email']";

        public string _selectedStoreText = "//h3[@data-qa-automation='selected-store-title']";

        public string _selectedStoreAddressText =
            "//h3[@data-qa-automation='selected-store-title']/parent::div/following-sibling::div[not(contains(@class, 'hours-wrapper'))]";

        public void SearchFor(string product)
        {
            driver.WaitForElement(By.XPath(searchBar),10000 );
            driver.SendKeys(By.XPath(searchBar), product);
            driver.Click(By.XPath(searchBarBtn));
        }
        public void SelectProduct(string product)
        {
            driver.Click(By.XPath(productOption.Replace("replaceValue", product)));
        }

        public void EnterDesignMessage(string designMessage)
        {
            driver.Click(By.XPath(designOptionsFlavor));
            driver.SendKeys(By.XPath(designOptionsMessage),designMessage);
            driver.Click(By.XPath(icingCOlor));
        }
        public void AddToCart()
        {
            driver.Click(By.XPath(addToOrderBtn));
            driver.Click((By.XPath(reviewOrderBtn)));
        }
        public void ReviewYourStore()
        {
            driver.Click(By.XPath(reviewYourStore));
        }
        public void Checkout()
        {
            driver.Click(By.XPath(checkout));
        }

        public void AssertSelectedStore(string store, string address)
        {
            driver.AssertElementText(By.XPath(_selectedStoreText), store);
            driver.AssertElementText(By.XPath(_selectedStoreAddressText), address);
        }

        public void WhoIsPickingUpInfo(string firstName, string lastName, string phoneNumber, string emailAddress)
        {
            driver.SendKeys(By.XPath(firstNameField), firstName);
            driver.SendKeys(By.XPath(lastNameField), lastName);
            driver.SendKeys(By.XPath(phoneNumberField),phoneNumber );
            driver.SendKeys(By.XPath(emailAddressField), emailAddress);
        }
    }
}