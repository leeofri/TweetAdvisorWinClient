using Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamir.SharpSsh;
using WinSCP;

namespace SSHWrapper
{
    public class SshManager
    {
        private string username;
        private string password;
        private string machine_ip;

        public SshManager(string ip, string username, string password)
        {
            this.username = username;
            this.password = password;
            this.machine_ip = ip;
        }

        public void MakeNewDirectory(string dirName)
        {

            //create a new ssh stream
            using (SshStream ssh = new SshStream(this.machine_ip, this.username, this.password))
            {
                //writing to the ssh channel
                ssh.Write("mkdir -p " + dirName);
            }
        }

        public void TransferFileToMachine(string localFilePath, string remoteFilePath)
        {
            try
            {
                //Create a new SCP instance
                Scp scp = new Scp(machine_ip, username, password);

                scp.Connect();

                //Copy a file from remote SSH server to local machine
                scp.To(localFilePath, remoteFilePath);

                scp.Close();
            }
            catch (Exception ex)
            {
                SLogger.log("TransferFileToMachine - Exeption Massage:" + ex.Message);
                throw;
            }
        }

        public void TransferFileFromMachine(string remoteFile, string localPath)
        {
            try
            {
                //Create a new SCP instance
                Scp scp = new Scp(machine_ip, username, password);

                scp.Connect();

                //Copy a file from remote SSH server to local machine
                scp.From(remoteFile, localPath);

                scp.Close();
            }
            catch (Exception ex)
            {
                SLogger.log("TransferFileFromMachine - Exeption Massage:" + ex.Message);
                throw;
            }
        }

        public void ExecuteSingleCommand(string command)
        {
            try
            {
                //create a new ssh stream
                SshExec ssh = new SshExec(machine_ip, username, password);

                ssh.Connect();

                //writing to the ssh channel
               var str =  ssh.RunCommand(command);

                ssh.Close();
            }
            catch (Exception e)
            {
                SLogger.log("ExecuteSingleCommand - Exeption Massage:" + e.Message);
                throw;
            }
        }

        public void ExecuteSingleCommand(string command,ref string StdOut,ref string StdErr)
        {
            try
            {
                //create a new ssh stream
                SshExec ssh = new SshExec(machine_ip, username, password);

                ssh.Connect();

                //writing to the ssh channel
                var str = ssh.RunCommand(command,ref StdOut,ref StdErr);

                ssh.Close();
            }
            catch (Exception e)
            {
                SLogger.log(" ExecuteSingleCommand(string command,ref string StdOut,ref string StdErr) - Exeption Massage:" + e.Message);
                throw;
            }
        }

        public bool TransferFolderToMachine(string localFolderPath, string remoteFolderPath)
        {
            try
            {
                // Setup session options
                SessionOptions sessionOptions = new SessionOptions 
                {
                    Protocol = Protocol.Scp,
                    HostName = this.machine_ip,
                    UserName = this.username,
                    Password = this.password,
                };

                using (Session session = new Session())
                {
                    // Obtaining host key 
                    sessionOptions.SshHostKeyFingerprint = session.ScanFingerprint(sessionOptions);

                    // Will continuously report progress of synchronization
                    session.FileTransferred += FileTransferred;

                    // Connect
                    session.Open(sessionOptions);

                    // Synchronize files
                    SynchronizationResult synchronizationResult;
                    synchronizationResult =
                        session.SynchronizeDirectories(
                            SynchronizationMode.Remote, localFolderPath,remoteFolderPath, true,true);

                    // Throw on any error
                    synchronizationResult.Check();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
                return false;
            }
        }


        private static void FileTransferred(object sender, TransferEventArgs e)
        {
            if (e.Error == null)
            {
                Console.WriteLine("Upload of {0} succeeded", e.FileName);
            }
            else
            {
                Console.WriteLine("Upload of {0} failed: {1}", e.FileName, e.Error);
            }

            if (e.Chmod != null)
            {
                if (e.Chmod.Error == null)
                {
                    Console.WriteLine("Permisions of {0} set to {1}", e.Chmod.FileName, e.Chmod.FilePermissions);
                }
                else
                {
                    Console.WriteLine("Setting permissions of {0} failed: {1}", e.Chmod.FileName, e.Chmod.Error);
                }
            }
            else
            {
                Console.WriteLine("Permissions of {0} kept with their defaults", e.Destination);
            }

            if (e.Touch != null)
            {
                if (e.Touch.Error == null)
                {
                    Console.WriteLine("Timestamp of {0} set to {1}", e.Touch.FileName, e.Touch.LastWriteTime);
                }
                else
                {
                    Console.WriteLine("Setting timestamp of {0} failed: {1}", e.Touch.FileName, e.Touch.Error);
                }
            }
            else
            {
                // This should never happen during "local to remote" synchronization
                Console.WriteLine("Timestamp of {0} kept with its default (current time)", e.Destination);
            }
        }

    }
}
