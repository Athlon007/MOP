using MSCLoader;
using UnityEngine;

namespace MOP
{
    public class MOP : Mod
    {
        public override string ID => "MOP"; //Your mod ID (unique)
        public override string Name => "Modern Optimization Plugin (BETA)"; //You mod name
        public override string Author => "Athlon"; //Your Username
        public override string Version => "1.2.0"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => false;
        public override bool SecondPass => true;

        WorldManager worldManager;

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void OnLoad()
        {
            string[] mscLoaderVersion = ModLoader.MSCLoader_Ver.Split('.');

            if (int.Parse(mscLoaderVersion[1]) < 1)
            {
                ModConsole.Error("MOP requires MSC Mod Loader version 1.1.5 or newer. Please update now!");
            }

            if (mscLoaderVersion[2] != null && int.Parse(mscLoaderVersion[2]) < 5)
            {
                ModConsole.Error("MOP requires MSC Mod Loader version 1.1.5 or newer. Please update now!");
            }
        }

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void SecondPassOnLoad()
        {
            MopSettings.UpdateValues();
            
            // Disable the mod, if KruFPS is present
            if (ModLoader.IsModPresent("KruFPS"))
            {
                ModConsole.Error("MOP is not compatbile with KruFPS. Please remove KruFPS first!");
                return;
            }

            // Initialize the WorldManager class
            GameObject worldManager = new GameObject("WorldManager");

            // Initialize CompatibiliyManager
            new CompatibilityManager();

            // Add Occlusion
            if ((bool)enableObjectOcclusion.GetValue() == true)
                worldManager.AddComponent<Occlusion>();

            this.worldManager = worldManager.AddComponent<WorldManager>();
        }

        //
        // BUTTONS
        //
        Settings openLastLog = new Settings("openLastLog", "Open last log", ErrorHandler.Open);
        Settings generateReport = new Settings("generateReport", "Generate mod report", ErrorHandler.GenerateReport);
        Settings faq = new Settings("faq", "FAQ", OpenFAQ);

        //
        // ACTIVATING OBJECTS
        //
        public static Settings activeDistance = new Settings("activeDistance", "Active Distance", 1, MopSettings.UpdateValues);
        // toggles
        public static Settings safeMode = new Settings("safeMode", "Safe Mode (Requires restart)", false, MopSettings.UpdateValues);
        public static Settings toggleVehicles = new Settings("toggleVehicles", "Vehicles", true, MopSettings.UpdateValues);
        public static Settings toggleItems = new Settings("toggleItems", "Shop Items", true, MopSettings.UpdateValues);

        //
        // TRAFFIC DENSITY
        //
        /*
        public static Settings highwayTrafficDensityQuarter = new Settings("highwayTrafficDensityHalf", "Quarter", true, MopSettings.UpdateValues);
        public static Settings highwayTrafficDensityHalf = new Settings("highwayTrafficDensityHalf", "Half", true, MopSettings.UpdateValues);
        public static Settings highwayTrafficDensityMost = new Settings("highwayTrafficDensityMost", "Most (3/4)", true, MopSettings.UpdateValues);
        public static Settings highwayTrafficDensityAll = new Settings("highwayTrafficDensityAll", "All", true, MopSettings.UpdateValues);
        */

        // 
        // OCCLUSION
        //
        public static Settings enableObjectOcclusion = new Settings("enableObjectOcclusion", "Object Occlusion (Requires restart)", false, MopSettings.UpdateValues);
        public static Settings occlusionDistance = new Settings("occlusionDistance", "Occlusion Distance", 400, MopSettings.UpdateValues);
        // Occlussion Sample Detail
        public static Settings occlusionSamplesLowest = new Settings("occlusionSamplesLowest", "Least (Fastest)", false, MopSettings.UpdateValues);
        public static Settings occlusionSamplesLower = new Settings("occlusionSamplesLow", "Less", false, MopSettings.UpdateValues);
        public static Settings occlusionSamplesLow = new Settings("occlusionSamplesLow", "Average", false, MopSettings.UpdateValues);
        public static Settings occlusionSamplesDetailed = new Settings("occlusionSamplesDetailed", "Detailed (Default)", true, MopSettings.UpdateValues);
        public static Settings occlusionSamplesVeryDetailed = new Settings("occlusionSamplesVeryDetailed", "Very Detailed (Slowest)", false, MopSettings.UpdateValues);
        // Delay and Range
        public static Settings occlusionSampleDelay = new Settings("occlusionSampleDelay", "Occlusion Sample Delay", 1, MopSettings.UpdateValues);
        public static Settings minOcclusionDistance = new Settings("minOcclusionDistance", "Minimum Occlusion Distance", 50, MopSettings.UpdateValues);
        // Occlusion Method
        public static Settings occlusionNormal = new Settings("occlusionNormal", "Legacy", false, MopSettings.UpdateValues);
        public static Settings occlusionChequered = new Settings("occlusionChequered", "Checquered (Default)", true, MopSettings.UpdateValues);
        public static Settings occlusionDouble = new Settings("occlusionDouble", "Double Occlusion (Most detailed, CPU heavy)", false, MopSettings.UpdateValues);

