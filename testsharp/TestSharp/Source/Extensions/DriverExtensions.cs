using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using AventStack.ExtentReports;
using FluentAssertions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Selenium.Axe;
using SoftAssertion;
using TestSharp.Source.DataHelpers;
using TestSharp.Source.Reporter;
using TestSharp.Source.Reports;
using TestSharp.Source.Utilities;

namespace TestSharp.Source.Extensions
{
    public static class DriverExtensions
    {
        #region Driver Methods

        private static WebDriverWait WebdriverWait(this IWebDriver driver, int timeout)
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
        }

        /// <summary>
        /// Takes a full page screenshot
        /// </summary>
        /// <param name="driver"></param>
        public static void TakeScreenShot(this IWebDriver driver)
        {
            var extentTest = ExtentTestManager.GetTest();

            var testClassName = TestContext.CurrentContext.Test.ClassName.Split('.').Last();
            var screenShotName = testClassName + "-" + TestContext.CurrentContext.Test.Name + DataGenerators.RandomNum(3) + ".png";

            Screenshot screenShot = ((ITakesScreenshot) driver).GetScreenshot();
            var localPath = ExtentManager.reportPath + screenShotName;
            screenShot.SaveAsFile(localPath, ScreenshotImageFormat.Png);

            var relPath = DirectoryManager.MakeRelative(localPath, ExtentManager.reportPath);
            extentTest.Log(Status.Info, $"Screenshot is available at the end of the test, its name is: <br>" +
                                        $"<pre lang='json'><code>{screenShotName}</code></pre>" + 
                                        extentTest.AddScreenCaptureFromPath(relPath));
        }

