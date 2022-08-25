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
using MOP.Rules.Types;

namespace MOP.Common
{
    class ExceptionManager
    {
        static readonly List<string> erorrsContainer = new List<string>();

        public static DateTime SessionTimeStart;
        static string currentLogFile;

        /// <summary>
        /// Creates then new error dump file
        /// </summary>
        /// <param name="ex"></param>
        public static void New(Exception ex, bool isCritical, string message, string messageBoxText = "")
        {
            // Don't save errors that already occured.
            if (erorrsContainer.Contains(message))
            {
                return;
            }

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

                currentLogFile = fileName + ".txt";
            }


            string logFilePath = Path.Combine(Paths.LogFolder, currentLogFile).Replace("\\", "/");
            string errorInfo = $"({DateTime.Now:HH:mm:ss.fff})\n  Code: {message}\n  Type: {ex.GetType().Name}\n  Message: {ex.Message}{ex.StackTrace}\n  Target Site: {ex.TargetSite}";

            using (StreamWriter sw = new StreamWriter(logFilePath, true))
            {
                sw.Write(errorInfo + "\n\n");
            }

            string errorMessage = 
                $"[MOP] An error has occured. The log file has been saved into:\n\n" +
                $"{logFilePath}.\nMessage: {message}\n\n" +
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

            erorrsContainer.Add(message);

            if (messageBoxText != "")
            {
                message += $"\n\nError Code: {message}";
                ModUI.ShowMessage(message);
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
                reportsInFolder++;

            string path = $"{Paths.LogFolder}/{Paths.DefaultReportLogName}_{reportsInFolder}.txt";

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(gameInfo);
                sw.Close();
                sw.Dispose();
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
            string output = $"Modern Optimization Plugin\nVersion: {MOP.ModVersion}\n";
#if PRO
            output += $"MSC Mod Loader Pro Version: {ModLoader.Version}\n";
#else
            output += $"MSC Mod Loader Version: {ModLoader.MSCLoader_Ver}\n";
#endif
            output += $"Date and Time: {DateTime.Now:yyyy-MM-ddTHH:mm:ssZ}\n";
            output += $"{GetSystemInfo()}\n";
            output += $"Game Restarts: {MopSettings.Restarts}\n";
            output += $"Game resolution: {Screen.width}x{Screen.height}\n";
            output += $"Screen resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}\n";
            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                var elapsed = DateTime.Now.Subtract(SessionTimeStart);
                output += $"Session Time: {elapsed.Hours} Hours {elapsed.Minutes} Minutes {elapsed.Seconds} Seconds\n";
            }
            output += $"CPU: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)\n";
            output += $"RAM: {SystemInfo.systemMemorySize} MB\n";
            output += $"GPU: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} MB VRAM)";

            output += "\n\n=== MOP SETTINGS ===\n\n";
            output += $"ActiveDistance: {MOP.ActiveDistance.GetValue()}\n";
            output += $"ActiveDistanceMultiplier: {MopSettings.ActiveDistanceMultiplicationValue}\n";
            output += $"PerformanceMode: {MopSettings.Mode}\n";
#if !PRO
            output += $"LimitFramerate: {MOP.LimitFramerate.GetValue()}\n";
#endif
            output += $"FramerateLimiter: {MOP.FramerateLimiter.GetValue()}\n";
            output += $"ShadowDistance: {MOP.ShadowDistance.GetValue()}\n";
            output += $"RunInBackground: {MOP.KeepRunningInBackground.GetValue()}\n";
            output += $"DynamicDrawDistance: {MOP.DynamicDrawDistance.GetValue()}\n";
            output += $"RulesAutoUpdate: {MOP.RulesAutoUpdate.GetValue()}\n";
            output += $"VerifyRules: {MOP.VerifyRuleFiles.GetValue()}\n";
            output += $"RulesAutoUpdateFrequency: {MopSettings.GetRuleFilesUpdateDaysFrequency()}\n";
            output += $"RuledDeleteAutomatically: {MOP.DeleteUnusedRules.GetValue()}\n";
            output += $"DestroyEmptyBottles: {MOP.DestroyEmptyBottles.GetValue()}\n";
            output += $"DisableEmptyItems: {MOP.DisableEmptyItems.GetValue()}\n";
            output += $"NoSkidmarks: {MOP.AlwaysDisableSkidmarks.GetValue()}\n";
            output += $"FastLoading: {MOP.FastLoading.GetValue()}\n";
            output += $"ToggleVehiclePhysicsOnly: {RulesManager.Instance.SpecialRules.ToggleAllVehiclesPhysicsOnly}\n";
            output += $"IgnoreModVehicles: {RulesManager.Instance.SpecialRules.IgnoreModVehicles}\n";
            output += $"CustomRuleFile: {File.Exists($"{MOP.ModConfigPath}/Custom.txt")}\n";

            // Game data
            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                output += "\n=== GAME DATA ===\n\n";
                output += GetGameData("PlayerPosition");
                output += GetGameData("PlayerHasHayosikoKey");
                output += GetGameData("IsPlayerInCar");
                output += GetGameData("IsPlayerInSatsuma");
                output += GetGameData("DrawDistance");
                output += GetGameData("CanTriggerStatus");
                output += GetGameData("IsTrailerAttached");
            }

            output += "\n=== MESSAGES ===\n\n";
            output += string.Join("\n", ModConsole.GetMessages().ToArray());

            // List installed mods.
            output += $"\n\n=== MODS ({ModLoader.LoadedMods.Count - 1}) ===\n\n";
            foreach (var mod in ModLoader.LoadedMods)
            {
                // Ignore MSCLoader or MOP.
                if (mod.ID.Equals("MOP"))
                    continue;

                output += $"{mod.Name}:\n  ID: {mod.ID}\n  Version: {mod.Version}\n  Author: {mod.Author}\n\n";
            }

            // If only 3 mods have been found, that means the only mods active are MOP and two ModLoader modules.
            if (ModLoader.LoadedMods.Count <= 3)
            {
                output += "No other mods found!\n\n";
            }

            // List rule files.
            output += "=== RULE FILES ===\n\n";
            List<string> files = new List<string>();
            foreach (Rule ruleFile in RulesManager.Instance.Rules)
            {
                if (!files.Contains(ruleFile.Filename))
                {
                    files.Add(ruleFile.Filename);
                }
            }
            output += string.Join("\n", files.ToArray());

            if (RulesManager.Instance.Rules.Count == 0)
            {
                output += $"No rule files loaded!\n";
            }

            if (File.Exists($"{MOP.ModConfigPath}/Custom.txt"))
            {
                output += "\n\n=== CUSTOM.TXT CONTENT ===\n\n";
                output += File.ReadAllText($"{MOP.ModConfigPath}/Custom.txt") + "\n\n";
            }

            return output;
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
                        output += (Managers.ItemsManager.Instance == null ? "manager_null" : Managers.ItemsManager.Instance.GetCanTrigger() == null ? "null" : $"Found ({Managers.ItemsManager.Instance.GetCanTrigger().Path()})");
                        break;
                    case "IsTrailerAttached":
                        output += FSM.FsmManager.IsTrailerAttached();
                        break;
                }

                output += "\n";
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

        static string GetWindowsName(string fullOS)
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

        static string GetLinuxName()
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
            else if (Directory.Exists("Z:/AppleInternal"))
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

        static void DeleteLogFiles(string[] files)
        {
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}
