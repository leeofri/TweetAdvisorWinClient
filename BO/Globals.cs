using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class Globals
    {
        public const string IMPORT_FOLDER = "";
        public const string EXPORT_FOLDER = "";
        public static string JAVA_FILE_FOLDER = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Resources\";
        public const string TARGET_FOLDER_HADOOP = "";
    }
}
