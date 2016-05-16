using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hadoop;
using System.IO;
using System.Xml;
using SSHWrapper;

namespace InvokeHadoop
{
    class Program
    {
        private static string configPath = @"../../config.xml";

        public static void Main()
        {
            Logger.log("starting app");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configPath);

            XmlNode node = xmlDoc.DocumentElement.SelectSingleNode("/config/java_dir");
            string javaDir = node.FirstChild.Value;

            node = xmlDoc.DocumentElement.SelectSingleNode("/config/tweets_dir");
            string tweetsDir = node.FirstChild.Value + "\\" + DateTime.Now.ToString("yyyyMMdd");

            node = xmlDoc.DocumentElement.SelectSingleNode("/config/results_dir");
            string resultsPath = node.FirstChild.Value + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

            Logger.log("config loaded");

            DateTime t = DateTime.Now;

            if (!Directory.Exists(tweetsDir))
            {
                Logger.log("tweets dir does not exist, terminating");
            }
            else
            {
                HadoopManager hadoop = new HadoopManager(new SshManager("192.168.196.128", "training", "training"));

                if (!hadoop.init(javaDir))
                {
                    Logger.log("init failed, terminating");
                }
                else
                {
                    Logger.log("init succeeded");

                    try
                    {
                        hadoop.Run(tweetsDir, resultsPath);
                        Logger.log("hadoop run finished with no exception");
                    }
                    catch (Exception)
                    {
                        Logger.log("hadoop run failed");
                    }
                }
            }

            Logger.log("app finished");
        }

    }
}
