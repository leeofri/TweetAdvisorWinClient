using SSHWrapper;
using System.IO;
using System.Linq;
using Logger;
using System;

namespace Hadoop
{
    // hadoop class
    public class HadoopManager
    {
        SshManager SshM;

        // DM
        public const string BASE_DIR = "/home/training";
        public const string HDFS_DIR = "/user/training";


        public HadoopManager(SshManager s)
        {
            this.SshM = s;
        }

        public bool init(string JAVA_FILE_FOLDER)
        {
            if (sendJavaFiles(JAVA_FILE_FOLDER) && CompilingJavaFilesOnRemote())
            {
                return true;
            }

            return false;
        }

        public void Run(string INPUT_FILE_PATH,string OUTPUT_FILE)
        {
            TransferTweetsFilesToRemote(INPUT_FILE_PATH);
            RunHadoopOnRemote();
            TransferOutputFilesFromRemoteMachine(BASE_DIR+"/FromTheTweet/output/part-r-00000", OUTPUT_FILE);
        }

        private void TransferTweetsFilesToRemote(string IMPORT_FOLDER)
        {
            // Sending the the tweets
            var tweetsFiles = Directory.GetFiles(IMPORT_FOLDER);

    
            // remove exsisting files 
            SshM.ExecuteSingleCommand("rm -r "+ BASE_DIR + "/FromTheTweet/input");
            SshM.ExecuteSingleCommand("mkdir " + BASE_DIR + "/FromTheTweet/input");

            for (int i = 0; i < tweetsFiles.Length; i++)
            {
                SshM.TransferFileToMachine(tweetsFiles[i], BASE_DIR + "/FromTheTweet/input/" + tweetsFiles[i].Substring(tweetsFiles[i].LastIndexOf("\\") + 1));
            }
        }


        private bool sendJavaFiles(string javaFileFolder)
        {
            // init the envuerment
            SshM.ExecuteSingleCommand("rm -r -f" + BASE_DIR + "/FromTheTweet/javafiles");
            SshM.ExecuteSingleCommand("mkdir -p " + BASE_DIR + "/FromTheTweet/javafiles");

            return SshM.TransferFolderToMachine(javaFileFolder, BASE_DIR + "/FromTheTweet/javafiles");
        }


        private bool CompilingJavaFilesOnRemote()
        {
            try
            {
                // Moving the class files to main folder - solving some isues
                //SshM.ExecuteSingleCommand("cd /home/training/FromTheTweet/ && cp -r javafiles/* ./");

                // Compiling the javafiles - Making class files
                SshM.ExecuteSingleCommand("cd " + BASE_DIR + "/FromTheTweet/javafiles/src && javac -cp /usr/lib/hadoop/*:/usr/lib/hadoop/client-0.20/*:/usr/lib/hadoop/lib/*:src/solution/ThirdParty/*:src/*: "+BASE_DIR+"/FromTheTweet/javafiles/src/solution/tfidf/*.java " + BASE_DIR + "/FromTheTweet/javafiles/src/solution/tfidf/model/*.java " + BASE_DIR + "/FromTheTweet/javafiles/src/solution/*.java ");

                // Creating the jar
                SshM.ExecuteSingleCommand("cd " + BASE_DIR + "/FromTheTweet/javafiles/src && jar -cvf " + BASE_DIR + "/FromTheTweet/FromTheTweet.jar -c .");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
                return false;
            }

            return true;
        }

        private void RunHadoopOnRemote()
        {
            // logging parameters
            string StdOut = "", StdErr = "";


            SshM.ExecuteSingleCommand("rm -f "+ BASE_DIR + "/FromTheTweet/output/part-r-00000");

            SshM.ExecuteSingleCommand("hadoop fs -rm -r " + HDFS_DIR + "/FromTheTweet");

            // Making all the folders
            SshM.ExecuteSingleCommand("hadoop fs -mkdir " + HDFS_DIR + "/FromTheTweet");

            SshM.ExecuteSingleCommand("hadoop fs -mkdir " + HDFS_DIR + "/FromTheTweet/input");

            SshM.ExecuteSingleCommand("hadoop fs -mkdir "+HDFS_DIR+"/FromTheTweet/data");

            //Upload files to hadoop HDFS
            SshM.ExecuteSingleCommand("hadoop fs -copyFromLocal " + BASE_DIR + "/FromTheTweet/input "+ HDFS_DIR + "/FromTheTweet/input");

            //upload config fill to HDFS
            SshM.ExecuteSingleCommand("hadoop fs -copyFromLocal " + BASE_DIR + "/FromTheTweet/javafiles/userConfigFile.config " + HDFS_DIR + "/FromTheTweet/data");

            //Upload word dic to hadoop HDFS
            SshM.ExecuteSingleCommand("hadoop fs -copyFromLocal " + BASE_DIR + "/FromTheTweet/javafiles/wordDictionary.txt " + HDFS_DIR + "/FromTheTweet/data");

            // Running the map reduce function from the jar
            SshM.ExecuteSingleCommand("hadoop jar " + BASE_DIR + "/FromTheTweet/FromTheTweet.jar solution.FinalProj " + HDFS_DIR + "/FromTheTweet/input/input " + HDFS_DIR + "/FromTheTweet/output",ref StdOut,ref StdErr);

            // send the hadoop result to the log
            SLogger.log(StdErr);

            // Handle output
            SshM.ExecuteSingleCommand("mkdir -p " + BASE_DIR + "/FromTheTweet/output");
            SshM.ExecuteSingleCommand("hadoop fs -get FromTheTweet/output/part-r-00000  " + BASE_DIR + "/FromTheTweet/output/part-r-00000");
        }

        private void TransferOutputFilesFromRemoteMachine(string remoteFile, string localPath)
        {
            SshM.TransferFileFromMachine(remoteFile, localPath);
        }

    }
}
