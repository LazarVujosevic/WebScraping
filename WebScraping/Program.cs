using System;
using WebScraping.Classes;
using WebScraping.Shared;

namespace WebScraping
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log(LoggerTypesEnum.Info, "Application Started");
           
            try
            {
                var navigation = new ScrapingController("https://srh.bankofchina.com/search/whpj/searchen.jsp");
                navigation.NavigateToPageAndFillData();
            }
            catch (Exception ex)
            {
                Logger.Log(LoggerTypesEnum.Error, $"Navigation Fail, exception : {ex.Message}");
            }
            
        }
    }
}
