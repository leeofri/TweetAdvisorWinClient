using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSHWrapper;
using System.Configuration;
using System.IO;

namespace Hadoop
{
    // hadoop class
    public class HadoopManager
    {
        const string JavaFilesPathOnMachine = @"C:\Users\Matan\Desktop\BigDataCourse\Task3\JavaFiles";
        const string WebLogsPathOnMacine = @"C:\Users\Matan\Desktop\BigDataCourse\Task3\Web_logs";

        const string EXPORT_FOLDER = @"C:\Users\Matan\Desktop\ExportFiles\";
        const string IMPORT_FOLDER = @"C:\Users\Matan\Desktop\ImportFiles\output.txt";

        public void Run()
        {
            TransferDirectoryToCloudera(EXPORT_FOLDER);
            CompilingJavaFilesOnRemote();
            RunHadoopOnRemote();
             TransferOutputFilesFromRemoteMachine("/home/training/finalrun/output/part-r-00000", IMPORT_FOLDER);
        }

        public void TransferDirectoryToCloudera(string path)
        {
            // Sending the the weblogs
            foreach (string file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                var fileName = file.Split('\\')[6];
                var directoryName = file.Split('\\')[5] + "/";
                SshManager.TransferFileToMachine(file, "/home/training/finalrun/" + directoryName + fileName);
            }
        }


        public void TransferWebLogFilesToRemote(string filesPath)
        {
            // Sending the the weblogs
            var webLogsFiles = Directory.GetFiles(@"C:\Users\Matan\Desktop\BigDataCourse\Task3\Web_logs");

            for (int i = 0; i < webLogsFiles.Length; i++)
            {

                SshManager.TransferFileToMachine(webLogsFiles[i], "/home/training/ex4run/WebLogs/" + "Log" + i + ".txt");
            }
        }

        private void TransferJavaFilesToRemote(string javaFilePath)
        {
            // Adding the files to hadoop hdfs

            // Sending the javafiles
            var javaFiles = Directory.GetFiles(@"C:\Users\Matan\Desktop\BigDataCourse\Task3\JavaFiles");
            string[] fileNames = Directory.GetFiles(@"C:\Users\Matan\Desktop\BigDataCourse\Task3\JavaFiles", "*.java")
                                     .Select(path => Path.GetFileName(path))
                                     .ToArray();

            for (int i = 0; i < javaFiles.Length; i++)
            {
                SshManager.TransferFileToMachine(javaFiles[i], "/home/training/ex4run/" + fileNames[i]);

            }
        }

        private void CompilingJavaFilesOnRemote()
        {
            SshManager.ExecuteSingleCommand("cd /home/training/finalrun/");

            // Moving the class files to main folder - solving some isues
            SshManager.ExecuteSingleCommand("cd /home/training/finalrun/ && cp -r javafiles/* ./");

            // Compiling the javafiles - Making class files
            SshManager.ExecuteSingleCommand("javac -cp /usr/lib/hadoop/*:/usr/lib/hadoop/client-0.20/*:/usr/lib/hadoop/lib/* -d /home/training/finalrun/ /home/training/finalrun/*.java");

            // Creating the jar
            SshManager.ExecuteSingleCommand("cd /home/training/finalrun/ && jar -cvf  /home/training/finalrun/StackAnalyzer.jar -c solution/*.class;");
        }

        private void RunHadoopOnRemote()
        {
            SshManager.ExecuteSingleCommand("rm -f /home/training/finalrun/output/part-r-00000");

            SshManager.ExecuteSingleCommand("hadoop fs -rm finalrun/input/input");

            SshManager.ExecuteSingleCommand("hadoop fs -rm finalrun/data/SequenceFile.canopyCenters");

            SshManager.ExecuteSingleCommand("hadoop fs -rm finalrun/data/SequenceFile.kmeansCenters");

            SshManager.ExecuteSingleCommand("hadoop fs -rm -r finalrun/output");

            // Making all the folders
            SshManager.ExecuteSingleCommand("hadoop fs -mkdir finalrun");

            SshManager.ExecuteSingleCommand("hadoop fs -mkdir finalrun/input");

            SshManager.ExecuteSingleCommand("hadoop fs -mkdir finalrun/data");

            SshManager.ExecuteSingleCommand("hadoop fs -mkdir finalrun/output");

            //Upload files to hadoop HDFS
            SshManager.ExecuteSingleCommand("hadoop fs -copyFromLocal /home/training/finalrun/input/input finalrun/input/");

            SshManager.ExecuteSingleCommand("hadoop fs -copyFromLocal /home/training/finalrun/data/userConfigFile.config finalrun/data/");

            // Running the map reduce function from the jar
            SshManager.ExecuteSingleCommand("hadoop jar /home/training/finalrun/StackAnalyzer.jar solution.FinalProj /user/training/finalrun/input/* /user/training/finalrun/output/");


            // Handle output
            SshManager.ExecuteSingleCommand("hadoop fs -get /user/training/finalrun/output/Kmeans0/part-r-00000  /home/training/finalrun/output/part-r-00000");
        }

        private void TransferOutputFilesFromRemoteMachine(string remoteFile, string localPath)
        {
            SshManager.TransferFileFromMachine(remoteFile, localPath);
        }
    }
}
