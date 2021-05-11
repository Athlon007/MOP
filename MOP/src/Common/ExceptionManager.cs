// Modern Optimization Plugin
// Copyright(C) 2019-2021 Athlon

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

namespace MOP.Common
{
    class ExceptionManager
    {
        public const string LogFolderName = "MOP_Logs";
        const string DefaultErrorLogName = "MOP_Crash";
        const string DefaultReportLogName = "MOP_Report";

        static List<string> erorrsContainer = new List<string>();

        public static DateTime SessionTimeStart;

        /// <summary>
        /// Creates then new error dump file
        /// </summary>
        /// <param name="ex"></param>
        public static void New(Exception ex, bool isCritical, string message)
        {
            // Don't save errors that already occured.
            if (erorrsContainer.Contains(message))
                return;

            string fileName = $"{DefaultErrorLogName}_{DateTime.Now:yyyy-MM-dd-HH-mm}";

            if (File.Exists($"{LogFolder}/{fileName}.txt"))
            {
                int crashesInFolder = 0;
                while (File.Exists($"{LogFolder}/{fileName}_{crashesInFolder}.txt"))
                    crashesInFolder++;

                fileName += $"_{crashesInFolder}";
            }

            string logFilePath = $"{LogFolder}/{fileName}.txt";
            string gameInfo = GetGameInfo();
            string errorInfo = $"{ex.Message}\n{ex.StackTrace}\nTarget Site: {ex.TargetSite}";

            using (StreamWriter sw = new StreamWriter(logFilePath))
            {
                sw.Write($"{gameInfo}\n=== ERROR ===\n\n// " +
                         $"{WittyComments.GetErrorWittyText()}\n\n" +
                         $"{message}{(message.Length > 0 ? "\n\n" : "")}" +
                         $"{errorInfo}");
            }

            string errorMessage = $"[MOP] An error has occured. Log has been saved into My Summer Car folder into:\n\n{logFilePath}.\n\n" +
                                  $"Please go into MOP Settings and click \"<b>I found a bug</b>\" button, in order to generate bug report and then follow the instructions.\n";

            if (isCritical)
            {
                ModConsole.LogError(errorMessage);
            }
            else
            {
                ModConsole.LogWarning(errorMessage + "\nYou can continue playing.");
            }

            erorrsContainer.Add(message);
        }

        public static void OpenCurrentSessionLogFolder()
        {
            if (ThisSessionLogDirectoryExists)
            {
                Process.Start(LogFolder);
            }
            else
            {
                ModPrompt.CreatePrompt("Logs folder is doesn't exist.", "MOP");
            }
        }

        public static void OpenOutputLog()
        {
            if (File.Exists(OutputLogPath))
            {
                Process.Start(OutputLogPath);
            }
            else
            {
                ModPrompt.CreatePrompt("File \"output_log.txt\" doesn't exist.", "MOP");
            }
        }

        /// <summary>
        /// Dumps the info about the mod and lists all installed mods into MOP_REPORT.txt
        /// </summary>
        public static void GenerateReport()
        {
            string gameInfo = GetGameInfo();

            int reportsInFolder = 0;
            while (File.Exists($"{LogFolder}/{DefaultReportLogName}_{reportsInFolder}.txt"))
                reportsInFolder++;

            string path = $"{LogFolder}/{DefaultReportLogName}_{reportsInFolder}.txt";

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
            output += $"MSC Mod Loader Pro Version: {ModLoader.Version}\n";
            output += $"Date and Time: {DateTime.Now:yyyy-MM-ddTHH:mm:ssZ}\n";
            output += $"{GetSystemInfo()}\n";
            output += $"Session ID: {MOP.SessionID}\n";
            output += $"Game resolution: {Screen.width}x{Screen.height}\n";
            output += $"Screen resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}\n";
            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                var elapsed = DateTime.Now.Subtract(SessionTimeStart);
                output += $"Session Time: {elapsed.Hours} Hours {elapsed.Minutes} Minutes {elapsed.Seconds} Seconds\n\n";
            }
            else
            {
                output += "\n";
            }

            output += "=== MOP SETTINGS ===\n\n";
            output += $"ActiveDistance: {MOP.ActiveDistance.Value}\n";
            output += $"ActiveDistanceMultiplicationValue: {MopSettings.ActiveDistanceMultiplicationValue}\n";
            output += $"Mode: {MopSettings.Mode}\n";
            output += $"DestroyEmptyBottles: {MOP.DestroyEmptyBottles.Value}\n";
            output += $"DisableEmptyItems: {MOP.DisableEmptyItems}\n";
            output += $"ToggleVehiclePhysicsOnly: {RulesManager.Instance.SpecialRules.ToggleAllVehiclesPhysicsOnly}\n";
            output += $"IgnoreModVehicles: {RulesManager.Instance.SpecialRules.IgnoreModVehicles}\n";
            output += $"EnableFramerateLimiter: {(int)MOP.FramerateLimiter.Value == 21}\n";
            output += $"FramerateLimiter: {(int)MOP.FramerateLimiter.Value + "0"}\n";
            output += $"EnableShadowAdjusting: {MOP.EnableShadowAdjusting.Value}\n";
            output += $"KeepRunningInBackground: {MOP.KeepRunningInBackground.Value}\n";
            output += $"DynamicDrawDistance: {MOP.DynamicDrawDistance.Value}\n";
            output += $"ShadowDistance: {MOP.ShadowDistance.Value}\n";
            output += $"RulesAutoUpdate: {MOP.RulesAutoUpdate.Value}\n";
            output += $"RulesAutoUpdateFrequency: {MopSettings.GetRuleFilesUpdateDaysFrequency()}\n";
            output += $"CustomRuleFile: {File.Exists($"{MOP.ModConfigPath}/Custom.txt")}\n\n";

