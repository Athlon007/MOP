using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MOP.Common
{
    class Paths
    {
        public const string LogFolderName = "MOP_Logs";
        public const string DefaultErrorLogName = "MOP_Crash";
        public const string DefaultReportLogName = "MOP_Report";

        public  static string LogFolder
        {
            get
            {
                string path = $"{RootPath}/{LogFolderName}";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public static bool LogDirectoryExists => Directory.Exists(LogFolder);
        public static string RootPath => Application.dataPath.Replace("mysummercar_Data", "");
        public static string OutputLogPath => $"{RootPath}/output_log.txt";
    }
}
