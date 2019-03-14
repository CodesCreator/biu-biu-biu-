using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlHTML.Service
{
   public class ErrorUtils
    {
        public static void ErrorException(AggregateException xx,string des,Logger logger)
        {
            if (xx != null) {
                Console.WriteLine(des + xx.Message);
                Console.WriteLine(des + xx.StackTrace);
                Console.WriteLine(des + xx.Source);
                logger.Warn(des + xx.Message);
                logger.Warn(des + xx.StackTrace);
                logger.Warn(des + xx.Source);
                if (xx.InnerExceptions != null) {
                    foreach (var exitem in xx.InnerExceptions) {
                        Console.WriteLine(des + exitem.Message);
                        Console.WriteLine(des + exitem.StackTrace);
                        Console.WriteLine(des + exitem.Source);
                        logger.Warn(des + exitem.Message);
                        logger.Warn(des + exitem.StackTrace);
                        logger.Warn(des + exitem.Source);
                    }
                }
            }
        }

        internal static void ErrorException(Exception xx,string des,Logger logger)
        {
            if (xx != null) {
                Console.WriteLine(des + xx.Message);
                Console.WriteLine(des + xx.StackTrace);
                Console.WriteLine(des + xx.Source);
                logger.Error(des + xx.Message);
                logger.Error(des + xx.StackTrace);
                logger.Error(des + xx.Source);
                if (xx.InnerException != null) {
                    Console.WriteLine(des + xx.InnerException.Message);
                    Console.WriteLine(des + xx.InnerException.StackTrace);
                    Console.WriteLine(des + xx.InnerException.Source);
                    logger.Error(des + xx.InnerException.Message);
                    logger.Error(des + xx.InnerException.StackTrace);
                    logger.Error(des + xx.InnerException.Source);
                }
            }
        }
    }
}
