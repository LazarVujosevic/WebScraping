using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraping.Shared;

namespace WebScraping.Classes
{
    public static class Logger
    {
        public static void Log(LoggerTypesEnum type, string message)
        {
            if (type == LoggerTypesEnum.Info)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[INFO] ");
            }
            else if (type == LoggerTypesEnum.Warning)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[WARNING] ");
            }
            else if (type == LoggerTypesEnum.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR] ");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{message}\n");
        }
    }
}
