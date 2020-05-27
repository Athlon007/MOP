// Modern Optimization Plugin
// Copyright(C) 2019-2020 Athlon

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
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace MOP
{
    class ExceptionManager
    {
        static bool isLogSaved;

        /// <summary>
        /// Creates then new error dump file
        /// </summary>
        /// <param name="ex"></param>
        public static void New(Exception ex, string message = "")
        {
            if (isLogSaved)
                return;

            if (File.Exists("MOP_LOG.txt"))
                File.Delete("MOP_LOG.txt");

            string gameInfo = GetGameInfo();
            string errorInfo = $"{ex.Message}\n{ex.StackTrace}\nTarget Site: {ex.TargetSite}";

            using (StreamWriter sw = new StreamWriter("MOP_LOG.txt"))
            {
                sw.Write($"{gameInfo}\n=== ERROR ===\n\n{message}{(message.Length > 0 ? "\n\n" : "")}{errorInfo}");
                sw.Close();
                sw.Dispose();
            }

            ModConsole.Error("[MOP] An error has occured. " +
                "Log has been saved in My Summer Car folder into MOP_LOG.txt.");

            isLogSaved = true;
        }

        public static void Open()
        {
            if (File.Exists("MOP_LOG.txt"))
            {
                Process.Start("MOP_LOG.txt");
            }
        }

        public static void OpenOutputLog()
        {
            if (File.Exists("mysummercar_Data/output_log.txt"))
            {
                Process.Start("mysummercar_Data/output_log.txt");
            }
        }

        /// <summary>
        /// Dumps the info about the mod and lists all installed mods into MOP_REPORT.txt
        /// </summary>
        public static void GenerateReport()
        {
            string gameInfo = GetGameInfo();

            using (StreamWriter sw = new StreamWriter("MOP_REPORT.txt"))
            {
                sw.Write(gameInfo);
                sw.Close();
                sw.Dispose();
            }

            ModConsole.Print("[MOP] Mod report has been generated.");
            Process.Start("MOP_REPORT.txt");
        }

        /// <summary>
        /// Generates the report about mod's settings and list of installed mods
        /// </summary>
        /// <returns></returns>
        static string GetGameInfo()
        {
            string output = $"Modern Optimization Plugin\nVersion: {MOP.ModVersion}{(MOP.ModVersion.EndsWith(".0") ? " Development" : "")}\n";
            output += $"MSC Mod Loader Version: {ModLoader.MSCLoader_Ver}\n";
            output += $"Date and Time: {DateTime.Now:yyyy-MM-ddTHH:mm:ssZ}\n";
            output += $"{GetSystemInfo()} \n\n";

            output += "=== MOP SETTINGS ===\n\n";
            output += $"ActiveDistance: {MopSettings.ActiveDistance}\n";
            output += $"ActiveDistanceMultiplicationValue: {MopSettings.ActiveDistanceMultiplicationValue}\n";
            output += $"SafeMode: {MopSettings.SafeMode}\n";
            output += $"RemoveEmptyBeerBottles: {MopSettings.RemoveEmptyBeerBottles}\n";
            output += $"SatsumaTogglePhysicsOnly: {MopSettings.SatsumaTogglePhysicsOnly}\n";
            output += $"ToggleVehiclePhysicsOnly: {Rules.instance.SpecialRules.ToggleAllVehiclesPhysicsOnly}\n";
            output += $"IgnoreModVehicles: {Rules.instance.SpecialRules.IgnoreModVehicles}\n";
            output += $"EnableFramerateLimiter: {((bool)MOP.EnableFramerateLimiter.GetValue())}\n";
            output += $"FramerateLimiter: {MOP.FramerateLimiter.GetValue()}\n";
            output += $"RulesAutoUpdate: {MOP.RulesAutoUpdate.GetValue()}\n"; 
            output += $"RulesAutoUpdateFrequency: {MopSettings.GetRuleFilesUpdateDaysFrequency()}\n";
            output += $"CustomRuleFile: {File.Exists($"{MOP.ModConfigPath}\\Custom.txt")}\n\n";
            output += $"CheckSteam: {ModLoader.CheckSteam()} \n";
            output += $"ExperimentalBranch: {ModLoader.CheckIfExperimental()}\n";

            // List installed mods.
            output += "\n=== MODS ===\n\n";
            foreach (var mod in ModLoader.LoadedMods)
            {
                // Ignore MSCLoader or MOP.
                if (mod.ID.EqualsAny("MSCLoader_Console", "MSCLoader_Settings", "MOP"))
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
            foreach (string ruleFile in Rules.instance.RuleFileNames)
                output += $"{ruleFile}\n";

            if (Rules.instance.RuleFileNames.Count == 0)
            {
                output += $"No rule files loaded!";
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
    }
}