        /// <summary>
        /// All settings should be created here. 
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
            // Links and utilities
            Settings.AddButton(this, openLastLog);
            Settings.AddButton(this, generateReport);
            Settings.AddButton(this, faq);

            // Activating Objects
            Settings.AddHeader(this, "Activating Objects", new Color32(29, 29, 29, 255));
            Settings.AddSlider(this, activeDistance, 0, 3);
            Settings.AddText(this, "From how far objects are disabled.\n - 0: Close (0.5x)\n - 1: Normal (Default)\n - 2: Far (2x)\n - 3: Very far(4x)");
            Settings.AddText(this, "Toggled Objects (requires restart):");
            Settings.AddCheckBox(this, toggleVehicles);
            Settings.AddCheckBox(this, toggleItems);
            Settings.AddText(this, "If unchecked, the following objects will not get disabled.\n");
            Settings.AddCheckBox(this, safeMode);
            Settings.AddText(this, "Safe mode will only toggle objects that are known to not cause any issues.");

            // Traffic density
            /*
            Settings.AddHeader(this, "Traffic Density", new Color32(29, 29, 29, 255));
            Settings.AddCheckBox(this, highwayTrafficDensityQuarter, "highwayTrafficDensity");
            Settings.AddCheckBox(this, highwayTrafficDensityHalf, "highwayTrafficDensity");
            Settings.AddCheckBox(this, highwayTrafficDensityMost, "highwayTrafficDensity");
            Settings.AddCheckBox(this, highwayTrafficDensityAll, "highwayTrafficDensity");
            */

            // Occlusion
            Settings.AddHeader(this, "(BETA) Occlusion Culling", new Color32(29, 29, 29, 255));
            Settings.AddText(this, "Occlusion Culling at current stage is highly experimental. Graphical glitches may occure, " +
                "or the mod can crash. It is recommended to try that feature on low end GPUs.");
            Settings.AddCheckBox(this, enableObjectOcclusion);
            Settings.AddSlider(this, occlusionDistance, 200, 3000);
            Settings.AddText(this, "How far the program will send rays which check visible objects. " +
                "Objects that are further than that value will not be checked.\nWARNING: large values" +
                " may cause lag.");
            Settings.AddText(this, "Occlusion Sampling Level of Detail");
            Settings.AddCheckBox(this, occlusionSamplesLowest, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesLower, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesLow, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesDetailed, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesVeryDetailed, "occlusionSampleDetail");
            Settings.AddSlider(this, occlusionSampleDelay, 1, 10);
            Settings.AddText(this, "How often the program will check if objects are visible.");
            Settings.AddSlider(this, minOcclusionDistance, 10, 500);
            Settings.AddText(this, "Minimum distance after which the object's visiblity in camera is checked." +
                " If you're closer to the object than this value, the object will not be hidden, even when you not look at it.");
            Settings.AddText(this, "Occlusion Method");
            Settings.AddCheckBox(this, occlusionNormal, "occlusionMethod");
            Settings.AddCheckBox(this, occlusionChequered, "occlusionMethod");
            Settings.AddCheckBox(this, occlusionDouble, "occlusionMethod");

            // Changelog
            Settings.AddHeader(this, "Changelog", new Color32(29, 29, 29, 255));
            Settings.AddText(this, Properties.Resources.changelog);
        }

        static void OpenFAQ()
        {
            System.Diagnostics.Process.Start("https://github.com/Athlon007/MOP/blob/master/FAQ.md");
        }
    }
}
