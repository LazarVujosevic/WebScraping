using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
    public class PageNavigation
    {
        private string _navigationUrl;

        private IWebDriver driver;

        private string _selectedCurrency;

        public PageNavigation(string navigationUrl)
        {
            this._navigationUrl = navigationUrl;
            this.driver = new ChromeDriver();
            var options = new ChromeOptions();
            options.AddArgument("no-sandbox");
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
                this.DoWhatEver(i);
            }
        }

        private void DoWhatEver(int currencyId)
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

            if (this.CheckDoesElementExists(By.ClassName("nav_pagenum")))
            {
                int numberOfPages = Int32.Parse(driver.FindElement(By.ClassName("nav_pagenum")).GetAttribute("innerText"));
                if (numberOfPages != 0 && numberOfPages > 1)
                {
                    int currentPageNumber = 1;
                    while (currentPageNumber < numberOfPages - 1)
                    {
                        currentPageNumber++;
                        ((IJavaScriptExecutor)driver).ExecuteScript($"PageContext.PageNav.go({currentPageNumber},{numberOfPages})");
                        this.GetdataFromTable(csv, false, currentPageNumber);
                    }

                    ((IJavaScriptExecutor)driver).ExecuteScript($"PageContext.PageNav.goLast()");
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
