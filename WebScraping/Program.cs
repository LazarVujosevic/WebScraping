using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraping.Classes;
using WebScraping.Shared;

namespace WebScraping
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log(LoggerTypesEnum.Info, "Application Started");
            var navigation = new PageNavigation("https://srh.bankofchina.com/search/whpj/searchen.jsp");
            try
            {
                navigation.NavigateToPageAndFillData();
            }
            catch (Exception ex)
            {
                Logger.Log(LoggerTypesEnum.Error, $"Navigation Fail, exception : {ex.Message}");
            }
            finally
            {
                Logger.Log(LoggerTypesEnum.Info, "Dispose Started");
                navigation.Dispose();
                Logger.Log(LoggerTypesEnum.Info, "Dispose Finished");
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }
}
