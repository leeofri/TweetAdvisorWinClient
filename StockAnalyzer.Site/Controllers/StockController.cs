using System;
using System.Collections.Generic;
using System.Globalization;
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

        // Input : Formatted file in given path
        // Output :  Array of the candidates id and result 
        private CandidatesResult[] ProcessFile(string FilePath)
        {
            
            string[] allLines = File.ReadAllLines(FilePath);
            string[] currCandidateLine;
            int currCandidateEnum = 0;
            int candidateID = -1;
            float candidateResult = 9.9F;

            CandidatesResult[] candidatesResults = new CandidatesResult[allLines.Length];

            foreach (string item in allLines)
            {
                currCandidateLine = item.Split(';');
                candidatesResults[currCandidateEnum] = new CandidatesResult();

                int.TryParse(currCandidateLine[0], out candidateID);
                float.TryParse(currCandidateLine[1], out candidateResult);
                candidatesResults[currCandidateEnum].ID = candidateID;
                candidatesResults[currCandidateEnum].Result = candidateResult;

                currCandidateEnum++;
       
            }
            return candidatesResults;
        }

        public object Get([FromUri] CandidateData CandidateData)
        {
            // Uses to save the results for given day
            CandidatesResult[] currCandidateResult;

            // Uses to return the full results of all candidates and days
            FullResult[] fullResults;

            //CreateUserConfigFile(UserOptions, 2);

            int currDate;
            string currFile;
            
            DateTime tempStartDate = new DateTime(2016, 5, 1);              // TODO : remove
            DateTime tempEndDate = new DateTime(2016, 5, 5);                // TODO : remove

            // Convert the dates to text
            int startDate = 0;

            string startDateText = ConvertDate(tempStartDate);              // TODO : remove
            // string startDateText = ConvertDate(CandidateData.startDate);
            int.TryParse(startDateText, out startDate);

            int endDate = 0;
            string endDateText = ConvertDate(tempEndDate);                  // TODO : remove
            // string endDateText = ConvertDate(CandidateData.endDate);
            int.TryParse(endDateText, out endDate);

            string[] fileEntries = Directory.GetFiles(IMPORT_FOLDER);

            // We have only one day
            if (startDate == endDate)
            {
                // We only have one day
                fullResults = new FullResult[0];
                
                if (File.Exists(IMPORT_FOLDER + startDateText))
                {
                    // Process the single file and insert the data into the result
                    currCandidateResult = ProcessFile(IMPORT_FOLDER + startDateText);
                    fullResults[0].CandidatesResults = currCandidateResult;
                    fullResults[0].PredictionDate = CandidateData.startDate;
                }
            }

            // we need to iterate over files
            else
            {
                int candidateEnum = 0;
                DateTime formattedDate = new DateTime();
                currDate = startDate;
                fullResults = new FullResult[fileEntries.Length];

                foreach (string fileName in fileEntries)
                {

                    currFile = IMPORT_FOLDER + "\\" + currDate + ".txt";
                    if (File.Exists(currFile))
                    {
                        currCandidateResult = ProcessFile(currFile);
                        fullResults[candidateEnum] = new FullResult();
                        fullResults[candidateEnum].CandidatesResults = currCandidateResult;

                           
                        formattedDate = DateTime.ParseExact(currDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                        fullResults[candidateEnum].PredictionDate = formattedDate;

                        // Increase the date and the enum
                        candidateEnum++;
                        currDate++;
                    }
                }
            }

            return fullResults;
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
