﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using Environment = System.Environment;

namespace SeleniumActions
{
    public class Browser
    {
        private static Browser Instance;
        private readonly Hashtable pool = new Hashtable();
        //CSS types that we care
        private string CSSXPath = Configuration.Settings("CSSType",
                                                         "//*[@class='ROW1' or @class='ROW2' or @class='' or @class='' or @class='' or @class='' or @class='' or @class='' or @class='' or @class='' or @class='' or @class='']");

        private IWebDriver browser;

        private Browser()
        {
        }

        public static Browser GetInstance()
        {
            return Instance ?? (Instance = new Browser());
        }

        public IWebDriver GetCurrentBrowser()
        {
            if (browser == null)
            {
                CloseBrowser();
                StartBrowser();
            }
            DismissUnexpectedAlert();
            ReadOnlyCollection<string> bs = browser.WindowHandles;
            if (bs.Count == 1)
            {
                browser.SwitchTo().Window(bs[0]);
            }
            else
            {
                browser.SwitchTo().Window(bs[bs.Count - 1]);
            }

            return browser;
        }

        public IWebDriver SwitchToAnotherBrowser()
        {
            if (browser == null)
            {
                CloseBrowser();
                StartBrowser();
            }
            DismissUnexpectedAlert();
            ReadOnlyCollection<string> bs = browser.WindowHandles;
            if (bs.Count == 1)
            {
                browser.SwitchTo().Window(bs[0]);
                return browser;
            }
            string currentHandle = browser.CurrentWindowHandle;
            foreach (string handle in bs)
            {
                if (!currentHandle.Equals(handle))
                {
                    browser.SwitchTo().Window(handle);
                    break;
                }
            }

            return browser;
        }

        //CSStype subpool<fingerprint string, webelement>

        public void Prepare()
        {
        }

        public void Clear()
        {
            foreach (object item in pool.Values)
            {
                if (item.GetType().Name.Contains("Hashtable"))
                {
                    ((Hashtable) item).Clear();
                }
            }
            pool.Clear();
        }

        public IWebElement GetWebElement(string tag, params string[] key_value)
        {
            //TODO get object from object pool? Multithread?
            return null;
        }

        public string Snapshot()
        {
            return ((ITakesScreenshot) GetCurrentBrowser()).GetScreenshot().AsBase64EncodedString;
            //IJavaScriptExecutor js = GetCurrentBrowser() as IJavaScriptExecutor;
            //Response screenshotResponse = js.ExecuteScript(DriverCommand.Screenshot, null);
            //return screenshotResponse.Value.ToString();
        }

        private void DismissUnexpectedAlert()
        {
            IAlert alert = GetAlert();
            if (alert != null)
            {
                alert.Dismiss();
                Logger.GetInstance().Log().Warn("Close an unexpected dialog, please check.");
            }
        }

        private IAlert GetAlert()
        {
            IAlert alert = null;
            try
            {
                alert = browser.SwitchTo().Alert();
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log().Debug("Suppress get alert");
            }
            return alert;
        }

        private void StartBrowser()
        {
            string browserType = Configuration.Settings("BrowserType", "IE");
            if (browserType.Equals("IE"))
                browser = new InternetExplorerDriver();
            if (browserType.Equals("Firefox"))
                browser = new FirefoxDriver();
            if (browserType.Equals("Chrome"))
                browser = new ChromeDriver();
            browser.Navigate().GoToUrl(Configuration.Settings("DefaultURL", "about:blank"));
            MaximiseBrowser();
        }

        private void MaximiseBrowser()
        {
            browser.Manage().Window.Maximize();
        }

        public void CloseBrowser()
        {
            if (browser != null)
                browser.Quit();
            browser = null;

            string browserType = Configuration.Settings("BrowserType", "IE");
            if (browserType.Equals("IE"))

                DosCommand(Environment.SystemDirectory + "\\taskkill.exe", " /IM iexplore.exe");
            if (browserType.Equals("Firefox"))
                DosCommand(Environment.SystemDirectory + "\\taskkill.exe", " /IM firefox.exe");
            if (browserType.Equals("Chrome"))
                DosCommand(Environment.SystemDirectory + "\\taskkill.exe", " /IM chrome.exe");
        }

        private static void DosCommand(string cmd, string param)
        {
            var proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.Arguments = param;
            proc.StartInfo.RedirectStandardError = false;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            proc.WaitForExit();
        }
    }
}