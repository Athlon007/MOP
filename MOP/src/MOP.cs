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
using System.IO;
using UnityEngine;
using System;

namespace MOP
{
    public class MOP : Mod
    {
        public override string ID => "MOP"; //Your mod ID (unique)
#if DEBUG
        public override string Name => "Modern Optimization Plugin (Debug)"; //You mod name
#else
        public override string Name => "Modern Optimization Plugin"; //You mod name
#endif
        public override string Author => "Athlon"; //Your Username
        public override string Version => "2.9.8"; //Version

        // ModLoader configuration.
        public override bool SecondPass => true;
        public override bool LoadInMenu => true;

        // Stores the config path of mod.
        static string modConfigPath;
        public static string ModConfigPath { get => modConfigPath; }

        // Stores the version of the mod.
        static string modVersion;
        public static string ModVersion { get => modVersion; }

        public override void OnMenuLoad()
        {
            if (ModLoader.IsModPresent("CheatBox"))
            {
                ModConsole.Warning("[MOP] CheatBox is not supported by MOP!");
            }

            modConfigPath = ModLoader.GetModConfigFolder(this).Replace('\\', '/');
            if (!MopSettings.DataSendingAgreed())
            {
                ModUI.ShowMessage($"Welcome to Modern Optimization Plugin <color=yellow>{Version}</color>!\n\n" +
                    $"While using rule files auto updates, MOP sends following data:\n" +
                    "• MOP Version\n" +
                    "• Operating System Version", "MOP");
                MopSettings.AgreeData();
            }

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public override void ModSettingsLoaded()
        {
            MopSettings.UpdateAll();
            modVersion = Version;
            ModConsole.Print($"<color=green>MOP {ModVersion} initialized!</color>");
            new Rules();
            ConsoleCommand.Add(new ConsoleCommands());
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

        // BUTTONS
        readonly Settings faq = new Settings("faq", "FAQ", ExternalExecuting.OpenFAQDialog);
        readonly Settings wiki = new Settings("wiki", "MOP Wiki", ExternalExecuting.OpenWikiDialog);
        readonly Settings donate = new Settings("donate", "Donate", ExternalExecuting.OpenDonateDialog);
        readonly Settings homepage = new Settings("homepage", "Homepage", ExternalExecuting.OpenHomepageDialog);

        // ACTIVATING OBJECTS
        public static Settings ActiveDistance = new Settings("activeDistance", "Active Distance", 1, MopSettings.UpdateAll);
        readonly string[] activeDistanceText = { "Close (0.75x)", "Normal (1x)", "Far (2x)", "Very Far (4x)" };
        public static Settings SafeMode = new Settings("safeMode", "Safe Mode (requires restart)", false, MopSettings.UpdateAll);

        // GRAPHICS
        public static Settings EnableFramerateLimiter = new Settings("enableFramerateLimiter", "Enable Framerate Limiter", false, MopSettings.UpdateAll);
        public static Settings FramerateLimiter = new Settings("framerateLimiter", "Limit Framerate", 60, MopSettings.UpdateAll);
        public static Settings EnableShadowAdjusting = new Settings("enableShadowAdjusting", "Adjust Shadows", false, MopSettings.UpdateAll);
        public static Settings ShadowDistance = new Settings("shadowDistance", "Shadow Distance", 200, MopSettings.UpdateAll);
        public static Settings KeepRunningInBackground = new Settings("keepRunningInBackground", "Run in Background", true, MopSettings.ToggleBackgroundRunning);

        // MOD RULES
        public static Settings RulesAutoUpdate = new Settings("rulesAutoUpdate", "Rules Auto Update", true, MopSettings.UpdateAll);
        public static Settings RulesAutoUpdateFrequency = new Settings("rulesAutoUpdateFrequency", "Auto Update Frequency", 2);
        readonly string[] rulesAutoUpdateFrequencyText = { "On Restart", "Daily", "Every 2 days", "Weekly" };
        readonly Settings forceRuleUpdate = new Settings("forceRuleUpdate", "Force Update", ForceRuleFilesUpdate);
        readonly Settings rulesLearnMore = new Settings("rulesLearnMore", "Learn More", ExternalExecuting.OpenRulesWebsiteDialog);
        public static Settings NoDeleteRuleFiles = new Settings("noDeleteRuleFiles", "Don't delete unused rule files", false, MopSettings.UpdateAll);
        public static Settings VerifyRuleFiles = new Settings("verifyRuleFiles", "Verify Rule Files", true, MopSettings.ToggleVerifyRuleFiles);

        // OTHERS
        public static Settings RemoveEmptyBeerBottles = new Settings("removeEmptyBeerBottles", "Destroy Empty Beer Bottles", false, MopSettings.UpdateAll);

        // LOGGING
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

            if (ModLoader.CheckIfExperimental())
            {
                Settings.AddHeader(this, "Warning: due to frequent updates in Experimental version of the game, " +
                    "expect more bugs, glitches and new features partially or completly not working.", Color.red, Color.black);
            }

            // Links and utilities
            Settings.AddButton(this, faq);
            Settings.AddButton(this, wiki);
            Settings.AddButton(this, donate, new Color32(0, 128, 0, 255), new Color32(0, 255, 0, 255), new Color32(127, 255, 0, 255));

            // Activating Objects
            Settings.AddHeader(this, "Activating Objects", headerColor);
            Settings.AddSlider(this, ActiveDistance, 0, 3, activeDistanceText);
            Settings.AddCheckBox(this, SafeMode);
            Settings.AddText(this, "Safe Mode will only allow to toggle objects that should not cause any issues with game.\n" +
                "Note: this option will dramatically decrease performance!");

            // Graphics
            Settings.AddHeader(this, "Graphics", headerColor);
            Settings.AddCheckBox(this, EnableFramerateLimiter);
            Settings.AddSlider(this, FramerateLimiter, 20, 144);
            Settings.AddCheckBox(this, EnableShadowAdjusting);
            Settings.AddSlider(this, ShadowDistance, 0, 4000);
            Settings.AddCheckBox(this, KeepRunningInBackground);

            // Mod Rules
            Settings.AddHeader(this, "Mod Rules", headerColor);
            Settings.AddButton(this, rulesLearnMore);
            Settings.AddCheckBox(this, RulesAutoUpdate);
            Settings.AddSlider(this, RulesAutoUpdateFrequency, 0, 3, rulesAutoUpdateFrequencyText);
            Settings.AddCheckBox(this, NoDeleteRuleFiles);
            Settings.AddButton(this, forceRuleUpdate, "This will force MOP to re-download all mod rule files.");
            Settings.AddCheckBox(this, VerifyRuleFiles);

            // Others
            Settings.AddHeader(this, "Other", headerColor);
            Settings.AddCheckBox(this, RemoveEmptyBeerBottles);

            // Logging
            Settings.AddHeader(this, "Logging", headerColor);
            Settings.AddButton(this, openOutputLog);
            Settings.AddButton(this, openLastLog);
            Settings.AddButton(this, generateReport);

            // Changelog
            Settings.AddHeader(this, "Changelog", headerColor);
            Settings.AddText(this, GetChangelog());

            // Info
            Settings.AddHeader(this, "Information", headerColor);
            Settings.AddText(this, $"<color=yellow>MOP</color> {Version}{(Version.EndsWith(".0") ? " Development" : "")}\n" +
                $"<color=yellow>ModLoader</color> {ModLoader.MSCLoader_Ver}\n" +
                $"{ExceptionManager.GetSystemInfo()}\n" +
                $"\nCopyright © Konrad Figura 2019-{DateTime.Now.Year}");
            Settings.AddButton(this, homepage);
        }

        static void ForceRuleFilesUpdate()
        {
            if (ModLoader.GetCurrentScene() != CurrentScene.MainMenu)
            {
                ModUI.ShowMessage("You can only force update while in main menu.");
                return;
            }

            if (File.Exists($"{ModConfigPath}/LastModList.mop"))
                File.Delete($"{ModConfigPath}/LastModList.mop");

            if (File.Exists($"{ModConfigPath}/LastUpdate.mop"))
                File.Delete($"{ModConfigPath}/LastUpdate.mop");

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

                if (line.Contains("(Development)"))
                {
                    line = line.Replace("(Development)", "<color=orange>Development: </color>");
                }

                if (line.Contains("(My Summer Car Bug)"))
                {
                    line = line.Replace("(My Summer Car Bug)", "<color=green>My Summer Car Bug: </color>");
                }

                output += line + "\n";
            }

            return output;
        }
    }
}
