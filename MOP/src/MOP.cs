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
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace MOP
{
    public class MOP : Mod
    {
        public override string ID => "MOP"; //Your mod ID (unique)
#if DEBUG
        public override string Name => "Modern Optimization Plugin (Super Duper Secret Edition)"; //You mod name
#else
        public override string Name => "Modern Optimization Plugin"; //You mod name
#endif
        public override string Author => "Athlon"; //Your Username

        public override string Version => "2.6.3"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => false;
        public override bool SecondPass => true;

        public static string ModConfigPath;
        public static string ModVersion;

        public override void ModSettingsLoaded()
        {
            ModConfigPath = ModLoader.GetModConfigFolder(this);
            MopSettings.UpdateAll();
            ModVersion = Version;
            ModConsole.Print($"<color=green>MOP {ModVersion} initialized!</color>");
            new Rules();
            ConsoleCommand.Add(new PrintRulesCommand());
        }

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void SecondPassOnLoad()
        {
            MopSettings.UpdateAll();

            // Create WorldManager game object
            GameObject worldManager = new GameObject("MOP_WorldManager");

            // Initialize CompatibiliyManager
            new CompatibilityManager();

            // Add WorldManager class
            worldManager.AddComponent<WorldManager>();
        }

        //
        // BUTTONS
        //
        readonly Settings faq = new Settings("faq", "FAQ", OpenFAQDialog);
        readonly Settings wiki = new Settings("wiki", "MOP Wiki", OpenWikiDialog);
        readonly Settings donate = new Settings("donate", "Donate", OpenDonateDialog);

        //
        // ACTIVATING OBJECTS
        //
        public static Settings activeDistance = new Settings("activeDistance", "Active Distance", 1, MopSettings.UpdateAll);
        string[] activeDistanceText = { "Close (0.5x)", "Normal (1x)", "Far (2x)", "Very Far (4x)" };
        public static Settings safeMode = new Settings("safeMode", "Safe Mode (requires restart)", false, MopSettings.UpdateAll);

        //
        // GRAPHICS
        //
        public static Settings enableFramerateLimiter = new Settings("enableFramerateLimiter", "Enable Framerate Limiter", false, MopSettings.UpdateAll);
        public static Settings framerateLimiter = new Settings("framerateLimiter", "Limit Framerate", 60, MopSettings.UpdateAll);

        //
        // MOD RULES
        //
        public static Settings rulesAutoUpdate = new Settings("rulesAutoUpdate", "Rules Auto Update", true, MopSettings.UpdateAll);
        public static Settings rulesAutoUpdateFrequency = new Settings("rulesAutoUpdateFrequency", "Auto Update Frequency", 2);
        string[] rulesAutoUpdateFrequencyText = { "Daily", "Every 2 days", "Weekly" };
        readonly Settings forceRuleUpdate = new Settings("forceRuleUpdate", "Force Update", ForceRuleFilesUpdate);
        readonly Settings rulesLearnMore = new Settings("rulesLearnMore", "Learn More", OpenRulesWebsiteDialog);

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

        //
        // LOGGING
        //
        readonly Settings openOutputLog = new Settings("openOutputLog", "Open output_log.txt", ExceptionManager.OpenOutputLog);
        readonly Settings openLastLog = new Settings("openLastLog", "Open last log", ExceptionManager.Open);
        readonly Settings generateReport = new Settings("generateReport", "Generate mod report", ExceptionManager.GenerateReport);

        readonly Color32 headerColor = new Color32(29, 29, 29, 255);

        /// <summary>
        /// All settings should be created here.
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
#if DEBUG
            Settings.AddHeader(this, "Shh...Don't leak my hard work ;)", Color.yellow, Color.black);
#endif

            if (!ModLoader.CheckSteam())
            {
                Settings.AddHeader(this, "Pirated game copies are not supported.\nUse only at your own risk!", Color.red, Color.black);
            }

            // Links and utilities
            Settings.AddButton(this, faq);
            Settings.AddButton(this, wiki);
            Settings.AddButton(this, donate, new Color32(0, 128, 0, 255), new Color32(0, 255, 0, 255), new Color32(127, 255, 0, 255));

            // Activating Objects
            Settings.AddHeader(this, "Activating Objects", headerColor);
            Settings.AddSlider(this, activeDistance, 0, 3, activeDistanceText);
            Settings.AddCheckBox(this, safeMode);
            Settings.AddText(this, "Safe Mode will only allow to toggle objects that should not cause any issues with game.\n" +
                "Note: this option will dramatically decrease performance!");

            // Graphics
            Settings.AddHeader(this, "Graphics", headerColor);
            Settings.AddCheckBox(this, enableFramerateLimiter);
            Settings.AddSlider(this, framerateLimiter, 20, 120);

            // Mod Rules
            Settings.AddHeader(this, "Mod Rules", headerColor);
            Settings.AddCheckBox(this, rulesAutoUpdate);
            Settings.AddSlider(this, rulesAutoUpdateFrequency, 0, 2, rulesAutoUpdateFrequencyText);
            Settings.AddButton(this, forceRuleUpdate, "This will force MOP to re-download all mod rule files (this may take a while!).");
            Settings.AddButton(this, rulesLearnMore);


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

            // Logging
            Settings.AddHeader(this, "Logging", headerColor);
            Settings.AddButton(this, openOutputLog);
            Settings.AddButton(this, openLastLog);
            Settings.AddButton(this, generateReport);

            // Changelog
            Settings.AddHeader(this, "Changelog", headerColor);
            Settings.AddText(this, GetChangelog());
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

        static void OpenRulesWebsiteDialog()
        {
            ModUI.ShowYesNoMessage("This will open a new web browser window. Are you sure you want to continue?", OpenRulesWebsite);
        }

        static void OpenRulesWebsite()
        {
            Process.Start("http://athlon.kkmr.pl/mop");
        }

        static void ForceRuleFilesUpdate()
        {
            if (ModLoader.GetCurrentScene() != CurrentScene.MainMenu)
            {
                ModUI.ShowMessage("You can only force update while in main menu.");
                return;
            }

            if (File.Exists($"{ModConfigPath}\\LastModList.mop"))
                File.Delete($"{ModConfigPath}\\LastModList.mop");

            if (File.Exists($"{ModConfigPath}\\LastUpdate.mop"))
                File.Delete($"{ModConfigPath}\\LastUpdate.mop");

            new Rules(true);
        }

        /// <summary>
        /// Gets changelog from changelog.txt and adds rich text elements.
        /// </summary>
        /// <returns></returns>
        string GetChangelog()
        {
            string[] changelog = Properties.Resources.changelog.Split('\n');
            string output = "";
            for (int i = 0; i < changelog.Length; i++)
            {
                string line = changelog[i];

                // If line starts with ###, make it look like a header of section.
                if (line.StartsWith("###"))
                {
                    line = line.Replace("###", "");
                    line = $"<color=yellow><size=24>{line}</size></color>";
                }

                // Replace - with bullet.
                if (line.StartsWith("-"))
                {
                    line = line.Substring(1);
                    line = $"• {line}";
                }

                // Similar to the bullet, but also increase the tab.
                if (line.StartsWith("  -"))
                {
                    line = line.Substring(3);
                    line = $"    • {line}";
                }

                output += line + "\n";
            }

            return output;
        }
    }
}
