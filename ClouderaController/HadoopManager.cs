using SSHWrapper;
using System.IO;
using System.Linq;
using Logger;
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
            sendJavaFiles(JAVA_FILE_FOLDER);
            bool result = CompilingJavaFilesOnRemote();
            return result;
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


        private void sendJavaFiles(string JAVA_FILE_FOLDER)
        {
            // Sending the javafiles
            var javaFiles = Directory.GetFiles(JAVA_FILE_FOLDER,"*",SearchOption.AllDirectories);

            // init the envuerment
            SshM.ExecuteSingleCommand("rm -r -f" + BASE_DIR + "/FromTheTweet/javafiles");
            SshM.ExecuteSingleCommand("mkdir -p " + BASE_DIR + "/FromTheTweet/javafiles");

            for (int i = 0; i < javaFiles.Length; i++)
            {
                // with full path as name
                //SshM.TransferFileToMachine(javaFiles[i], "/home/training/FromTheTweet/javafiles" + javaFiles[i].Substring(javaFiles[i].IndexOf(JAVA_FILE_FOLDER) + JAVA_FILE_FOLDER.Length));

                // all file to onr folder
                SshM.TransferFileToMachine(javaFiles[i], BASE_DIR + "/FromTheTweet/javafiles/" + javaFiles[i].Substring(javaFiles[i].LastIndexOf("\\")+1));
            }
        }


        private bool CompilingJavaFilesOnRemote()
        {
            try
            {
                // Moving the class files to main folder - solving some isues
                //SshM.ExecuteSingleCommand("cd /home/training/FromTheTweet/ && cp -r javafiles/* ./");

                // Compiling the javafiles - Making class files
                SshM.ExecuteSingleCommand("javac -cp /usr/lib/hadoop/*:/usr/lib/hadoop/client-0.20/*:/usr/lib/hadoop/lib/* -d "+BASE_DIR + "/FromTheTweet "+ BASE_DIR + "/FromTheTweet/javafiles/*.java");

                // Creating the jar
                SshM.ExecuteSingleCommand("cd " + BASE_DIR + "/FromTheTweet && jar -cvf " + BASE_DIR + "/FromTheTweet/FromTheTweet.jar -c solution/*.class;");
            }
            catch
            {
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

            //Upload word dic to hadoop HDFS
            SshM.ExecuteSingleCommand("hadoop fs -copyFromLocal " + BASE_DIR + "/FromTheTweet/javafiles/wordDictionary.txt " + HDFS_DIR + "/FromTheTweet");

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
