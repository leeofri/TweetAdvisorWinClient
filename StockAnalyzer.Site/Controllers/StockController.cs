﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using StockAnalyzer.Site.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Hadoop;
using System.IO;
using BO;
using StockReader;
using System.Text;

namespace StockAnalyzer.Site.Controllers
{
   

    public class StockController : ApiController
    {
        const string USER_CONFIG_FILE_PATH = @"C:\Users\Matan\Desktop\ExportFiles\data\userConfigFile.config";
        const string IMPORT_FOLDER = @"C:\Shahaf\FinalProj\Honey\TweetAdvisorWinClient\FileSamples";

        HadoopManager hadoop = new HadoopManager();

        private string ConvertDate(DateTime currDate)
        {
            return currDate.ToString("yyyyMMdd");

            //StringBuilder sb = new StringBuilder();
            //sb.Append(currDate.Year);
            //sb.Append(currDate.Month);
            //sb.Append(currDate.Day);

            //return sb.ToString();
        }

        private string ProcessFile(string FilePath)
        {
            StringBuilder sb = new StringBuilder();
            string[] allLines = File.ReadAllLines(FilePath);
            foreach (string item in allLines)
            {
                sb.Append(item);
                sb.Append("@");
            }
            return sb.ToString();
        }

        public object Get([FromUri] CandidateData CandidateData)
        {
            string resultString = string.Empty;
            int currDate;
            string currFile;


            DateTime tempStartDate = new DateTime(2016, 5, 1);              // TODO : remove
            DateTime tempEndDate = new DateTime(2016, 5, 5);                // TODO : remove


            // Convert the dates to text
            int startDate = 0;

            string startDateText = ConvertDate(tempStartDate);              // TODO : remove
            // string startDateText = ConvertDate(CandidateData.startDate);
            int.TryParse(startDateText,out startDate);

            int endDate = 0;
            string endDateText = ConvertDate(tempEndDate);                  // TODO : remove
            // string endDateText = ConvertDate(CandidateData.endDate);
            int.TryParse(endDateText , out endDate);

            StringBuilder sb = new StringBuilder();

            string[] fileEntries = Directory.GetFiles(IMPORT_FOLDER);

            // We have only one day
            if (startDate == endDate)
            {
                if (File.Exists(IMPORT_FOLDER + startDateText))
                {
                    resultString = ProcessFile(IMPORT_FOLDER + startDateText);
                }
            }

            // we need to iterate over files
            else
            {
                currDate = startDate;
                foreach (string fileName in fileEntries)
                {
                    
                    currFile = IMPORT_FOLDER + "\\" + currDate + ".txt";
                    if (File.Exists(currFile))
                    {
                        sb.Append(currDate.ToString());
                        sb.Append(";");
                        sb.Append(ProcessFile(currFile));
                        sb.Append("&");
                        currDate++;
                    }
                }
                resultString = sb.ToString();
            }

            return resultString;

            /*
            StockManager stockReader = new StockReader.StockManager();
            var stocks = stockReader.MakeInputStocksFile(UserOptions.StocksNumber, UserOptions.DaysNumber);

            var businessDays = stocks[0].Days.Count;

            CreateUserConfigFile(UserOptions, businessDays);

            hadoop.Run();

            // Reading the imported file - and clustering the stocks acordingly
            Dictionary<string, List<String>> importedFileContent = 
                ReadingImportedFileAndCluserTheLines(IMPORT_FOLDER);

            return Clustering(stocks, importedFileContent); 
            */
        }

        private Dictionary<string, List<string>> ReadingImportedFileAndCluserTheLines(string path)
        {
            Dictionary<string, List<string>> final = new Dictionary<string, List<string>>();

            var lines = File.ReadLines(path);

            foreach (var line in lines)
            {
                try
                {
                    string clusterName = line.Split('\t')[0];
                    string stockName = line.Split('\t')[1];

                    if (clusterName != String.Empty && stockName != String.Empty)
                    {
                        if (!final.ContainsKey(clusterName))
                        {
                            var newList = new List<string>();
                            newList.Add(stockName);

                            final.Add(clusterName, newList);
                        }
                        else
                        {
                            final[clusterName].Add(stockName);
                        }
                    }
                }
                catch (Exception)
                {

                    
                }
            }

            return final;
        }

        private object Clustering(List<Stock> stocks, Dictionary<string, List<String>> importedFileContent)
        {

            var clusteredStocks = new List<Cluster>();

            foreach (var currClusterName in importedFileContent.Keys)
            {
                var currCluster = new Cluster(currClusterName);
                foreach (var currStockName in importedFileContent[currClusterName])
                {
                    try
                    {
                        var currStock = stocks.Where(x => x.Name == currStockName).First();
                        currCluster.Stocks.Add(currStock);
                    }
                    catch (Exception)
                    {

                      
                    }
                   
                }

                clusteredStocks.Add(currCluster);
            }

            return clusteredStocks;
        }

        private void CreateUserConfigFile(UserOptions userOptions, int businessDays)
        {
            var featuresNumber = CalcFeaturesNumber(Convert.ToInt32(userOptions.Open),
                Convert.ToInt32(userOptions.Close),
                Convert.ToInt32(userOptions.High),
                Convert.ToInt32(userOptions.Low));


            var content = String.Format("kmeansCount {0} daysNumber {1} featuresNumber {2}",
                                userOptions.ClusterNumber, businessDays, featuresNumber);

            //File.Create(USER_CONFIG_FILE_PATH);
            File.WriteAllText(USER_CONFIG_FILE_PATH, content);
        }

        private int CalcFeaturesNumber(int open, int close, int high, int low)
        {
            return close + open + high + low;
        }
    }
}
