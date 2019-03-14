using CrawlHTML.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlHTML
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            CrawlPage.Start();
            HtmlData.Do();
            Console.ReadLine();
        }
    }
}
