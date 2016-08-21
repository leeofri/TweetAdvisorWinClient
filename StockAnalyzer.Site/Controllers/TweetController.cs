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
using System.Text;
using SSHWrapper;

namespace StockAnalyzer.Site.Controllers
{
   

    public class TweetController : ApiController
    {
        const string USER_CONFIG_FILE_PATH = @"C:\Users\Matan\Desktop\ExportFiles\data\userConfigFile.config";
        const string IMPORT_FOLDER = @"C:\Shahaf\FProj\Result";

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
        private DayResult ProcessFile(string FilePath)
        {
            
            string[] allLines = File.ReadAllLines(FilePath);
            string[] currCandidateLine;
            float candidateResult = 9.9F;

            DayResult daysResults = new DayResult();

            foreach (string item in allLines)
            {
                currCandidateLine = item.Split('\t');

                float.TryParse(currCandidateLine[1], out candidateResult);
                switch (currCandidateLine[0])
                {
                    case "HC":
                        daysResults.HC = candidateResult;
                        break;
                    case "DT":
                        daysResults.DT = candidateResult;
                        break;
                    case "TC":
                        daysResults.TC = candidateResult;
                        break;
                    case "BS":
                        daysResults.BS = candidateResult;
                        break;
                    case "JK":
                        daysResults.JK = candidateResult;
                        break;
                    default:
                        throw new Exception();
                }
            }
            return daysResults;
        }

        public object Get([FromUri] CandidateData CandidateData)
        {

            // Lee test
            //HadoopManager hadoop = new HadoopManager(new SshManager("192.168.196.128", "training", "training"));
            //hadoop.init(@"E:\Programming\FromTheTweet\TweetAdvisorWinClient\StockAnalyzer.Site\Resources\JavaCode");
            //hadoop.Run(@"E:\Programming\FromTheTweet\TweetAdvisorWinClient\StockAnalyzer.Site\Resources\Tweets", @"E:\Programming\FromTheTweet\TweetAdvisorWinClient\StockAnalyzer.Site\Resources\Results");
            //hadoop.init(System.Configuration.ConfigurationManager.AppSettings["javaFilePath"]);

            // Uses to save the results for given day
            DayResult currCandidateResult;

            // Uses to return the full results of all candidates and days
            DayResult[] result;


            //CreateUserConfigFile(UserOptions, 2);

            int currDate;
            string currFile;
            DateTime startDate = Convert.ToDateTime(CandidateData.startDate);              // TODO : remove
            DateTime endDate = Convert.ToDateTime(CandidateData.endDate);                // TODO : remove
            DateTime empty = new DateTime(1, 1, 1);

            // Convert the dates to text

            if (startDate == empty || endDate == empty)
                return new DayResult[0];


            string[] fileEntries = Directory.GetFiles(IMPORT_FOLDER);

            // We have only one day
            if (startDate == endDate)
            {
                // We only have one day
                result = new DayResult[1];
                
                if (File.Exists(IMPORT_FOLDER + "\\" + startDate.ToString("yyyyMMdd") + ".txt"))
                {
                    // Process the single file and insert the data into the result
                    currCandidateResult = ProcessFile(IMPORT_FOLDER + "\\" + startDate.ToString("yyyyMMdd") + ".txt");
                    currCandidateResult.PredictionDate = CandidateData.startDate;
                    result[0] = currCandidateResult;
                }
            }

            // we need to iterate over files
            else
            {
                int candidateEnum = 0;
                DateTime curDate = startDate;
                result = new DayResult[((int)(endDate - startDate).TotalDays + 1)];

                while (curDate <= endDate)
                {
                    currFile = IMPORT_FOLDER + "\\" + curDate.ToString("yyyyMMdd") + ".txt";
                    if (File.Exists(currFile))
                    {
                        currCandidateResult = ProcessFile(currFile);
                        result[candidateEnum] = currCandidateResult;

                           
                        result[candidateEnum].PredictionDate = curDate.ToString("yyyy-MM-dd");

                        // Increase the date and the enum
                    }
                    else
                    {
                        if (candidateEnum == 0)
                        {
                            result[candidateEnum] = new DayResult();
                            result[candidateEnum].BS = 0;
                            result[candidateEnum].DT = 0;
                            result[candidateEnum].HC = 0;
                            result[candidateEnum].JK = 0;
                            result[candidateEnum].TC = 0;
                        }
                        else
                        {
                            result[candidateEnum] = new DayResult();
                            result[candidateEnum].BS = result[candidateEnum - 1].BS;
                            result[candidateEnum].DT = result[candidateEnum - 1].DT;
                            result[candidateEnum].HC = result[candidateEnum - 1].HC;
                            result[candidateEnum].JK = result[candidateEnum - 1].JK;
                            result[candidateEnum].TC = result[candidateEnum - 1].TC;
                        }
                        result[candidateEnum].PredictionDate = curDate.ToString("yyyy-MM-dd");
                    }
                    curDate = curDate.AddDays(1);
                    candidateEnum++;

                }
            }

            return result;
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
