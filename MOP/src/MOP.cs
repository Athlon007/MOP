using MSCLoader;
using UnityEngine;
using System.Diagnostics;

namespace MOP
{
    public class MOP : Mod
    {
        public override string ID => "MOP"; //Your mod ID (unique)
        public override string Name => "Modern Optimization Plugin (RC 1)"; //You mod name
        public override string Author => "Athlon"; //Your Username
        public override string Version => "1.7.0"; //Version

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
                ModUI.ShowMessage("MOP requires MSC Mod Loader version 1.1.5 or newer. Please update now!", "[MOP] Error");
            }

            // Check Build number 
            if (mscLoaderVersion[2] == null || int.Parse(mscLoaderVersion[2]) < 5)
            {
                ModUI.ShowMessage("MOP requires MSC Mod Loader version 1.1.5 or newer. Please update now!", "[MOP] Error");
            }

            // Disable the mod, if KruFPS is present
            if (ModLoader.IsModPresent("KruFPS"))
            {
                ModUI.ShowMessage("MOP is not compatbile with KruFPS. Please remove KruFPS first!", "[MOP] Error");
                return;
            }
        }

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void SecondPassOnLoad()
        {
            MopSettings.UpdateAll();

            // Create WorldManager game object
            GameObject worldManager = new GameObject("WorldManager");

            // Initialize CompatibiliyManager
            new CompatibilityManager();

            // Add Occlusion
            if (MopSettings.EnableObjectOcclusion)
                worldManager.AddComponent<Occlusion>();

            // Add WorldManager class
            worldManager.AddComponent<WorldManager>();
        }

        //
        // BUTTONS
        //
        Settings openLastLog = new Settings("openLastLog", "Open last log", ErrorHandler.Open);
        Settings generateReport = new Settings("generateReport", "Generate mod report", ErrorHandler.GenerateReport);
        Settings faq = new Settings("faq", "FAQ", OpenFAQDialog);
        Settings wiki = new Settings("wiki", "Go to MOP wiki", OpenWikiDialog);

        //
        // ACTIVATING OBJECTS
        //
        public static Settings activeDistance = new Settings("activeDistance", "Active Distance", 1, MopSettings.UpdateAll);
        // toggles
        public static Settings safeMode = new Settings("safeMode", "Safe Mode (requires restart)", false, MopSettings.UpdateAll);

        //
        // OTHERS
        //
        public static Settings removeEmptyBeerBottles = new Settings("removeEmptyBeerBottles", "Remove Empty Beer Bottles", false);
        public static Settings satsumaTogglePhysicsOnly = new Settings("satsumaTogglePhysicsOnly", "SATSUMA: Toggle Physics Only", false);
        Settings temporarilyDisablePhysicsToggling = new Settings("temporarilyDisablePhysicsToggling", "Temporarily Disable Physics Toggling", MopSettings.DisablePhysicsToggling);

        // 
        // OCCLUSION
        //
        public static Settings enableObjectOcclusion = new Settings("enableObjectOcclusion", "Object Occlusion (requires restart)", false, MopSettings.UpdateAll);
        // Occlussion Sample Detail
        public static Settings occlusionSamplesLower = new Settings("occlusionSamplesLow", "Lowest (fastest)", false, MopSettings.UpdateAll);
        public static Settings occlusionSamplesLow = new Settings("occlusionSamplesLow", "Low", false, MopSettings.UpdateAll);
        public static Settings occlusionSamplesDetailed = new Settings("occlusionSamplesDetailed", "High (default)", true, MopSettings.UpdateAll);
        public static Settings occlusionSamplesVeryDetailed = new Settings("occlusionSamplesVeryDetailed", "Very High (slowest)", false, MopSettings.UpdateAll);
        // Delay and Range
        public static Settings minOcclusionDistance = new Settings("minOcclusionDistance", "Minimum Distance", 50, MopSettings.UpdateAll);
        public static Settings occlusionDistance = new Settings("occlusionDistance", "Maximum Distance", 400, MopSettings.UpdateAll);
        // Occlusion Method
        public static Settings occlusionChequered = new Settings("occlusionChequered", "Fast (Default)", true, MopSettings.UpdateAll);
        public static Settings occlusionDouble = new Settings("occlusionDouble", "Fancy", false, MopSettings.UpdateAll);

        //
        // CHANGELOG
        //
        public static Settings toggleVehicles = new Settings("toggleVehicles", "Vehicles", true, MopSettings.UpdateAll);
        public static Settings toggleItems = new Settings("toggleItems", "Shop Items", true, MopSettings.UpdateAll);

        readonly Color32 headerColor = new Color32(29, 29, 29, 255);

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
            Settings.AddButton(this, wiki);

            // Activating Objects
            Settings.AddHeader(this, "Activating Objects", headerColor);
            Settings.AddSlider(this, activeDistance, 0, 3);
            Settings.AddText(this, "From how far objects are disabled.\n - 0: Close (0.5x)\n - 1: Normal (Default)\n - 2: Far (2x)\n - 3: Very Far (4x)");
            Settings.AddCheckBox(this, safeMode);
            Settings.AddText(this, "Safe Mode will only allow to toggle objects that are known to not to cause any issues.\n" +
                "Note: framerate boost will be dramatically decreased!");

            // Others
            Settings.AddHeader(this, "Other", headerColor);
            Settings.AddCheckBox(this, removeEmptyBeerBottles);
            Settings.AddCheckBox(this, satsumaTogglePhysicsOnly);
            Settings.AddText(this, "May fix issues with disappearing body panels in some cases.\n" +
                "Note: this will decrease the performance boost.");
            Settings.AddButton(this, temporarilyDisablePhysicsToggling, "If your car is stuck in the air (for some reason), " +
                "you can use that button to temporarily disable physics toggling, so it will fall back to the ground.");

            // Occlusion
            Settings.AddHeader(this, "(BETA) Occlusion Culling", headerColor);
            Settings.AddText(this, "Occlusion Culling disables rendering of objects not visible by camera.");
            Settings.AddText(this, "Note: Occlusion Culling at current stage is in beta. A minor graphical glitches may occure!\n");
            Settings.AddCheckBox(this, enableObjectOcclusion);
            Settings.AddSlider(this, minOcclusionDistance, 10, 500);
            Settings.AddText(this, "Minimum distance after which the object's visiblity in camera is checked." +
                " If you're closer to the object than this value, the object will not be hidden, even when you not look at it.");
            Settings.AddSlider(this, occlusionDistance, 200, 3000);
            Settings.AddText(this, "How far the rays will be sent which check visible objects. " +
                "Objects that are further than that value will not be checked.\nWARNING: large values" +
                " may cause lag.");
            Settings.AddText(this, "Occlusion Sampling Level of Detail");
            Settings.AddCheckBox(this, occlusionSamplesLower, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesLow, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesDetailed, "occlusionSampleDetail");
            Settings.AddCheckBox(this, occlusionSamplesVeryDetailed, "occlusionSampleDetail");
            Settings.AddText(this, "Occlusion Method");
            Settings.AddCheckBox(this, occlusionChequered, "occlusionMethod");
            Settings.AddCheckBox(this, occlusionDouble, "occlusionMethod");

            // Advanced
            Settings.AddHeader(this, "Advanced", headerColor);
            Settings.AddText(this, "Toggled Objects (requires restart):");
            Settings.AddCheckBox(this, toggleVehicles);
            Settings.AddCheckBox(this, toggleItems);
            Settings.AddText(this, "If unchecked, the following objects will not get disabled.\n" +
                "WARNING: Disabling Vehicles without disabling toggled items may cause items to fall through on the ground. " +
                "DO NOT disable any of these, unless you REALLY need to!");

            // Changelog
            Settings.AddHeader(this, "Changelog", headerColor);
            Settings.AddText(this, Properties.Resources.changelog);
        }

        static void OpenFAQDialog()
        {
            ModUI.ShowYesNoMessage("This will open a new web browser window. Are you sure you want to continue?", OpenFAQ);
        }

        static void OpenFAQ()
        {   
            Process.Start("https://github.com/Athlon007/MOP/blob/master/FAQ.md");
        }

        static void OpenWikiDialog()
        {
            ModUI.ShowYesNoMessage("This will open a new web browser window. Are you sure you want to continue?", OpenWiki);
        }

        static void OpenWiki()
        {
            Process.Start("https://github.com/Athlon007/MOP/wiki");
        }
    }
}
