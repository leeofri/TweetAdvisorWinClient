using SSHWrapper;
using System.IO;
using System.Linq;

namespace Hadoop
{
    // hadoop class
    public class HadoopManager
    {
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
            TransferOutputFilesFromRemoteMachine("/home/training/FromTheTweet/output/part-r-00000", OUTPUT_FILE);
        }

        private void TransferTweetsFilesToRemote(string IMPORT_FOLDER)
        {
            // Sending the the tweets
            var tweetsFiles = Directory.GetFiles(IMPORT_FOLDER);

            //string[] fileNames = Directory.GetFiles(JAVA_FILE_FOLDER, "*.java")
            //                .Select(path => Path.GetFileName(path))
            //                .ToArray();

            // remove exsisting files 
            SshManager.ExecuteSingleCommand("rm -r /home/training/FromTheTweet/input/*");

            for (int i = 0; i < tweetsFiles.Length; i++)
            {
                SshManager.TransferFileToMachine(tweetsFiles[i], "/home/training/FromTheTweet/input" + tweetsFiles[i].Substring(tweetsFiles[i].LastIndexOf("\\")));
            }

        }


        private void sendJavaFiles(string JAVA_FILE_FOLDER)
        {
            // Sending the javafiles
            var javaFiles = Directory.GetFiles(JAVA_FILE_FOLDER);
            //string[] fileNames = Directory.GetFiles(JAVA_FILE_FOLDER, "*.java")
            //                 .Select(path => Path.GetFileName(path))
            //                 .ToArray();

            for (int i = 0; i < javaFiles.Length; i++)
            {
                SshManager.TransferFileToMachine(javaFiles[i], "/home/training/FromTheTweet/javafiles" + javaFiles[i].Substring(javaFiles[i].LastIndexOf("\\")));
            }
        }


        private bool CompilingJavaFilesOnRemote()
        {
            try
            {
                SshManager.ExecuteSingleCommand("cd /home/training/FromTheTweet/");

                // Moving the class files to main folder - solving some isues
                SshManager.ExecuteSingleCommand("cd /home/training/FromTheTweet/ && cp -r javafiles/* ./");

                // Compiling the javafiles - Making class files
                SshManager.ExecuteSingleCommand("javac -cp /usr/lib/hadoop/*:/usr/lib/hadoop/client-0.20/*:/usr/lib/hadoop/lib/* -d /home/training/FromTheTweet/ /home/training/FromTheTweet/*.java");

                // Creating the jar
                SshManager.ExecuteSingleCommand("cd /home/training/FromTheTweet/ && jar -cvf  /home/training/FromTheTweet/FromTheTweet.jar -c solution/*.class;");
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void RunHadoopOnRemote()
        {
            SshManager.ExecuteSingleCommand("rm -f /home/training/FromTheTweet/output/part-r-00000");

            SshManager.ExecuteSingleCommand("hadoop fs -rm FromTheTweet/input/input");

            SshManager.ExecuteSingleCommand("hadoop fs -rm FromTheTweet/data/SequenceFile.canopyCenters");

            SshManager.ExecuteSingleCommand("hadoop fs -rm FromTheTweet/data/SequenceFile.kmeansCenters");

            SshManager.ExecuteSingleCommand("hadoop fs -rm -r FromTheTweet/output");

            // Making all the folders
            SshManager.ExecuteSingleCommand("hadoop fs -mkdir FromTheTweet");

            SshManager.ExecuteSingleCommand("hadoop fs -mkdir FromTheTweet/input");

            SshManager.ExecuteSingleCommand("hadoop fs -mkdir FromTheTweet/data");

            SshManager.ExecuteSingleCommand("hadoop fs -mkdir FromTheTweet/output");

            //Upload files to hadoop HDFS
            SshManager.ExecuteSingleCommand("hadoop fs -copyFromLocal /home/training/FromTheTweet/input/input finalrun/input/");

            SshManager.ExecuteSingleCommand("hadoop fs -copyFromLocal /home/training/FromTheTweet/data/userConfigFile.config finalrun/data/");

            // Running the map reduce function from the jar
            SshManager.ExecuteSingleCommand("hadoop jar /home/training/FromTheTweet/FromTheTweet.jar solution.FromTheTweet /user/training/FromTheTweet/input/* /user/training/FromTheTweet/output/");


            // Handle output
            SshManager.ExecuteSingleCommand("hadoop fs -get /user/training/FromTheTweet/output/Kmeans0/part-r-00000  /home/training/FromTheTweet/output/part-r-00000");
        }

        private void TransferOutputFilesFromRemoteMachine(string remoteFile, string localPath)
        {
            SshManager.TransferFileFromMachine(remoteFile, localPath);
        }

    }
}
