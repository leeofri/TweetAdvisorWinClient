using Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamir.SharpSsh;


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
    }
}
