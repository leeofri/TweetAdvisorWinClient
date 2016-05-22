using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hadoop;
using System.IO;
using System.Xml;
using SSHWrapper;
using Logger;

namespace InvokeHadoop
{
    class Program
    {
        private static string configPath = @"../../config.xml";

        public static void Main()
        {
            SLogger.log("starting app");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configPath);

            DateTime today = System.DateTime.Now;
            string yesterday = today.AddDays(-1).ToString("yyyyMMdd");

            XmlNode node = xmlDoc.DocumentElement.SelectSingleNode("/config/java_dir");
            string javaDir = node.FirstChild.Value;

            node = xmlDoc.DocumentElement.SelectSingleNode("/config/tweets_dir");
            string tweetsDir = node.FirstChild.Value + "\\";

            node = xmlDoc.DocumentElement.SelectSingleNode("/config/results_dir");
            string resultsPath = node.FirstChild.Value + "\\";

            node = xmlDoc.DocumentElement.SelectSingleNode("/config/hadoop_master_node");
            string masterNode = node.FirstChild.Value;

            node = xmlDoc.DocumentElement.SelectSingleNode("/config/hadoop_user");
            string hadoopUser = node.FirstChild.Value;

            node = xmlDoc.DocumentElement.SelectSingleNode("/config/hadoop_pass");
            string hadoopPass = node.FirstChild.Value;

            SLogger.log("config loaded");
            string resultFile, currDate = "";


            if (!Directory.Exists(tweetsDir))
            {
                SLogger.log("tweets dir does not exist, terminating");
            }
            else
            {
                HadoopManager hadoop = new HadoopManager(new SshManager(masterNode, hadoopUser, hadoopPass));

                if (!hadoop.init(javaDir))
                {
                    SLogger.log("init failed, terminating");
                }
                else
                {
                    SLogger.log("init succeeded");

                    try
                    {
                        foreach (string dir in Directory.GetDirectories(tweetsDir))
                        {
                            currDate = dir.Substring(dir.LastIndexOf("\\") + 1);
                            resultFile = resultsPath + currDate + ".txt";
                            if ((!File.Exists(resultFile)) && currDate != today.ToString("yyyyMMdd"))
                            {
                                hadoop.Run(dir, resultFile);
                                SLogger.log("hadoop run finished with no exception. date: " + currDate);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        SLogger.log("hadoop run failed. date: " + currDate + " error: " +e.Message);
                    }
                }
            }

            SLogger.log("app finished");
        }

    }
}
