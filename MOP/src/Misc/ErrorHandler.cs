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
            if (File.Exists("mop_err.txt"))
                File.Delete("mop_err.txt");

            using (StreamWriter sw = new StreamWriter("MOP_LOG.txt"))
            {
                sw.Write(ex.ToString());
                sw.Close();
            }

            MSCLoader.ModConsole.Print("An error has occured. " +
                "Log has been saved in My Summer Car folder into MOP_LOG.txt\n\n" + ex.ToString());
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
            string output = "";
            output += "ActiveDistance: " + MopSettings.ActiveDistance + "\n";
            output += "ActiveDistanceMultiplicationValue: " + MopSettings.ActiveDistanceMultiplicationValue + "\n";
            output += "SafeMode: " + MopSettings.SafeMode.ToString() + "\n";
            output += "ToggleVehicles: " + MopSettings.ToggleVehicles.ToString() + "\n";
            output += "ToggleItems: " + MopSettings.ToggleItems.ToString() + "\n";
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

            output += "\nMODS:\n";
            foreach (var mod in MSCLoader.ModLoader.LoadedMods)
            {
                if (mod.ID == "MOP")
                {
                    output = $"{mod.Name}\nVersion: {mod.Version}\n" + output;
                    continue;
                }
                output += $"{mod.Name} ({mod.ID}/{mod.Version})\n";
            }

            using (StreamWriter sw = new StreamWriter("MOP_REPORT.txt"))
            {
                sw.Write(output);
                sw.Close();
            }

            MSCLoader.ModConsole.Print("Mod report has been generated.");
            Process.Start("MOP_REPORT.txt");
        }
    }
}