        /// <summary>
        /// Used to wait for WebElement.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="timeout">Used to wait for WebElement.</param>
        /// <param name="stepRecorded">Used for logging to Reporter.</param>
        /// <returns></returns>
        public static void WaitForElement(this IWebDriver driver, By locator, int timeout,
            bool stepRecorded = true)
        {
            try
            {
                driver.WebdriverWait(timeout).Until(drv => drv.FindElement(locator));
            }
            catch (Exception ex)
            {
                if (stepRecorded)
                {
                    ExtentManager.LogStep($"Error. Wait timed out after <b>{timeout}</b> seconds." +
                                          $"<br><pre lang='json'><code> Locator: {locator}</code></pre>.",
                        Status.Fail);
                    Assert.Warn(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Used to in combination with IF statement (ex. if (driver.IsVisible(locator) ***do something*** )
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static bool IsVisible(this IWebDriver driver, By element, int timeout = 0)
        {
            try
            {
                driver.WaitForElement(element, timeout, stepRecorded:false);
                return driver.FindElement(element).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Used to assert if element is visible or not.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="expectedCondition">Used to pass condition to check if element is present (True)
        /// /NOT present (False).</param>
        public static void AssertElementIsVisible(this IWebDriver driver, By locator, bool expectedCondition = true, bool screenShot = true)
        { 
            bool elementFound = IsVisible(driver ,locator);

            if (elementFound && expectedCondition)
            {
                ExtentManager.LogStep($"Element is visible. <br>" +
                                      $"<pre lang='json'><code>Locator: {locator}</code></pre>");
            }
            else if (!elementFound && !expectedCondition)
            {
                ExtentManager.LogStep($"Element is NOT visible as expected. <br>" +
                                      $"<pre lang='json'><code>Locator: {locator}</code></pre>");
            }
            else if (elementFound && !expectedCondition)
            {
                ExtentManager.LogStep($"Element is visible and should NOT be visible. <br>" +
                                      $"<pre lang='json'><code>Locator: <font color='red'>{locator}<font></code></pre>", Status.Fail);
                Assert.Warn($"Element \"{locator}\" is visible and should NOT be visible.");
            }
            else if (!elementFound && expectedCondition)
            {
                if (screenShot) TakeScreenShot(driver);

                ExtentManager.LogStep($"Element is NOT visible. <br>" +
                                      $"<pre lang='json'><code>Locator: <font color='red'>{locator}<font></code></pre>", Status.Fail);
                Assert.Warn($"Element \"{locator}\" is NOT visible.");
            }
        }
        
        /// <summary>
        /// Used to assert if element is present or not.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="expectedCondition">Used to pass condition to check if element is present (True)
        /// /NOT present (False).</param>
        public static bool IsPresent(this IWebDriver driver, By locator)
        {
            try
            {
                driver.FindElement(locator);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Used to assert if element is visible or not.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="expectedCondition">Used to pass condition to check if element is present (True)
        /// /NOT present (False).</param>
        public static void AssertElementIsPresent(this IWebDriver driver, By element, bool expectedCondition = true, bool screenShot = true)
        {
            bool elementFound = IsPresent(driver, element);

            if (elementFound && expectedCondition)
            {
                ExtentManager.LogStep($"Element is present. <br>" +
                                      $"<pre lang='json'><code>{element}</code></pre>");
            }
            else if (elementFound == false && expectedCondition == false)
            {
                ExtentManager.LogStep($"Element is NOT present as expected. <br>" +
                                      $"<pre lang='json'><code>{element}</code></pre>");
            }
            else
            {
                if (screenShot) TakeScreenShot(driver);

                ExtentManager.LogStep($"Element is NOT present. <br>" +
                                      $"<pre lang='json'><code><font color='red'>{element}<font></code></pre>", Status.Fail);
                Assert.Warn($"Element \"{element}\" is NOT present.");
            }
        }

        /// <summary>
        /// Will navigate to specified URL within browser.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="appUrl">Used to pass desired URL as a string.</param>
        /// <param name="logStatus">Used for logging to Reporter.</param>
        public static void GoToSite(this IWebDriver driver, string appUrl, bool logStatus = true)
        {
            driver.Navigate().GoToUrl(appUrl);
            if (logStatus) ExtentManager.LogStep($"Navigating to URL: <br> <a href='{appUrl}'>{appUrl}</a>");
        }
        
        /// <summary>
        /// Used to send text to text box. Clears field by default.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="value">Used for value that will be entered to the text box.</param>
        /// <param name="timeout">Used for wait for WebElement. Default wait is 0 seconds.</param>
        /// <param name="clear">Used to clear field before entering data. Default value is True, can be set to False if
        /// needs to be disabled.</param>
        public static void SendKeys(this IWebDriver driver, By locator, string value, bool clear = true, bool logStep = true, bool debug = false, bool screenShot = true)
        {
            try
            {
                Thread.Sleep(1000);
                var webElement = driver.FindElement(locator);
                driver.ScrollElementIntoView(locator);
                if (clear)
                {
                    webElement.Clear();
                }
                webElement.SendKeys(value);
                ExtentManager.LogStep($"Text value was entered to the element. <br>" +
                                      $"<pre lang='json'><code>Value: {value}<br>Locator: {locator}</code></pre>");
            }
            catch (Exception ex)
            {
                if (screenShot) TakeScreenShot(driver);

                if (ex is ElementNotInteractableException)
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep($"Element is not interactable. <br>" +
                                              $"<pre lang='json'><code><font color='red'>{locator}<font></code></pre>", Status.Fail);
                        Assert.Warn($"Element \"{locator}\" not interactable.");
                    }

                    if (debug != true) throw;
                }
                else if (ex is StaleElementReferenceException)
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep($"Element is stale. Unable to send text. <br>" +
                                              $"<pre lang='json'><code><font color='red'>{locator}<font></code></pre>",
                            Status.Fail);
                        Assert.Warn($"Element \"{locator}\" is stale. Unable to send text.");
                    }

                    if (debug != true) throw;
                }
                else if (ex is NoSuchElementException)
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep($"Element not found. <br>" +
                                              $"<pre lang='json'><code><font color='red'>{locator}<font></code></pre>", Status.Fail);
                        Assert.Warn($"Element \"{locator}\" not found.");
                    }
                    if (debug != true) throw;
                }
                else
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep(ex.ToString(), Status.Fail);
                        Assert.Warn(ex.ToString()); 
                    }
                    if (debug != true) throw;
                }
            }
        }

        public static void EnterTextSlower(this IWebDriver driver, By locator, string value)
        {
            char[] characters = value.ToCharArray();
            foreach (char c in characters)
            {
                driver.FindElement(locator).SendKeys(c.ToString());
                Thread.Sleep(200);
            }
            ExtentManager.LogStep($"Text value was entered to the element. <br>" +
                                  $"<pre lang='json'><code>Value: {value}<br>Locator: {locator}</code></pre>");
        }
        
        public static void Clear(this IWebDriver driver, By locator)
        {
            try
            {
                var webElement = driver.FindElement(locator);
                driver.ScrollElementIntoView(locator);
                webElement.Clear();
            }
            catch (Exception ex)
            {
                ExtentManager.LogStep(ex.ToString(), Status.Fail);
                Assert.Warn(ex.ToString()); 
                throw;

            }
        }

        /// <summary>
        /// Used to click on element.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="timeout">Used for wait for WebElement. Default wait is 0 seconds.</param>
        public static void Click(this IWebDriver driver, By locator, bool logStep = true, bool debug = false, bool screenShot = true, bool js = true)
        {
            try
            {
                try
                {
                    driver.ScrollElementIntoView(locator);
                    driver.FindElement(locator).Click();
                }
                catch
                {
                    driver.JavaScriptClick(locator);
                }
                
                // driver.JavaScriptClick(element);
                ExtentManager.LogStep($"Clicked on the element:" +
                                      $" <br> <pre lang='json'><code>{locator}</code></pre>");
            }
            catch (Exception ex)
            {
                if (screenShot) TakeScreenShot(driver);

                if (ex is ElementClickInterceptedException)
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep($"Element click was intercepted. <br>" +
                                              $"<pre lang='json'><code><font color='red'>{locator}<font></code></pre>", Status.Fail);
                        Assert.Warn($"Element \"{locator}\" click was intercepted.");
                    }

                    if (debug != true) throw;
                } 
                if (ex is ElementNotInteractableException)
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep($"Element is not interactable. <br>" +
                                              $"<pre lang='json'><code><font color='red'>{locator}<font></code></pre>", Status.Fail);                
                        Assert.Warn($"Element \"{locator}\" is not interactable.");
                    }

