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

using MSCLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

using MOP.Rules;
using System.Text;
using System.Linq;

namespace MOP.Common
{
    class ExceptionManager
    {
        private static readonly List<string> erorrsContainer = new List<string>();

        public static DateTime SessionTimeStart { get; set; }
        private static string currentLogFile;

        /// <summary>
        /// Creates then new error dump file
        /// </summary>
        /// <param name="ex"></param>
        public static void New(Exception ex, bool isCritical, string message)
        {
            // Don't save errors that already occured.
            if (erorrsContainer.Contains(message))
            {
                return;
            }

            bool isNewFile = false;
            // Log file doens't exist? Generate a new one.
            if (string.IsNullOrEmpty(currentLogFile))
            {
                string fileName = $"{Paths.DefaultErrorLogName}_{DateTime.Now:yyyy-MM-dd-HH-mm}";
                if (File.Exists($"{Paths.LogFolder}/{fileName}.txt"))
                {
                    int crashesInFolder = 0;
                    while (File.Exists($"{Paths.LogFolder}/{fileName}_{crashesInFolder}.txt"))
                    {
                        crashesInFolder++;
                    }

                    fileName += $"_{crashesInFolder}"; 
                }
                
                isNewFile = true;
                currentLogFile = fileName + ".txt";
            }

            string logFilePath = Path.Combine(Paths.LogFolder, currentLogFile).Replace("\\", "/");
            string errorInfo = $"({DateTime.Now:HH:mm:ss.fff})\n  Code: {message}\n  Type: {ex.GetType().Name}\n  Message: {ex.Message}{ex.StackTrace}\n  Target Site: {ex.TargetSite}";
            // Add version stamp, to the new log file.
            if (isNewFile)
            {
                errorInfo = $"MOP {MOP.ModVersion}\n" + errorInfo;
            }

            using (StreamWriter sw = new StreamWriter(logFilePath, true))
            {
                sw.Write(errorInfo + "\n\n");
            }

            string errorMessage = 
                $"[MOP] An error has occured. The log file has been saved into:\n\n" +
                $"{logFilePath}\nMessage: {message}\n\n" +
                $"Please go into MOP Settings, and click \"<b>I FOUND A BUG</b>\" button, in order to generate the bug report. " +
                $"Then please follow the provided instructions.\n";

            if (isCritical || erorrsContainer.Contains(message))
            {
                ModConsole.LogError(errorMessage);
            }
            else
            {
                ModConsole.LogWarning(errorMessage + "\n<b>You can continue playing.</b>");
            }
        }

        public static void OpenCurrentSessionLogFolder()
        {
            if (Paths.LogDirectoryExists)
            {
                Process.Start(Paths.LogFolder);
            }
            else
            {
                ModUI.ShowMessage("Logs folder doesn't exist.", "MOP");
            }
        }

        public static void OpenOutputLog()
        {
            if (File.Exists(Paths.OutputLogPath))
            {
                Process.Start(Paths.OutputLogPath);
            }
            else
            {
                ModUI.ShowMessage("File \"output_log.txt\" doesn't exist.", "MOP");
            }
        }

        /// <summary>
        /// Dumps the info about the mod and lists all installed mods into MOP_REPORT.txt
        /// </summary>
        public static void GenerateReport()
        {
            string gameInfo = GetGameInfo();

            int reportsInFolder = 0;
            while (File.Exists($"{Paths.LogFolder}/{Paths.DefaultReportLogName}_{reportsInFolder}.txt"))
            {
                reportsInFolder++;
            }

            string path = $"{Paths.LogFolder}/{Paths.DefaultReportLogName}_{reportsInFolder}.txt";

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(gameInfo);
            }

            ModConsole.Log("[MOP] Mod report has been successfully generated.");
            Process.Start(path);
        }

