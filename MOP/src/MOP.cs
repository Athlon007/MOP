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
using UnityEngine;
using System.Diagnostics;

namespace MOP
{
    public class MOP : Mod
    {
        public override string ID => "MOP"; //Your mod ID (unique)
        public override string Name => "Modern Optimization Plugin"; //You mod name
        public override string Author => "Athlon"; //Your Username
        public override string Version => "2.1.3"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => false;
        public override bool SecondPass => true;

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
        public static Settings safeMode = new Settings("safeMode", "Safe Mode (requires restart)", false, MopSettings.UpdateAll);
        public static Settings ignoreModVehicles = new Settings("ignoreModVehicles", "Ignore Mod Vehicles", false, MopSettings.UpdateAll);

        //
        // OTHERS
        //
        public static Settings removeEmptyBeerBottles = new Settings("removeEmptyBeerBottles", "Remove Empty Beer Bottles", false, MopSettings.UpdateAll);
        public static Settings satsumaTogglePhysicsOnly = new Settings("satsumaTogglePhysicsOnly", "SATSUMA: Toggle Physics Only", false, MopSettings.UpdateAll);
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
        // ADVANCED
        //
        public static Settings toggleVehicles = new Settings("toggleVehicles", "Vehicles", true, MopSettings.UpdateAll);
        public static Settings toggleItems = new Settings("toggleItems", "Shop Items", true, MopSettings.UpdateAll);
        public static Settings toggleVehiclePhysicsOnly = new Settings("toggleVehiclePhysicsOnly", "Toggle Vehicles Physics Only", false, MopSettings.UpdateAll);

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
            Settings.AddCheckBox(this, ignoreModVehicles);

            // Others
            Settings.AddHeader(this, "Other", headerColor);
            Settings.AddCheckBox(this, removeEmptyBeerBottles);
            Settings.AddCheckBox(this, satsumaTogglePhysicsOnly);
            Settings.AddText(this, "May fix issues with disappearing body panels in some cases.\n" +
                "Note: this will decrease the performance boost.");
            Settings.AddButton(this, temporarilyDisablePhysicsToggling, "If your car is stuck in the air (for some reason), " +
                "you can use that button to temporarily disable physics toggling, so it will fall back to the ground.");

            // Occlusion
            Settings.AddHeader(this, "Occlusion Culling - Experimental", headerColor);
            Settings.AddText(this, "Occlusion Culling disables rendering of objects not visible by camera.");
            Settings.AddText(this, "Note: Occlusion Culling is the experimental feature that may cause bigger or smaller graphical glitches!\n");
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
            Settings.AddCheckBox(this, toggleVehiclePhysicsOnly);

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
