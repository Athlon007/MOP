using MSCLoader;
using UnityEngine;
using System.Diagnostics;

namespace MOP
{
    public class MOP : Mod
    {
        public override string ID => "MOP"; //Your mod ID (unique)
        public override string Name => "Modern Optimization Plugin (BETA)"; //You mod name
        public override string Author => "Athlon"; //Your Username
        public override string Version => "1.3.0"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => false;
        public override bool SecondPass => true;

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void OnLoad()
        {
            string[] mscLoaderVersion = ModLoader.MSCLoader_Ver.Split('.');

            // Check Minor number
            if (int.Parse(mscLoaderVersion[1]) < 1)
            {
                ModConsole.Error("[MOP] MOP requires MSC Mod Loader version 1.1.5 or newer. Please update now!");
            }

            // Check Build number
            if (mscLoaderVersion[2] != null && int.Parse(mscLoaderVersion[2]) < 5)
            {
                ModConsole.Error("[MOP] MOP requires MSC Mod Loader version 1.1.5 or newer. Please update now!");
            }

            // Disable the mod, if KruFPS is present
            if (ModLoader.IsModPresent("KruFPS"))
            {
                ModConsole.Error("[MOP] MOP is not compatbile with KruFPS. Please remove KruFPS first!");
                return;
            }
        }

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void SecondPassOnLoad()
        {
            MopSettings.UpdateAll();

            // Create WorldManage game object
            GameObject worldManager = new GameObject("WorldManager");

            // Initialize CompatibiliyManager
            new CompatibilityManager();

            // Add Occlusion
            if ((bool)enableObjectOcclusion.GetValue() == true)
                worldManager.AddComponent<Occlusion>();

            // Add WorldManager class
            worldManager.AddComponent<WorldManager>();
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
        public static Settings activeDistance = new Settings("activeDistance", "Active Distance", 1, MopSettings.UpdateAll);
        // toggles
        public static Settings safeMode = new Settings("safeMode", "Safe Mode (Requires restart)", false, MopSettings.UpdateAll);
        public static Settings toggleVehicles = new Settings("toggleVehicles", "Vehicles", true, MopSettings.UpdateAll);
        public static Settings toggleItems = new Settings("toggleItems", "Shop Items", true, MopSettings.UpdateAll);

        // 
        // OCCLUSION
        //
        public static Settings enableObjectOcclusion = new Settings("enableObjectOcclusion", "Object Occlusion (Requires restart)", false, MopSettings.UpdateAll);
        // Occlussion Sample Detail
        public static Settings occlusionSamplesLowest = new Settings("occlusionSamplesLowest", "Least (Fastest)", false, MopSettings.UpdateAll);
        public static Settings occlusionSamplesLower = new Settings("occlusionSamplesLow", "Less", false, MopSettings.UpdateAll);
        public static Settings occlusionSamplesLow = new Settings("occlusionSamplesLow", "Average", false, MopSettings.UpdateAll);
        public static Settings occlusionSamplesDetailed = new Settings("occlusionSamplesDetailed", "Detailed (Default)", true, MopSettings.UpdateAll);
        public static Settings occlusionSamplesVeryDetailed = new Settings("occlusionSamplesVeryDetailed", "Very Detailed (Slowest)", false, MopSettings.UpdateAll);
        // Delay and Range
        public static Settings minOcclusionDistance = new Settings("minOcclusionDistance", "Minimum Occlusion Distance", 50, MopSettings.UpdateAll);
        public static Settings occlusionDistance = new Settings("occlusionDistance", "Maximum Occlusion Distance", 400, MopSettings.UpdateAll);
        // Occlusion Method
        public static Settings occlusionNormal = new Settings("occlusionNormal", "Legacy", false, MopSettings.UpdateAll);
        public static Settings occlusionChequered = new Settings("occlusionChequered", "Chequered (Default)", true, MopSettings.UpdateAll);
        public static Settings occlusionDouble = new Settings("occlusionDouble", "Double Occlusion (Most detailed, CPU heavy)", false, MopSettings.UpdateAll);

        static Color32 headerColor = new Color32(29, 29, 29, 255);

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
            Settings.AddHeader(this, "Activating Objects", headerColor);
            Settings.AddSlider(this, activeDistance, 0, 3);
            Settings.AddText(this, "From how far objects are disabled.\n - 0: Close (0.5x)\n - 1: Normal (Default)\n - 2: Far (2x)\n - 3: Very Far (4x)");
            Settings.AddText(this, "Toggled Objects (requires restart):");
            Settings.AddCheckBox(this, toggleVehicles);
            Settings.AddCheckBox(this, toggleItems);
            Settings.AddText(this, "If unchecked, the following objects will not get disabled.\n");
            Settings.AddCheckBox(this, safeMode);
            Settings.AddText(this, "Safe mode will only toggle objects that are known to not cause any issues.");

            // Occlusion
            Settings.AddHeader(this, "(BETA) Occlusion Culling", headerColor);
            Settings.AddText(this, "Occlusion Culling at current stage is in beta. A minor graphical glitches " +
                "may occure!");
            Settings.AddCheckBox(this, enableObjectOcclusion);
            Settings.AddSlider(this, minOcclusionDistance, 10, 500);
            Settings.AddText(this, "Minimum distance after which the object's visiblity in camera is checked." +
                " If you're closer to the object than this value, the object will not be hidden, even when you not look at it.");
            Settings.AddSlider(this, occlusionDistance, 200, 3000);
            Settings.AddText(this, "How far the rays will be sent which check visible objects. " +
                "Objects that are further than that value will not be checked.\nWARNING: large values" +
                " may cause lag.");
            Settings.AddText(this, "Occlusion Sampling Level of Detail");
            Settings.AddCheckBox(this, occlusionSamplesLowest, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesLower, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesLow, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesDetailed, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesVeryDetailed, "occlusionSampleDetail");
            Settings.AddText(this, "Occlusion Method");
            Settings.AddCheckBox(this, occlusionNormal, "occlusionMethod");
            Settings.AddCheckBox(this, occlusionChequered, "occlusionMethod");
            Settings.AddCheckBox(this, occlusionDouble, "occlusionMethod");

            // Changelog
            Settings.AddHeader(this, "Changelog", headerColor);
            Settings.AddText(this, Properties.Resources.changelog);
        }

        static void OpenFAQ()
        {
            Process.Start("https://github.com/Athlon007/MOP/blob/master/FAQ.md");
        }
    }
}