            // Steam stuff.
            output += $"ExperimentalBranch: {ModLoader.CheckIfExperimental()}\n";

            // Game data
            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                try
                {
                    output += "\n=== GAME DATA ===\n\n";
                    output += $"PlayerPosition: {GameObject.Find("PLAYER").transform.position}\n";
                    output += $"PlayerHasHayosikoKey: {FSM.FsmManager.PlayerHasHayosikoKey()}\n";
                    output += $"IsPlayerInCar: {FSM.FsmManager.IsPlayerInCar()}\n";
                    output += $"IsPlayerInSatsuma: {FSM.FsmManager.IsPlayerInSatsuma()}\n";
                    output += $"DrawDistance: {FSM.FsmManager.GetDrawDistance()}\n";
                    output += $"CanTriggerStatus: {(Managers.ItemsManager.Instance == null ? "manager_null" : Managers.ItemsManager.Instance.GetCanTrigger() == null ? "null" : $"Found ({Managers.ItemsManager.Instance.GetCanTrigger().GetGameObjectPath()})")}\n";
                    output += $"IsTrailerAttached: {FSM.FsmManager.IsTrailerAttached()}\n";
                }
                catch
                {
                    output += "ERROR GETTING GAME DATA!";
                }

                if (FramerateRecorder.Instance != null)
                {
                    output += $"AverageFramerate: {FramerateRecorder.Instance.GetAverage()} FPS\n";
                }
            }

            // List installed mods.
            output += $"\n=== MODS ({ModLoader.LoadedMods.Count}) ===\n\n";
            foreach (var mod in ModLoader.LoadedMods)
            {
                // Ignore MSCLoader or MOP.
                if (mod.ID.EqualsAny("MSCLoader_Console", "MSCLoader_Settings", "MOP"))
                    continue;

                output += $"{mod.Name}:\n  ID: {mod.ID}\n  Version: {mod.Version}\n  Author: {mod.Author}\n  Enabled: {mod.Enabled}\n\n";
            }

            // If only 3 mods have been found, that means the only mods active are MOP and two ModLoader modules.
            if (ModLoader.LoadedMods.Count <= 3)
            {
                output += "No other mods found!\n\n";
            }

            // List rule files.
            output += "=== RULE FILES ===\n\n";
            foreach (string ruleFile in RulesManager.Instance.RuleFileNames)
                output += $"{ruleFile}\n";

            if (RulesManager.Instance.RuleFileNames.Count == 0)
            {
                output += $"No rule files loaded!\n";
            }

            if (File.Exists($"{MOP.ModConfigPath}/Custom.txt"))
            {
                output += "\n=== CUSTOM.TXT CONTENT ===\n\n";
                output += File.ReadAllText($"{MOP.ModConfigPath}/Custom.txt") + "\n\n";
            }

            return output;
        }

        public static string GetSystemInfo()
        {
            string fullOS = SystemInfo.operatingSystem;
            int build = int.Parse(fullOS.Split('(')[1].Split(')')[0].Split('.')[2]);
            if (build > 9600)
            {
                string realOS = $"Windows 10 (10.0.{build})";

                if (SystemInfo.operatingSystem.Contains("64bit"))
                {
                    realOS += " 64bit";
                }

                return realOS;
            }

            return fullOS;
        }

        /// <summary>
        /// Returns MOP log folder, ex.: C:\My Summer Car\MOP_Logs
        /// </summary>
        /// <returns></returns>
        internal static string LogFolder
        {
            get
            {
                string path = $"{RootPath}/{LogFolderName}";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        static bool ThisSessionLogDirectoryExists => Directory.Exists($"{RootPath}/{LogFolder}");

        internal static string RootPath => Application.dataPath.Replace("mysummercar_Data", "");

        /// <summary>
        /// Deletes the entire logs folder.
        /// </summary>
        public static void DeleteAllLogs()
        {
            if (!Directory.Exists($"{RootPath}/{LogFolder}"))
            {
                ModPrompt.CreatePrompt("Log folder doesn't exist.", "MOP");
                return;
            }

            ModPrompt.CreateYesNoPrompt("Are you sure you want to delete all logs?", "MOP", () => Directory.Delete($"{RootPath}/{LogFolder}", true));
        }

        static string OutputLogPath => $"{RootPath}/output_log.txt";
    }
}
