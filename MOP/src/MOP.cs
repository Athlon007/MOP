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
#if DEBUG
        public override string Name => "Modern Optimization Plugin (DEBUG. DO NOT DISTRIBUTE!)"; //You mod name
#else
        public override string Name => "Modern Optimization Plugin"; //You mod name
#endif
        public override string Author => "Athlon"; //Your Username

        public override string Version => "2.5"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => false;
        public override bool SecondPass => true;
        public override bool LoadInMenu => true;

        public override void OnMenuLoad()
        {
            new RuleFiles(ModLoader.GetModConfigFolder(this));
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

            // Add WorldManager class
            worldManager.AddComponent<WorldManager>();
        }

        //
        // BUTTONS
        //
        readonly Settings donate = new Settings("donate", "Donate", OpenDonateDialog);
        readonly Settings openLastLog = new Settings("openLastLog", "Open last log", ExceptionManager.Open);
        readonly Settings generateReport = new Settings("generateReport", "Generate mod report", ExceptionManager.GenerateReport);
        readonly Settings faq = new Settings("faq", "FAQ", OpenFAQDialog);
        readonly Settings wiki = new Settings("wiki", "MOP Wiki", OpenWikiDialog);

        //
        // ACTIVATING OBJECTS
        //
        public static Settings activeDistance = new Settings("activeDistance", "Active Distance", 1, MopSettings.UpdateAll);
        public static Settings safeMode = new Settings("safeMode", "Safe Mode (requires restart)", false, MopSettings.UpdateAll);

        //
        // GRAPHICS
        //
        public static Settings enableFramerateLimiter = new Settings("enableFramerateLimiter", "Enable Framerate Limiter", false, MopSettings.UpdateAll);
        public static Settings framerateLimiter = new Settings("framerateLimiter", "Limit Framerate", 60, MopSettings.UpdateAll);

        //
        // OTHERS
        //
        public static Settings removeEmptyBeerBottles = new Settings("removeEmptyBeerBottles", "Destroy Empty Beer Bottles", false, MopSettings.UpdateAll);
        public static Settings satsumaTogglePhysicsOnly = new Settings("satsumaTogglePhysicsOnly", "SATSUMA: Toggle Physics Only", false, MopSettings.UpdateAll);

        //
        // ADVANCED
        //
        public static Settings ignoreModVehicles = new Settings("ignoreModVehicles", "Ignore Mod Vehicles", false, MopSettings.UpdateAll);
        public static Settings toggleVehiclePhysicsOnly = new Settings("toggleVehiclePhysicsOnly", "Toggle Vehicles Physics Only", false, MopSettings.UpdateAll);

        readonly Color32 headerColor = new Color32(29, 29, 29, 255);

        /// <summary>
        /// All settings should be created here.
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
#if DEBUG
            Settings.AddHeader(this, "Debug version. DO NOT DISTRIBUTE!", Color.red, Color.black);
#endif

            // Links and utilities
            Settings.AddButton(this, openLastLog);
            Settings.AddButton(this, generateReport);
            Settings.AddButton(this, faq);
            Settings.AddButton(this, wiki);
            Settings.AddButton(this, donate, new Color32(0, 128, 0, 255), new Color32(0, 255, 0, 255), new Color32(127, 255, 0, 255));

            // Activating Objects
            Settings.AddHeader(this, "Activating Objects", headerColor);
            Settings.AddSlider(this, activeDistance, 0, 3);
            Settings.AddText(this, "From how far objects are disabled.\n - 0: Close (0.5x)\n - 1: Normal (Default)\n - 2: Far (2x)\n - 3: Very Far (4x)");
            Settings.AddCheckBox(this, safeMode);
            Settings.AddText(this, "Safe Mode will only allow to toggle objects that are known to not to cause any issues.\n" +
                "Note: this option will dramatically decrease performance!");

            // Graphics
            Settings.AddHeader(this, "Graphics", headerColor);
            Settings.AddCheckBox(this, enableFramerateLimiter);
            Settings.AddSlider(this, framerateLimiter, 20, 120);

            // Others
            Settings.AddHeader(this, "Other", headerColor);
            Settings.AddCheckBox(this, removeEmptyBeerBottles);
            Settings.AddCheckBox(this, satsumaTogglePhysicsOnly);
            Settings.AddText(this, "May fix issues with disappearing body panels in some cases.\n" +
                "Note: this will decrease the performance");

            // Advanced
            Settings.AddHeader(this, "Advanced", headerColor);
            Settings.AddCheckBox(this, ignoreModVehicles);
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

        static void OpenDonateDialog()
        {
            ModUI.ShowYesNoMessage("This will open a new web browser window. Are you sure you want to continue?", OpenDonate);
        }

        static void OpenDonate()
        {
            Process.Start("https://paypal.me/figurakonrad");
        }
    }
}
