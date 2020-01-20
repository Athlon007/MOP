using System;
using System.IO;
using System.Diagnostics;

namespace MOP
{
    class ErrorHandler
    {
        /// <summary>
        /// Creates then new error dump file
        /// </summary>
        /// <param name="ex"></param>
        public static void New(Exception ex)
        {
            if (File.Exists("MOP_LOG.txt"))
                File.Delete("MOP_LOG.txt");

            string gameInfo = GetGameInfo();
            string errorInfo = ex.Message + "\n" + ex.StackTrace + "\nTarget Site: " + ex.TargetSite;

            using (StreamWriter sw = new StreamWriter("MOP_LOG.txt"))
            {
                sw.Write(gameInfo + "\n=== ERROR ===\n\n" + errorInfo);
                sw.Close();
            }

            MSCLoader.ModConsole.Error("[MOP] An error has occured. " +
                "Log has been saved in My Summer Car folder into MOP_LOG.txt\n\n" + errorInfo);
        }

        public static void Open()
        {
            if (File.Exists("MOP_LOG.txt"))
            {
                Process.Start("MOP_LOG.txt");
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
            }

            MSCLoader.ModConsole.Print("[MOP] Mod report has been generated.");
            Process.Start("MOP_REPORT.txt");
        }

        /// <summary>
        /// Generates the report about mod's settings and list of installed mods
        /// </summary>
        /// <returns></returns>
        static string GetGameInfo()
        {
            string output = "";
            output += "MSC Mod Loader Version: " + MSCLoader.ModLoader.MSCLoader_Ver + "\n";
            output += "Date and Time: " + DateTime.Now.ToString("ddd, dd.MM.yyyy H:mm") + "\n\n";

            output += "=== MOP SETTINGS ===\n\n";

            output += "ActiveDistance: " + MopSettings.ActiveDistance + "\n";
            output += "ActiveDistanceMultiplicationValue: " + MopSettings.ActiveDistanceMultiplicationValue + "\n";
            output += "SafeMode: " + MopSettings.SafeMode.ToString() + (MopSettings.PlayerIsNotAPirateScum == false ? "n't" : "") + "\n";
            output += "ToggleVehicles: " + MopSettings.ToggleVehicles.ToString() + "\n";
            output += "ToggleItems: " + MopSettings.ToggleItems.ToString() + "\n";
            output += "RemoveEmptyBeerBottles: " + MopSettings.RemoveEmptyBeerBottles.ToString() + "\n";
            output += "SatsumaTogglePhysicsOnly: " + MopSettings.SatsumaTogglePhysicsOnly.ToString() + "\n";
            output += "OverridePhysicsToggling: " + MopSettings.OverridePhysicsToggling.ToString() + "\n";
            output += "EnableObjectOcclusion: " + MopSettings.EnableObjectOcclusion.ToString() + "\n";
            if (MopSettings.EnableObjectOcclusion)
            {
                output += "OcclusionSamples: " + MopSettings.OcclusionSamples + "\n";
                output += "ViewDistance: " + MopSettings.ViewDistance + "\n";
                output += "OcclusionSampleDelay: " + MopSettings.OcclusionSampleDelay + "\n";
                output += "OcclusionHideDelay: " + MopSettings.OcclusionHideDelay + "\n";
                output += "MinOcclusionDistance: " + MopSettings.MinOcclusionDistance + "\n";
                output += "OcclusionMethod: " + MopSettings.OcclusionMethod + "\n";
            }

            // List installed mods
            output += "\n=== MODS ====\n\n";
            foreach (var mod in MSCLoader.ModLoader.LoadedMods)
            {
                if (mod.ID == "MOP")
                {
                    output = $"{mod.Name}\nVersion: {mod.Version}\n" + output;
                    continue;
                }

                if (mod.ID == "MSCLoader_Console" || mod.ID == "MSCLoader_Settings") 
                    continue;

                output += $"{mod.Name}:\n  ID: {mod.ID}\n  Version: {mod.Version}\n  Author: {mod.Author}\n\n";
            }

            return output;
        }
    }
}
