using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadoop
{
    public class Logger
    {
        private static string logPath = @"./log.txt";

        public static void log(string logMessage)
        {
            StreamWriter log = File.AppendText(logPath);

            log.Write("\r\nLog Entry : ");
            log.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            log.WriteLine("{0}", logMessage);
            log.WriteLine("-------------------------------");
            log.Flush();

            log.Close();
        }
    }
}
