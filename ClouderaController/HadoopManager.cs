using System.Linq;
using SSHWrapper;
using System.IO;
using BO;

namespace Hadoop
{
    // hadoop class
    public class HadoopManager
    {
        public void Run()
        {   
            TransferDirectoryToCloudera(Globals.JAVA_FILE_FOLDER);
            CompilingJavaFilesOnRemote();
            RunHadoopOnRemote();
            TransferOutputFilesFromRemoteMachine("/home/training/FromTheTweet/output/part-r-00000", null);
        }

        public void TransferDirectoryToCloudera(string path)
        {
            // Sending the the weblogs
            // TODO: changre the matod to move the tweets file
            foreach (string file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                var fileName = file.Split('\\')[6];
                var directoryName = file.Split('\\')[5] + "/";
                SshManager.TransferFileToMachine(file, "/home/training/FromTheTweet/" + directoryName + fileName);
            }
        }


        public void TransferWebLogFilesToRemote(string filesPath)
        {
            // Sending the the weblogs
            // TODO: changre the matod to move the tweets file
            var webLogsFiles = Directory.GetFiles(Globals.IMPORT_FOLDER);
            // TODO: changre the matod to move the tweets file
            // Sending the javafiles
            var javaFiles = Directory.GetFiles(Globals.JAVA_FILE_FOLDER);
            string[] fileNames = Directory.GetFiles(Globals.JAVA_FILE_FOLDER, "*.java")
                                     .Select(path => Path.GetFileName(path))
                                     .ToArray();

            for (int i = 0; i < javaFiles.Length; i++)
            {
                SshManager.TransferFileToMachine(javaFiles[i], "/home/training/FromTheTweet/" + fileNames[i]);

            }
        }

        private void CompilingJavaFilesOnRemote()
        {
            SshManager.ExecuteSingleCommand("cd /home/training/FromTheTweet/");

            // Moving the class files to main folder - solving some isues
            SshManager.ExecuteSingleCommand("cd /home/training/FromTheTweet/ && cp -r javafiles/* ./");

            // Compiling the javafiles - Making class files
            SshManager.ExecuteSingleCommand("javac -cp /usr/lib/hadoop/*:/usr/lib/hadoop/client-0.20/*:/usr/lib/hadoop/lib/* -d /home/training/FromTheTweet/ /home/training/FromTheTweet/*.java");

            // Creating the jar
            SshManager.ExecuteSingleCommand("cd /home/training/FromTheTweet/ && jar -cvf  /home/training/FromTheTweet/FromTheTweet.jar -c solution/*.class;");
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
