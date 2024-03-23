using System.Linq;
using AventStack.ExtentReports;
using AventStack.ExtentReports.MarkupUtils;
using AventStack.ExtentReports.Reporter;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using SharpCompress;
using TestSharp.Source.Extensions;
using TestSharp.Source.Reports;
using TestSharp.Source.Utilities;

namespace TestSharp.Source.Reporter
{
public class ExtentManager
    {
        private static readonly Lazy<ExtentReports> _lazy = new Lazy<ExtentReports>(() => new ExtentReports());
        public static string reportPath;
        public static ExtentReports Instance => _lazy.Value;

        static ExtentManager()
        {
            reportPath = DirectoryManager.GetBinDirectory() + @"\Reports\";
            var htmlReporter = new ExtentHtmlReporter(reportPath);
            htmlReporter.LoadConfig(DirectoryManager.GetBinDirectory() + @"\extent-config.xml");
            Instance.AttachReporter(htmlReporter);
        }

        public static void FinishReport(IWebDriver driver)
        {
            var status = TestContext.CurrentContext.Result.Outcome.Status;
            var stacktrace = string.IsNullOrEmpty(TestContext.CurrentContext.Result.StackTrace)
                ? ""
                : string.Format("<pre>{0}</pre>", TestContext.CurrentContext.Result.StackTrace);
            var errorMessage = string.IsNullOrEmpty(TestContext.CurrentContext.Result.Message)
                ? ""
                : string.Format("<pre>{0}</pre>", TestContext.CurrentContext.Result.Message);
            var extentTest = ExtentTestManager.GetTest();
            Status logStatus;

            switch (status)
            {
                case TestStatus.Failed:
                    logStatus = Status.Fail;
                    driver.TakeScreenShot();
                    extentTest.Log(logStatus, "Test error " + errorMessage);
                    break;

                case TestStatus.Inconclusive:
                    logStatus = Status.Warning;
                    break;

                case TestStatus.Skipped:
                    logStatus = Status.Skip;
                    break;
                case TestStatus.Passed:
                    logStatus = Status.Pass;
                    break;
                default:
                    logStatus = Status.Fail;
                    driver.TakeScreenShot();
                    extentTest.Log(logStatus, "Test error " + errorMessage);
                    break;
            }

            extentTest.Log(logStatus, "Test ended with " + logStatus + stacktrace);
        }

        public static void LogStep(string value, Status status = Status.Info)
        {
            ExtentTestManager.GetTest().Log(status, value);
        }
        
        public static void LogStep(IMarkup value, Status status = Status.Info)
        {
            ExtentTestManager.GetTest().Log(status, value);
        }
    }
}