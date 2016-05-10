using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hadoop;
using System.IO;
using System.Xml;

namespace InvokeHadoop
{
    class Program
    {
        private static string logPath = @"./log.txt";
        private static string configPath = @"../../config.xml";

        public static void Main()
        {
            StreamWriter log = File.AppendText(logPath);
            Log("starting app", log);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configPath);

            XmlNode node = xmlDoc.DocumentElement.SelectSingleNode("/config/java_dir");
            string javaDir = node.FirstChild.Value;

            node = xmlDoc.DocumentElement.SelectSingleNode("/config/tweets_dir");
            string tweetsDir = node.FirstChild.Value + "\\" + DateTime.Now.ToString("yyyyMMdd");

            node = xmlDoc.DocumentElement.SelectSingleNode("/config/results_dir");
            string resultsPath = node.FirstChild.Value + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

            Log("config loaded", log);

            DateTime t = DateTime.Now;

            if (!Directory.Exists(tweetsDir))
            {
                Log("tweets dir does not exist, terminating", log);
            }
            else
            {
                HadoopManager hadoop = new HadoopManager();

                if (!hadoop.init(javaDir))
                {
                    Log("init failed, terminating", log);
                }
                else
                {
                    Log("init succeeded", log);

                    try
                    {
                        hadoop.Run(tweetsDir, resultsPath);
                        Log("hadoop run finished with no exception", log);
                    }
                    catch (Exception)
                    {
                        Log("hadoop run failed", log);
                    }
                }
            }

            Log("app finished", log);
            log.Close();
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("{0}", logMessage);
            w.WriteLine("-------------------------------");
            w.Flush();
        }

    }
}
