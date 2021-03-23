using HtmlAgilityPack;
using System;
using System.IO;
using System.Text;
using WebScraping.Shared;

namespace WebScraping.Classes
{
    public class ScrapingController
    {
        #region Fields

        private string _navigationUrl;

        private HtmlWeb _web;

        private HtmlDocument _doc;

        #endregion

        public ScrapingController(string navigationUrl)
        {
            this._navigationUrl = navigationUrl;
            this._web = new HtmlWeb();
            this._doc = _web.Load(this._navigationUrl);
        }

        public void NavigateToPageAndFillData()
        {
            foreach (HtmlNode node in _doc.DocumentNode.SelectNodes(WellKnownValues.CurrenciesSelect))
            {
                if (node.InnerText != WellKnownValues.SelectTheCurrentyOption)
                {
                    this.GetDataForCurrency(node.InnerText);
                }
            }
        }

        private void GetDataForCurrency(string value)
        {
            var csv = new StringBuilder();
            var document = _web.Load($"{this._navigationUrl}?{WellKnownValues.CurrenciesFieldId}={value}&{WellKnownValues.StartDateId}={DateTime.Now.AddDays(-2).ToString(WellKnownValues.DateInputFormat)}&{WellKnownValues.EndDateId}={DateTime.Now.ToString(WellKnownValues.DateInputFormat)}");
            this.GetdataFromTable(csv, document, value);

            #region Bonus Points

            
            if (csv.ToString() != WellKnownValues.NoRecordsMessage)
            {
                int pageNumber = 2;
                string previousPageHtml = document.Text;
                var documentNewPage = _web.Load($"{this._navigationUrl}?{WellKnownValues.CurrenciesFieldId}={value}&{WellKnownValues.StartDateId}={DateTime.Now.AddDays(-2).ToString(WellKnownValues.DateInputFormat)}&{WellKnownValues.EndDateId}={DateTime.Now.ToString(WellKnownValues.DateInputFormat)}&page={pageNumber}");
                while (previousPageHtml != documentNewPage.Text)
                {
                    this.GetdataFromTable(csv, documentNewPage, value, false, pageNumber);
                    previousPageHtml = documentNewPage.Text;
                    pageNumber++;
                    documentNewPage = _web.Load($"{this._navigationUrl}?{WellKnownValues.CurrenciesFieldId}={value}&{WellKnownValues.StartDateId}={DateTime.Now.AddDays(-2).ToString(WellKnownValues.DateInputFormat)}&{WellKnownValues.EndDateId}={DateTime.Now.ToString(WellKnownValues.DateInputFormat)}&page={pageNumber}");
                }
            }

            #endregion

            try
            {
                var startDateColumnValue = DateTime.Now.AddDays(-2).ToString(WellKnownValues.DateFormatForSaveFile);
                var endDateColumnValue = DateTime.Now.ToString(WellKnownValues.DateFormatForSaveFile);
                var fileName = $"{startDateColumnValue}-{endDateColumnValue}-{value}";
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

        private void GetdataFromTable(StringBuilder csv, HtmlDocument document, string value, bool getHeaderFromTable = true, int pageNumber = 1)
        {
            var table = document.DocumentNode.SelectSingleNode(WellKnownValues.TableForScraping);
            var rows = table.SelectNodes(WellKnownValues.TrElement);
            Logger.Log(LoggerTypesEnum.Info, $"Started Getting data for currency {value}, page number: {pageNumber}");
            int rowNumber = 0;
            foreach (var row in rows)
            {
                if (!getHeaderFromTable && rowNumber == 0)
                {
                    rowNumber++;
                    continue;
                }

                string rowString = string.Empty;
                var rowTds = row.SelectNodes(WellKnownValues.TdElement);

                foreach (var td in rowTds)
                {
                   rowString += $"{td.InnerText},";
                }

                if (rowString.Length > 0)
                {
                    rowString = rowString.Remove(rowString.Length - 1, 1);
                }

                csv.AppendLine(rowString);
                rowNumber++;
            }
        }        
    }
}
