using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace visualstudioselenium
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //create instance for reporting 
            var HTMLReport = new ExtentHtmlReporter("extentReport_" + DateTime.Now + ".html");
            var extent = new ExtentReports();
            extent.AttachReporter(HTMLReport);
            extent.AddSystemInfo("Operating System", Environment.Is64BitOperatingSystem.ToString());

            // Create a driver instance for chromedriver
            IWebDriver driver = new ChromeDriver();

            //Add step for reporting
            var test = extent.CreateTest("ExtentTest");

            //Navigate to google page
            driver.Navigate().GoToUrl("http:www.google.com");
            test.Log(Status.Info,"driver navigate OK");
            test.Log(Status.Pass, "Step 1: openBrowserOK");
            extent.Flush();


            //Close the browser.
            driver.Close();
            //driver.Quit();

        }
    }
}