        /// <summary>
        /// Generates the report about mod's settings and list of installed mods
        /// </summary>
        /// <returns></returns>
        internal static string GetGameInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Modern Optimization Plugin");
            sb.Append($"Version: ").AppendLine(MOP.ModVersion);
#if PRO
            sb.AppendLine($"MSC Mod Loader Pro Version: {ModLoader.Version}");
#else
            sb.Append($"MSC Mod Loader Version: ").AppendLine(ModLoader.MSCLoader_Ver);
#endif
            sb.Append($"Date and Time: ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine(GetSystemInfo());
            sb.Append($"Game restarts: ").Append(MopSettings.Restarts).AppendLine();
            sb.Append($"Game resolution: ").Append(Screen.width).Append("x").Append(Screen.height).AppendLine();
            sb.Append($"Screen resolution: ").Append(Screen.currentResolution.width).Append("x").Append(Screen.currentResolution.height).AppendLine();
            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                var elapsed = DateTime.Now.Subtract(SessionTimeStart);
                sb.Append("Session time: ").Append(elapsed.Hours).Append(" Hours ").Append(elapsed.Minutes).Append(" Minutes ").Append(elapsed.Seconds).AppendLine(" Seconds");
            }
            sb.Append("CPU: ").Append(SystemInfo.processorType).Append(" (").Append(SystemInfo.processorCount).AppendLine(" cores)");
            sb.Append($"RAM: ").Append(SystemInfo.systemMemorySize).AppendLine(" MB");
            sb.Append($"GPU: ").Append(SystemInfo.graphicsDeviceName).Append(" (").Append(SystemInfo.graphicsMemorySize).AppendLine(" MB VRAM)");
            sb.AppendLine();
            sb.AppendLine("=== MOP SETTINGS ===");
            sb.AppendLine(); 
            sb.Append($"ActiveDistance: ").Append(MOP.ActiveDistance.GetValue()).AppendLine();
            sb.Append($"ActiveDistanceMultiplier: ").Append(MopSettings.ActiveDistanceMultiplicationValue).AppendLine();
            sb.Append($"PerformanceMode: ").Append(MopSettings.Mode).AppendLine();
#if !PRO
            sb.Append($"LimitFramerate: ").Append(MOP.LimitFramerate.GetValue()).AppendLine();
#endif
            sb.Append($"FramerateLimiter: ").Append(MOP.FramerateLimiter.GetValue()).AppendLine();
            sb.Append($"ShadowDistance: ").Append(MOP.ShadowDistance.GetValue()).AppendLine();
            sb.Append($"RunInBackground: ").Append(MOP.KeepRunningInBackground.GetValue()).AppendLine();
            sb.Append($"DynamicDrawDistance: ").Append(MOP.DynamicDrawDistance.GetValue()).AppendLine();
            sb.Append($"RulesAutoUpdate: ").Append(MOP.RulesAutoUpdate.GetValue()).AppendLine();
            sb.Append($"VerifyRules: ").Append(MOP.VerifyRuleFiles.GetValue()).AppendLine();
            sb.Append($"RulesAutoUpdateFrequency: ").Append(MopSettings.GetRuleFilesUpdateDaysFrequency()).AppendLine();
            sb.Append($"RuledDeleteAutomatically: ").Append(MOP.DeleteUnusedRules.GetValue()).AppendLine();
            sb.Append($"DestroyEmptyBottles: ").Append(MOP.DestroyEmptyBottles.GetValue()).AppendLine();
            sb.Append($"DisableEmptyItems: ").Append(MOP.DisableEmptyItems.GetValue()).AppendLine();
            sb.Append($"NoSkidmarks: ").Append(MOP.AlwaysDisableSkidmarks.GetValue()).AppendLine();
            sb.Append($"ToggleVehiclePhysicsOnly: ").Append(RulesManager.Instance.SpecialRules.ToggleAllVehiclesPhysicsOnly).AppendLine();
            sb.Append($"IgnoreModVehicles: ").Append(RulesManager.Instance.SpecialRules.IgnoreModVehicles).AppendLine();
            sb.Append($"CustomRuleFile: ").Append(File.Exists(Path.Combine(MOP.ModConfigPath, "Custom.txt"))).AppendLine();
         
