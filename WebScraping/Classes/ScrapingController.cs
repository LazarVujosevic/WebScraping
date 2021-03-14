using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebScraping.Shared;

namespace WebScraping.Classes
{
    public class ScrapingController
    {
        private string _navigationUrl;

        private IWebDriver driver;

        private string _selectedCurrency;

        private bool _isFirefoxBrowser;

        public ScrapingController(string navigationUrl)
        {
            this._navigationUrl = navigationUrl;
            this.SetDriverForBrowser();
        }

        private void SetDriverForBrowser()
        {
            try
            {
                Logger.Log(LoggerTypesEnum.Info, $"Trying to create Chrome Web Driver");
                this.driver = new ChromeDriver();
                var options = new ChromeOptions();
                options.AddArgument("no-sandbox");
                Logger.Log(LoggerTypesEnum.Info, $"Successfully created Chrome Web Driver");
            }
            catch (Exception ex)
            {
                Logger.Log(LoggerTypesEnum.Error, $"Creating Chrome Web Driver Failed, message: {ex.Message}");
                try
                {
                    Logger.Log(LoggerTypesEnum.Info, $"Trying to create Firefox Web Driver");
                    FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();
                    service.Host = "::1";
                    this.driver = new FirefoxDriver(service);
                    Logger.Log(LoggerTypesEnum.Info, $"Successfully created Chrome Web Driver");
                    this._isFirefoxBrowser = true;
                }
                catch (Exception exeption)
                {
                    Logger.Log(LoggerTypesEnum.Error, $"Creating Firefox Web Driver Failed, message: {exeption.Message}");
                }
            }
        }

        public void NavigateToPageAndFillData()
        {
            this.driver.Navigate().GoToUrl(this._navigationUrl);
            this.driver.FindElement(By.Name(WellKnownValues.StartDateFieldName)).SendKeys(DateTime.Now.AddDays(-2).ToString(WellKnownValues.DateInputFormat));
            this.driver.FindElement(By.Name(WellKnownValues.EndDateFieldName)).SendKeys(DateTime.Now.ToString(WellKnownValues.DateInputFormat));
            var currencies = driver.FindElement(By.XPath(WellKnownValues.CurrenciesField));
            SelectElement selectList = new SelectElement(currencies);
            IList<IWebElement> options = selectList.Options;

            for (int i = 1; i < options.Count(); i++)
            {
                this.GetDataForCurrency(i);
            }
        }

        private void GetDataForCurrency(int currencyId)
        {
            var csv = new StringBuilder();
            var currencies = driver.FindElement(By.XPath(WellKnownValues.CurrenciesField));
            SelectElement selectList1 = new SelectElement(currencies);
            IList<IWebElement> options1 = selectList1.Options;
            this._selectedCurrency = options1[currencyId].Text;
            selectList1.SelectByText(this._selectedCurrency);
            var button = driver.FindElement(By.XPath(WellKnownValues.Button));

            button.Click();
            this.GetdataFromTable(csv);

            #region Bonus Points

            if (this.CheckDoesElementExists(By.ClassName(WellKnownValues.NumberOfPages)))
            {
                int numberOfPages = Int32.Parse(driver.FindElement(By.ClassName(WellKnownValues.NumberOfPages)).GetAttribute("innerText"));
                if (numberOfPages != 0 && numberOfPages > 1)
                {
                    int currentPageNumber = 1;
                    while (currentPageNumber < numberOfPages - 1)
                    {
                        currentPageNumber++;
                        ((IJavaScriptExecutor)driver).ExecuteScript($"PageContext.PageNav.go({currentPageNumber},{numberOfPages})");
                        if (this._isFirefoxBrowser)
                        {
                            Thread.Sleep(1000);
                        }
                        this.GetdataFromTable(csv, false, currentPageNumber);
                    }

                    ((IJavaScriptExecutor)driver).ExecuteScript($"PageContext.PageNav.goLast()");
                    if (this._isFirefoxBrowser)
                    {
                        Thread.Sleep(1000);
                    }
                    this.GetdataFromTable(csv, false, numberOfPages);
                }
            }

            #endregion

            try
            {
                var startDateColumnValue = driver.FindElement(By.Name(WellKnownValues.StartDateFieldName)).GetAttribute("value");
                var endDateColumnValue = driver.FindElement(By.Name(WellKnownValues.EndDateFieldName)).GetAttribute("value");
                var fileName = $"{startDateColumnValue.Replace('-', '.')}-{endDateColumnValue.Replace('-', '.')}-{this._selectedCurrency}";
                Logger.Log(LoggerTypesEnum.Info, $"Started Saving file {fileName}.csv");
                this.SaveCsvFile(csv.ToString(), fileName);
                Logger.Log(LoggerTypesEnum.Info, $"Sucessfully saved file {fileName}.csv");
            }
            catch (Exception ex)
            {
                Logger.Log(LoggerTypesEnum.Error, $"Error saving file, exception message:{ex.Message}");
            }
        }

        private void SaveCsvFile(string csv, string fileName)
        {
            var filePath = System.Configuration.ConfigurationManager.AppSettings["FileSaveLocation"];
            File.WriteAllText($@"{filePath}{fileName}.csv", csv.ToString());
        }

        public void Dispose()
        {
            this.driver.Close();
        }

        private void GetdataFromTable(StringBuilder csv, bool getHeaderFromTable = true, int pageNumber = 1)
        {
            var table = driver.FindElement(By.XPath(WellKnownValues.TableForScraping));
            var rows = table.FindElements(By.TagName("tr"));
            Logger.Log(LoggerTypesEnum.Info, $"Started Getting data for currency {this._selectedCurrency}, page number: {pageNumber}");
            int rowNumber = 0;
            foreach (var row in rows)
            {
                 if (!getHeaderFromTable && rowNumber == 0)
                {
                    rowNumber++;
                    continue;
                }
                string rowString = string.Empty;
                var rowTds = row.FindElements(By.TagName("td"));
                foreach (var td in rowTds)
                {
                    if (td.Text != WellKnownValues.WrongWordSumbitMessage)
                    {
                        rowString += $"{td.Text},";
                    }
                }
                if (rowString.Length > 0)
                {
                    rowString = rowString.Remove(rowString.Length - 1, 1);
                }
                csv.AppendLine(rowString);
                rowNumber++;
            }
        }

        private bool CheckDoesElementExists(By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
}
