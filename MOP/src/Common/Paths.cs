// Modern Optimization Plugin
// Copyright(C) 2019-2022 Athlon

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

using System.CodeDom;
using System.IO;
using UnityEngine;

namespace MOP.Common
{
    class Paths
    {
        public const string LogFolderName = "MOP_Logs";
        public const string DefaultErrorLogName = "MOP_Crash";
        public const string DefaultReportLogName = "MOP_Report";
        public const string BugReportFileName = "MOP Bug Report";
        public const string SaveFileBugsReport = "SaveFileBugsReport";
        public static string MopSettingsFile = Path.Combine(MOP.ModConfigPath, "settings.json");

        public static string LogFolder
        {
            get
            {
                string path = Path.Combine(RootPath, LogFolderName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public static bool LogDirectoryExists => Directory.Exists(LogFolder);
        public static string RootPath => Application.dataPath.Replace("/mysummercar_Data", "");
        public static string OutputLogPath => $"{RootPath}/output_log.txt";
        public static string BugReportPath => $"{RootPath}/MOP_bugreport";
    }
}