            // Game data
            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                sb.AppendLine();
                sb.AppendLine("=== GAME DATA ===");
                sb.AppendLine();
                sb.AppendLine(GetGameData("PlayerPosition"));
                sb.AppendLine(GetGameData("PlayerHasHayosikoKey"));
                sb.AppendLine(GetGameData("IsPlayerInCar"));
                sb.AppendLine(GetGameData("IsPlayerInSatsuma"));
                sb.AppendLine(GetGameData("DrawDistance"));
                sb.AppendLine(GetGameData("CanTriggerStatus"));
                sb.AppendLine(GetGameData("IsTrailerAttached"));
            }

            sb.AppendLine();
            sb.AppendLine("=== MESSAGES ===");
            sb.AppendLine();
            sb.AppendLine(string.Join("\n", ModConsole.GetMessages().ToArray()));

            // List installed mods.
            sb.AppendLine();
            sb.Append("=== MODS (").Append(ModLoader.LoadedMods.Count - 1).AppendLine(") ===");
            sb.AppendLine();
            foreach (var mod in ModLoader.LoadedMods)
            {
                // Ignore MSCLoader or MOP.
                if (mod.ID.Equals("MOP"))
                    continue;

                sb.AppendLine(mod.Name);
                sb.Append("  ID: ").AppendLine(mod.ID);
                sb.Append("  Version: ").AppendLine(mod.Version);
                sb.Append("  Author: ").AppendLine(mod.Author);
                sb.AppendLine();
            }

            // If only 3 mods have been found, that means the only mods active are MOP and two ModLoader modules.
            if (ModLoader.LoadedMods.Count <= 3)
            {
                sb.AppendLine("No other mods found");
            }

            // List rule files.
            sb.AppendLine()
                .AppendLine("=== RULE FILES ===")
                .AppendLine();
            string[] rulefiles = RulesManager.Instance.Rules.Select(f => f.Filename).Distinct().ToArray();
            sb.AppendLine(string.Join("\n", rulefiles));

            if (RulesManager.Instance.Rules.Count == 0)
            {
                sb.AppendLine("No rule files loaded!");
            }

            if (File.Exists($"{MOP.ModConfigPath}/Custom.txt"))
            {
                sb.AppendLine();
                sb.AppendLine("=== CUSTOM.TXT CONTENT ===");
                sb.AppendLine();
                sb.Append(File.ReadAllText(Path.Combine(MOP.ModConfigPath, "Custom.txt")));                
            }

            return sb.ToString();
        }

        static string GetGameData(string name)
        {
            string output = $"{name}: ";
            try
            {
                switch (name)
                {
                    default:
                        output += "null";
                        break;
                    case "PlayerPosition":
                        output += GameObject.Find("PLAYER").transform.position;
                        break;
                    case "PlayerHasHayosikoKey":
                        output += FSM.FsmManager.PlayerHasHayosikoKey();
                        break;
                    case "IsPlayerInCar":
                        output += FSM.FsmManager.IsPlayerInCar();
                        break;
                    case "IsPlayerInSatsuma":
                        output += FSM.FsmManager.IsPlayerInSatsuma();
                        break;
                    case "DrawDistance":
                        output += FSM.FsmManager.GetDrawDistance();
                        break;
                    case "CanTriggerStatus":
                        output += (Managers.ItemsManager.Instance == null 
                            ? "manager_null" 
                            : Managers.ItemsManager.Instance.GetCanTrigger() == null ? "null" 
                            : $"Found ({Managers.ItemsManager.Instance.GetCanTrigger().Path()})");
                        break;
                    case "IsTrailerAttached":
                        output += FSM.FsmManager.IsTrailerAttached();
                        break;
                }
            }
            catch
            {
                output += "error";
            }

            return output;
        }

        public static string GetSystemInfo()
        {
            string fullOS = SystemInfo.operatingSystem;
            string windowsVersion = GetWindowsName(fullOS);
            
            if (File.Exists("Z:/var/log/syslog"))
            {
                string name = GetLinuxName();
                return $"{name} [Wine, {windowsVersion}]";
            }
            else
            {
                return windowsVersion;
            }
        }

        private static string GetWindowsName(string fullOS)
        {
            int build = int.Parse(fullOS.Split('(')[1].Split(')')[0].Split('.')[2]);
            if (build > 9600)
            {
                string realOS = build >= 22000 ? $"Windows 11 (10.0.{build})" : $"Windows 10 (10.0.{build})";
                if (Directory.Exists("C:\\Program Files (Arm)"))
                {
                    realOS += " ARM";
                }
                else
                {
                    realOS += SystemInfo.operatingSystem.Contains("64bit") ? " 64bit" : " 32bit";
                }

                return realOS;
            }

            return fullOS;
        }

        private static string GetLinuxName()
        {
            string linuxInfoFile = "Z:/etc/os-release";
            string output = "Linux";
            if (File.Exists(linuxInfoFile))
            {
                StreamReader reader = new StreamReader(linuxInfoFile);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Contains("PRETTY_NAME"))
                    {
                        line = line.Replace("PRETTY_NAME=", "");
                        line = line.Replace("\"", "");
                        output = line;
                        break;
                    }
                }

                reader.Close();
            }
            else if (Directory.Exists("Z:/Applications"))
            {
                output = "macOS";
            }

            return output;
        }

        /// <summary>
        /// Deletes the entire logs folder.
        /// </summary>
        public static void DeleteAllLogs()
        {
            string[] files = Directory.GetFiles(Paths.LogFolder, "*.txt");

            if (files.Length == 0)
            {
                ModUI.ShowMessage("No logs exist.", "MOP");
                return;
            }

            ModUI.ShowYesNoMessage($"Are you sure you want to delete <color=yellow>{files.Length}</color> log{(files.Length > 1 ? "s" : "")}?", "MOP", () => DeleteLogFiles(files));
        }

        private static void DeleteLogFiles(string[] files)
        {
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}