                    if (debug != true) throw;
                }
                if (ex is StaleElementReferenceException)
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep($"Element is not found. <br>" +
                                              $"<pre lang='json'><code><font color='red'>{locator}<font></code></pre>", Status.Fail);
                        Assert.Warn($"Element \"{locator}\" is not found.");
                    }

                    if (debug != true) throw;
                }
                
                if (ex is NoSuchElementException)
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep($"Element is stale. Unable to click. <br>" +
                                              $"<pre lang='json'><code><font color='red'>{locator}<font></code></pre>", Status.Fail);
                        Assert.Warn($"Element \"{locator}\" is stale. Unable to click.");
                    }

                    if (debug != true) throw;
                }
                else
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep(ex.ToString(), Status.Fail);
                        Assert.Warn(ex.ToString());
                    }
                    if (debug != true) throw; 
                }

            }
        }

        public static void JavaScriptClick(this IWebDriver driver, By locator)
        {
            var webElement = driver.FindElement(locator);
            IJavaScriptExecutor executor = JavaScript(driver);
            executor.ExecuteScript("arguments[0].click();", webElement);
        }

        public static void JavaScriptSetValue(this IWebDriver driver, By locator, string value)
        {
            var webElement = driver.FindElement(locator);
            IJavaScriptExecutor executor = JavaScript(driver);
            executor.ExecuteScript($"arguments[0].setAttribute('value', '{value}')", webElement);
        }
        
        /// <summary>
        /// Used to select value from dropdown.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="value">Used for value that will selected from dropdown.</param>
        /// <param name="timeout">Used for wait for WebElement. Default wait is 0 seconds.</param>
        public static void SelectDropdown(this IWebDriver driver, By locator, string value, bool logStep = true, bool debug = false, bool screenShot = true)
        {
            try
            {
                var webElement = driver.FindElement(locator);
                new SelectElement(webElement).SelectByText(value);
                ExtentManager.LogStep($"Text value was selected from a dropdown of element. <br>" +
                                      $"<pre lang='json'><code>Value: {value}<br>Locator: {locator}</code></pre>");
            }
            catch (Exception ex)
            {
                if (screenShot) TakeScreenShot(driver);

                if (ex is NoSuchElementException)
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep($"Element was not found. Unable to select text value. <br>" +
                                              $"<pre lang='json'><code>Value: {value}<br>Locator: <font color='red'>{locator}<font></code></pre>", Status.Fail);
                        Assert.Warn($"Text value '{value}' of element \"{locator}\" was not found.");
                    }

                    if (debug != true) throw;
                }
                if (ex is StaleElementReferenceException)
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep($"Dropdown element is stale. Unable to select text value. <br>" +
                                              $"<pre lang='json'><code>Value: {value}<br>Locator: <font color='red'>{locator}<font></code></pre>", Status.Fail);
                        Assert.Warn($"Dropdown element \"{locator}\" is stale. Unable to select text value '{value}'.");
                    }
                    if (debug != true) throw;
                }
                if (ex is ElementNotInteractableException)
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep($"Dropdown element is stale. Unable to select text value. <br>" +
                                              $"<pre lang='json'><code>Value: {value}<br>Locator: <font color='red'>{locator}<font></code></pre>'.", Status.Fail);
                        Assert.Warn($"Dropdown element \"{locator}\" is stale. Unable to select text value '{value}'.");
                    } 
                    if (debug != true) throw;
                }
                else
                {
                    if (logStep)
                    {
                        ExtentManager.LogStep(ex.ToString(), Status.Fail);
                        Assert.Warn(ex.ToString());
                    }
                    if (debug != true) throw;
                }
            }
        }
        
        /// <summary>
        /// Method used to check or uncheck checkbox, verifies if checkbox has already attribute "checked" or not
        /// before click.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element"></param>
        /// <param name="check"></param>
        public static void UpdateCheckbox(this IWebDriver driver, By locator, bool check, bool debug = false, bool screenShot = true)
        {
            if (check)
            {
                var isChecked = driver.GetValueByAttribute(locator, Attribute.Checked, logStep: false);
                if (isChecked != "true")
                {
                    driver.Click(locator, debug:debug, screenShot:screenShot);
                }
            }
            else
            {
                var isChecked = driver.GetValueByAttribute(locator, Attribute.Checked, logStep: false);
                if (isChecked == "true")
                {
                    driver.Click(locator, debug:debug, screenShot:screenShot);
                }
            }
        }
        
        /// <summary>
        /// Used to move mouse to the element.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element">Used to pass WebElement ID, Xpath, CSS etc.</param>
        
        public static void HoverOver(this IWebDriver driver, By locator)
        {
            try
            {
                var _element = driver.FindElement(locator);
                var action = new Actions(driver);
                action.MoveToElement(_element).Perform();
                ExtentManager.LogStep($"Hovering over the element: <br>" +
                                      $"<pre lang='json'><code>{locator}</code></pre>");
            }
            catch (NoSuchElementException)
            {
                ExtentManager.LogStep($"Element is not found. <br>" +
                                      $"<pre lang='json'><code><font color='red'>{locator}<font></code></pre>", Status.Fail);
                Assert.Warn($"Element \"{locator}\" is not found.");
            }
            
        }
        
        /// <summary>
        /// Performs drag-and-drop action from one element to another
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="sourceElement"></param>
        /// <param name="targetElement"></param>
        public static void DragAndDrop(this IWebDriver driver, By sourceElement, By targetElement)
        {
            var _sourceElement = driver.FindElement(sourceElement);
            var _targetElement = driver.FindElement(targetElement);
            var action = new Actions(driver);
            action.DragAndDrop(_sourceElement, _targetElement).Perform();
            ExtentManager.LogStep($"Dragged and dropped from element '{_sourceElement}' to element '{_targetElement}'.");
        }

        #endregion

        #region Assertions

        private static IWebElement ReturnElement(this IWebDriver driver, By locator)
        {
            try
            {
                IWebElement element = driver.FindElement(locator);
                return element;
            }
            catch (NoSuchElementException)
            {
                ExtentManager.LogStep($"Element not found:" +
                                      $"<pre lang='json'><code><font color='red'>{locator}<font></code></pre>", Status.Fail);
                Assert.Warn($"Element \"{locator}\" not found.", Status.Fail);
                return null;
            }
        }
        
        /// <summary>
        /// Used to obtain string value from attribute of WebElement.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="attribute">Takes attribute type. Types: name, value, checked, text etc.</param>
        /// <param name="logStep">Used for logging to Reporter.</param>
        /// <returns></returns>
        public static string GetValueByAttribute(this IWebDriver driver, By locator, Attribute attribute, 
            bool logStep = true, bool trim = false)
        {
            var webElement = driver.ReturnElement(locator);
            
            string value;

            if (webElement != null)
            {
                switch (attribute)
                {
                    case Attribute.Name:
                        value = webElement.GetAttribute("name");
                        break;
                    case Attribute.Value:
                        value = webElement.GetAttribute("value");
                        break;
                    case Attribute.Href:
                        value = webElement.GetAttribute("href");
                        break;
                    case Attribute.Disabled:
                        value = webElement.GetAttribute("disabled");
                        break;
                    case Attribute.Class:
                        value = webElement.GetAttribute("class");
                        break;
                    case Attribute.Selected:
                        value = webElement.GetAttribute("selected");
                        break;
                    case Attribute.Checked:
                        value = webElement.GetAttribute("checked");
                        break;
                    case Attribute.Text:
                        value = webElement.Text;
                        break;
                    default:
                        value = null;
                        break;
                }
                if (logStep)
                {
                    ExtentManager.LogStep($"Obtained value from element by attribute <b>{attribute}</b>." +
                                          $"<pre lang='json'><code>Value: {value}<br>Locator: {locator}</code></pre>");
                }
            
                if (trim) value = value.Trim();
                return value;
            }

            return null;

        }
        
        /// <summary>
        /// Used to obtain string value from attribute of WebElement.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="attributeType">Takes attribute type. Types: name, value, checked, text etc.</param>
        /// <param name="logStep">Used for logging to Reporter.</param>
        /// <returns></returns>
        public static string GetValueByAttribute(this IWebDriver driver, By locator, string attributeType, 
            bool logStep = true, bool trim = false)
        {
            var webElement = driver.ReturnElement(locator);

            if (webElement != null)
            {
                 
                var value = webElement.GetAttribute(attributeType);
            
                if (logStep)
                {
                    ExtentManager.LogStep($"Obtained value from element by attribute <b>{attributeType}</b>." +
                                          $"<pre lang='json'><code>Value: {value}<br>Locator: {locator}</code></pre>");
                }
            
                if (trim) value = value.Trim();
                return value;
            }

            return null;
        }

        /// <summary>
        /// Used to assert value and it will not fail the entire session, but will display assertion error in the report.
        /// </summary>
        /// <param name="expectedValue"></param>
        /// <param name="actualValue"></param>
        public static void SoftAssertAreEqual(object expectedValue, object actualValue)
        {
            SoftAssert softAssert = new SoftAssert();
            softAssert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Used to assert value pulled from the attribute of WebElement.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="attributeType">Takes attribute type. Types: name, value, checked, text etc.</param>
        /// <param name="expectedValue">Takes string value that will be asserted.</param>
        /// <param name="stepRecorded">Used for logging to Reporter.</param>
        
        public static void AssertElementByAttribute(this IWebDriver driver, By locator, Attribute attributeType,
            string expectedValue, bool trim = false, bool logStep = true, bool screenShot = true)
        {
            string actualValue = driver.GetValueByAttribute(locator, attributeType, false, trim);

            if (actualValue == null) actualValue = String.Empty;
            
                try
                {
                    Assert.AreEqual(expectedValue, actualValue);
                    if (logStep)
                    {
                        ExtentManager.LogStep(
                            $"Actual value of the attribute <b>{attributeType}</b> of the element is <b>EQUAL</b> to expected. <br>" +
                            $"<pre lang='json'><code>Locator: {locator}</code></pre> <br>" +
                            $"<pre lang='json'><code>" +
                            $"Actual:   {actualValue}<br>" +
                            $"Expected: {expectedValue}</code></pre>");
                    }
                }
                catch (AssertionException)
                {
                    if (logStep)
                    {
                        if (screenShot) TakeScreenShot(driver);

                        ExtentManager.LogStep(
                            $"Actual value of the attribute <b>{attributeType}</b> of the element is <b>NOT EQUAL</b> to expected. <br>" +
                            $"<pre lang='json'><code>Locator: {locator}</code></pre> <br>" +
                            $"<pre lang='json'><code>" +
                            $"Actual:   <font color='red'>{actualValue}</font><br>" +
                            $"Expected: <font color='red'>{expectedValue}</font></code></pre>", Status.Fail);
                        Assert.Warn($"Value of the attribute '{attributeType.ToString()}' of the element \"{locator}\" is " +
                                    $"'{actualValue}' NOT equal to expected '{expectedValue}'.");
                    }
                }
            
            
        }
        
        public static void AssertElementByAttribute(this IWebDriver driver, By locator, string attributeType,
            string expectedValue, bool stepRecorded = true, int timeout = 0, bool trim = false, bool logStep = true, bool screenShot = true)
        {
            string actualValue = driver.GetValueByAttribute(locator, attributeType, false, trim);

            if (actualValue == null) actualValue = String.Empty;
            
                try
                {
                    Assert.AreEqual(expectedValue, actualValue);
                    if (logStep)
                    {
                        ExtentManager.LogStep(
                            $"Actual value of the attribute <b>{attributeType}</b> of the element is <b>EQUAL</b> to expected. <br>" +
                            $"<pre lang='json'><code>Locator: {locator}</code></pre> <br>" +
                            $"<pre lang='json'><code>" +
                            $"Actual:   {actualValue}<br>" +
                            $"Expected: {expectedValue}</code></pre>");
                    }
                }
                catch (AssertionException)
                {
                    if (logStep)
                    {
                        if (screenShot) TakeScreenShot(driver);

                        ExtentManager.LogStep(
                            $"Actual value of the attribute <b>{attributeType}</b> of the element is <b>NOT EQUAL</b> to expected. <br>" +
                            $"<pre lang='json'><code>Locator: {locator}</code></pre> <br>" +
                            $"<pre lang='json'><code>" +
                            $"Actual:   <font color='red'>{actualValue}</font><br>" +
                            $"Expected: <font color='red'>{expectedValue}</font></code></pre>", Status.Fail);
                        Assert.Warn($"Value of the attribute '{attributeType.ToString()}' of the element \"{locator}\" is " +
                                    $"'{actualValue}' NOT equal to expected '{expectedValue}'.");
                    }
                }
            
        }

        /// <summary>
        /// Used to assert title.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="expectedTitle">Used to specify string which should be asserted.</param>
        public static void AssertTitle(this IWebDriver driver, string expectedTitle, bool logStep = true)
        {
            var actualTitle = driver.Title;
            
            try
            {
                Assert.AreEqual(expectedTitle, actualTitle);
                if (logStep)
                {
                    ExtentManager.LogStep($"The title is correct. <br>" +
                                          $"<pre lang='json'><code>" +
                                          $"Actual Title:   {actualTitle}<br>" +
                                          $"Expected Title: {expectedTitle}</code></pre>");;
                }
            }
            catch (AssertionException)
            {
                if (logStep)
                {
                    Assert.Warn($"Title is incorrect. Actual value '{actualTitle}' is " +
                                          $"different from expected '{expectedTitle}'.", Status.Fail);
                    ExtentManager.LogStep($"The title is incorrect. <br>" +
                                          $"<pre lang='json'><code>" +
                                          $"Actual Title:   <font color='red'>{actualTitle}<font><br>" +
                                          $"Expected Title: <font color='red'>{expectedTitle}<font></code></pre>", Status.Fail);
                }
            }
        }

        /// <summary>
        /// Primarily used for checkboxes and radiobuttons. Asserts if element is checked.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element">Used to pass WebElement ID, Xpath, CSS etc.</param>
        
        /// <param name="stepRecorded">Used for logging to Reporter.</param>
        /// <param name="expectedValue">Used to pass or fail test if element is not checked.</param>
        public static void AssertElementChecked(this IWebDriver driver, By locator, bool expectedValue = true, bool screenShot = true)
        {

            var actualValue = driver.GetValueByAttribute(locator, Attribute.Checked, false);
            var actualTextValue = actualValue == "true" ? "CHECKED" : "NOT CHECKED";
            
            string expectedTextValue = expectedValue ? "CHECKED" : "NOT CHECKED";

            try
            {
                Assert.AreEqual(expectedTextValue, actualTextValue);
                ExtentManager.LogStep($"Checkbox is <b>{actualTextValue}</b> as expected. <br>" +
                                      $"<pre lang='json'><code>Locator: {locator}</code></pre>");
            }
            catch (AssertionException)
            {
                if (screenShot) TakeScreenShot(driver);

                ExtentManager.LogStep($"Checkbox is <b>{actualTextValue}</b> and should be <b>{expectedTextValue}</b>. <br>" +
                                      $"<pre lang='json'><code>Locator: <font color='red'>{locator}<font></code></pre>", Status.Fail);
            }
        }

        /// <summary>
        /// Primarily used for checkboxes and radiobuttons. Asserts if element is checked.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="stepRecorded">Used for logging to Reporter.</param>
        /// <param name="expectedValue">Used to pass or fail test if element is not checked.</param>
        public static void AssertElementText(this IWebDriver driver, By locator, string expectedValue, bool trim = false, bool screenShot = true)
        {
            string actualValue = driver.GetValueByAttribute(locator, Attribute.Text, true, trim:trim);

            if (actualValue == null) actualValue = String.Empty; 

                try
                {
                    Assert.AreEqual(expectedValue, actualValue);
                    ExtentManager.LogStep(
                            $"Text value of element is <b>EQUAL</b> to expected. <br>" +
                            $"<pre lang='json'><code>Locator: {locator}</code></pre> <br>" +
                            $"<pre lang='json'><code>" +
                            $"Actual:   {actualValue}<br>" +
                            $"Expected: {expectedValue}</code></pre>");
                }
                catch (AssertionException)
                {
                    if (screenShot) TakeScreenShot(driver);

                    ExtentManager.LogStep(
                        $"Text value of element is <b>NOT EQUAL</b> to expected. <br>" +
                        $"<pre lang='json'><code>Locator: {locator}</code></pre> <br>" +
                        $"<pre lang='json'><code>" +
                        $"Actual:   <font color='red'>{actualValue}</font><br>" +
                        $"Expected: <font color='red'>{expectedValue}</font></code></pre>", Status.Fail);
                }
            

        }
        
        private static string RemoveDoubleSpaces(string value)
        {
            try
            {
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);     
                return regex.Replace(value, " ");
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Used to assert string pulled from the attribute of WebElement containing text.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element">Used to pass WebElement ID, Xpath, CSS etc.</param>
        /// <param name="expectedValue">Takes string value that will be asserted.</param>
        /// <param name="toLower">Makes text all lower case if set to true.</param>
        public static void AssertElementContainsText(this IWebDriver driver, By locator, string expectedValue, bool toLower = false)
        {
            string textValue;

            textValue = toLower ? driver.GetValueByAttribute(locator, Attribute.Text, false).ToLower() 
                : driver.GetValueByAttribute(locator, Attribute.Text, false);
            
            textValue = RemoveDoubleSpaces(textValue);
     

            if (textValue!=null) textValue = String.Empty;
            
                try
                {
                    Assert.That(textValue, Does.Contain(expectedValue));
                    ExtentManager.LogStep($"Element <b>CONTAINS</b> expected value. <br>" +
                                          $"<pre lang='json'><code>Value: {expectedValue}<br>Locator: {locator}</code></pre>");
                }
                catch (AssertionException)
                {
                    ExtentManager.LogStep($"Element does <b>NOT CONTAIN</b> value." +
                                          $"<pre lang='json'><code>Value: <font color='red'>{expectedValue}<font>" +
                                          $"<br>Locator: {locator}</code></pre>" +
                                          $"<pre lang='json'><br>Actual Text: {textValue}</code></pre>", Status.Fail);
                }
            
        }
        
        public static void AssertDropdownById(this IWebDriver driver, string elementId, string value)
        {
            driver.AssertElementByAttribute(By.XPath($"//select[@id='{elementId}']/child::option[text()='{value}']"), 
                Attribute.Selected, "true");
        }
        
        public static void AssertDropdownByXpath(this IWebDriver driver, string xpath, string value)
        {
            driver.AssertElementByAttribute(By.XPath($"{xpath}/child::option[text()='{value}']"), 
                Attribute.Selected, "true");
        }


        public static void AssertDropdownContents(this IWebDriver driver, By locator, List<string> dList, bool sort = true)
        {
            try
            {
                var webElement = driver.FindElement(locator);
                var select = new SelectElement(webElement);
                IList<IWebElement> listOptions = select.Options;
                
                List<string> optionText = new List<string>();
                List<string> reportActual = new List<string>();
                List<string> reportExpected = new List<string>();
                
                foreach (var option in listOptions)
                {
                    var x = option.Text;
                    optionText.Add(x);
                    if (x == "") continue;
                    var y = "<li>" + x + "</li>";
                    reportActual.Add(y);
                }

                foreach (var option in dList)
                {
                    if (option == "") continue;
                    var y = "<li>" + option + "</li>";
                    reportExpected.Add(y);
                }
                
                dList.Sort();
                if (sort) optionText.Sort();

                var expectedList = string.Join(", ", dList.ToArray());
                var actualList = string.Join(", ", optionText.ToArray());

                var reportActualList = string.Join("", reportActual.ToArray());
                var reportExpectedList = string.Join("", reportExpected.ToArray());

                try
                {
                    CollectionAssert.AreEqual(expected: dList, actual: optionText);
                    ExtentManager.LogStep(
                        $"Dropdown options of element <b>ARE EQUAL</b> to expected options. <br>" +
                        $"<pre lang='json'><code>Locator: {locator}</code></pre> <br>" +
                        $"<pre lang='json'><code>Actual:   {reportActualList}</code></pre> <br>" +
                        $"<pre lang='json'><code>Expected: {reportExpectedList}</code></pre>");
                }
                catch (AssertionException)
                {
                    ExtentManager.LogStep(
                        $"Dropdown options of element <b>ARE NOT EQUAL</b> to expected options. <br>" +
                        $"<pre lang='json'><code>Locator: {locator}</code></pre> <br>" +
                        $"<pre lang='json'><code>Actual:   <font = 'red'>{reportActualList}<font></code></pre> <br>" +
                        $"<pre lang='json'><code>Expected: <font = 'red'>{reportExpectedList}<font></code></pre>", Status.Fail);
                    Assert.Warn($"Dropdown options '{expectedList}' of element \"{locator}\" are NOT equal to expected values '{actualList}'.");
                }
            }
            catch (Exception e)
            {
                ExtentManager.LogStep($"Unable to collect values from dropdown. See details below. <br>" +
                                      $"<pre lang='json'><code>Locator: <font color='red'>{locator}<font></code></pre>", Status.Fail);
                Assert.Warn(e.ToString());
            }
        }
        
        #endregion

        
        #region Utils

        /// <summary>
        /// Scroll to the element.
        /// </summary>
        /// <param name="driver"></param>
        public static void ScrollElementIntoView(this IWebDriver driver, By locator)
        {
            try
            {
                var _element = driver.FindElement(locator);
                String scrollElementIntoMiddle = "var viewPortHeight = Math.max(document.documentElement.clientHeight, window.innerHeight || 0);"
                                                 + "var elementTop = arguments[0].getBoundingClientRect().top;"
                                                 + "window.scrollBy(0, elementTop-(viewPortHeight/2));";
                driver.JavaScript().ExecuteScript(scrollElementIntoMiddle, _element);
            }
            catch (NoSuchElementException ex)
            {
                ExtentManager.LogStep("Unable to scroll to element. <br>" +
                                      $"<pre lang='json'><code><font color='red'>{locator}<font></code></pre>", Status.Fail);
                Assert.Warn(ex.ToString());
            }
            
        }

        /// <summary>
        /// Used to switch between browser windows.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="switchWindow">Switch between windows. Set to True to switch to the last window/
        /// False to switch to First window.</param>
        public static void SwitchWindow(this IWebDriver driver, bool switchWindow)
        {
            driver.SwitchTo().Window(switchWindow ? driver.WindowHandles.Last() : driver.WindowHandles.First());
        }
        
        public static void SwitchWindow(this IWebDriver driver, int windowNum)
        {
            driver.SwitchTo().Window(driver.WindowHandles[windowNum]);
        }
        
        public static void SwitchWindow(this IWebDriver driver, string windowName)
        {
            driver.SwitchTo().Window(windowName);
        }

        /// <summary>
        /// Enables JS Executor.
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public static IJavaScriptExecutor JavaScript(this IWebDriver driver)
        {
            return (IJavaScriptExecutor)driver;
        }

        public static void UpdateImplicitTimeOut(this IWebDriver driver, int timeout)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(timeout);
        }

        public static void Refresh(this IWebDriver driver, bool hard = false)
        {
            if (hard)
                driver.JavaScript().ExecuteScript("location.reload(true);");
            else
            {
                driver.Navigate().Refresh();
            }
        }

        public static void SwitchToIframe(this IWebDriver driver, By locator, bool logStep = false)
        {
            var iframeElement = driver.FindElement(locator);
            driver.SwitchTo().Frame(iframeElement);
            if (logStep)
            {
                ExtentManager.LogStep($"Switching to iFrame \"{locator}\".");
            }
        }

        public static void SwitchToMainContent(this IWebDriver driver, bool logStep = false)
        {
            driver.SwitchTo().DefaultContent();
            if (logStep)
            {
                ExtentManager.LogStep("Switching back to main content.");

            }
        }

        public static void AssertCurrentPageUrl(this IWebDriver driver, string expectedValue, bool logStep)
        {
            string actualValue = driver.Url;
          

            if (actualValue != null)
            {
                try
                {
                    Assert.That(actualValue, Does.Contain(expectedValue));
                    ExtentManager.LogStep(
                            $"Current URL '{actualValue}' is equal to expected URL " +
                            $"value '{expectedValue}'.");
                }
                catch (AssertionException)
                {
                    ExtentManager.LogStep(
                        $"Current URL '{actualValue}' is NOT equal to expected URL " +
                        $"value '{expectedValue}'.", Status.Fail);
                }
            }
        }
        #endregion

        #region Unused
        
        [Obsolete("Not to be used.")]
        public static void IframeClick(this IWebDriver driver, By locator, By iframe)
        {
            var iframeElement = driver.FindElement(iframe);
            driver.SwitchTo().Frame(iframeElement);
            Thread.Sleep(1000);
            driver.FindElement(locator).Click();
            driver.SwitchTo().DefaultContent();
            ExtentManager.LogStep("Clicked iFrame element: '" + locator + "'");
        }

        [Obsolete("Not to be used.")]
        public static void IframeSendKeys(this IWebDriver driver, By locator, string value, By iframe)
        {
            var iframeElement = driver.FindElement(iframe);
            driver.SwitchTo().Frame(iframeElement);
            Thread.Sleep(1000);
            driver.FindElement(locator).SendKeys(value);
            driver.SwitchTo().DefaultContent();
            ExtentManager.LogStep("'" + value + "' value was entered to iFrame element '" + locator + "'");
        }
        
        #endregion

        #region AxeReport

        private static void AxeShouldBeEmpty(AxeResult axeResult)
        {
            AssertionOptions.FormattingOptions.MaxDepth = 10;
            AssertionOptions.FormattingOptions.MaxLines = 200;
            axeResult.Violations.Should().BeEmpty();
        }

        public static void AxeFullPageAnalysis(this IWebDriver driver, ReportTypes reportTypes = ReportTypes.All)
        {
            string axePath = ExtentManager.reportPath + $"/AxeReport{DataGenerators.GetDate("yyyymmdd-HHmmss")}.html";
            AxeResult axeResult = new AxeBuilder(driver).Analyze();

            driver.CreateAxeHtmlReport(axeResult, axePath, reportTypes);
            TestContext.AddTestAttachment(axePath);
            
            var relPath = DirectoryManager.MakeRelative(axePath, ExtentManager.reportPath);
            
            try
            {
                AxeShouldBeEmpty(axeResult);
                ExtentManager.LogStep($"Full page accessibility report was generated: <br> <a href='{relPath}'>Click here to see AxeReport</a>");
            }
            catch (Exception ex)
            {
                Assert.Warn($"Accessibility violation was found.");
                ExtentManager.LogStep($"<font size ='+1'>Violation was found: <br> <a href='{relPath}'>Click here to see AxeReport</a></font>", Status.Fail);
            }
        }

        public static void AxeAnalyzeElement(this IWebDriver driver, By locator, ReportTypes reportTypes = ReportTypes.All)
        {
            string axePath = ExtentManager.reportPath + $"AxeReport{DataGenerators.GetDate("yyyymmdd-HHmmss")}.html";

            IWebElement element = driver.FindElement(locator);
            AxeResult axeResult = new AxeBuilder(driver)
                .Analyze(element);
            
            var relPath = DirectoryManager.MakeRelative(axePath, ExtentManager.reportPath);
            
            driver.CreateAxeHtmlReport(axeResult, axePath, reportTypes);
            TestContext.AddTestAttachment(axePath);

            try
            {
                AxeShouldBeEmpty(axeResult);
                ExtentManager.LogStep(
                    $"Accessibility report for a weblement was generated: <br> <a href='{relPath}'>Click here to see AxeReport</a>" +
                    $"<br> <pre lang='json'><code>{locator}</code></pre>");
            }
            catch (Exception ex)
            {
                Assert.Warn($"Accessibility violation was found.");
                ExtentManager.LogStep(
                    $"<font size ='+1'>Violation was found: <br> <a href='{relPath}'>Click here to see AxeReport</a></font>" +
                    $"<br> <pre lang='json'><code><font color='red'>{locator}<font></code></pre>",
                    Status.Fail);
            }
        }
        
        #endregion
    }
}