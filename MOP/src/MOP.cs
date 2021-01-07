// Modern Optimization Plugin
// Copyright(C) 2019-2021 Athlon

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

using MOP.FSM;
using MOP.Common;
using MOP.Helpers;
using MOP.Rules;

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
        public override string Version => "3.0.1"; //Version
        public const string SubVersion = "BETA_1"; // NIGHTLY-yyyymmdd | BETA_x | RC_x

        #region Settings & Configuration
        // ModLoader configuration.
        public override bool SecondPass => true;
        public override bool LoadInMenu => true;

        // Stores the config path of mod.
        static string modConfigPath;
        public static string ModConfigPath { get => modConfigPath; }

        // Stores version number of the mod.
        static string modVersion;
        public static string ModVersion { get => modVersion + (SubVersion != "" ? "_" + SubVersion : ""); }
        public static string ModVersionShort { get => modVersion; }

        // BUTTONS
        readonly Settings faq = new Settings("faq", "FAQ", ExternalExecuting.OpenFAQDialog);
        readonly Settings wiki = new Settings("wiki", "Documentation", ExternalExecuting.OpenWikiDialog);
        //readonly Settings patreon = new Settings("donatePatreon", "Patreon", ExternalExecuting.OpenPatreonDialog);
        readonly Settings paypal = new Settings("donatePaypal", "PayPal", ExternalExecuting.OpenPaypalDialog);
        readonly Settings homepage = new Settings("homepage", "Homepage", ExternalExecuting.OpenHomepageDialog);

        // ACTIVATING OBJECTS
        public static Settings ActiveDistance = new Settings("activeDistance", "Active Distance", 1, MopSettings.UpdateAll);
        readonly string[] activeDistanceText = { "Close (0.75x)", "Normal (1x)", "Far (2x)", "Very Far (4x)" };
        
        public static Settings ModeSafe = new Settings("modeSafe", "Safe Mode", false, MopSettings.UpdateAll);
        public static Settings ModePerformance = new Settings("modePerformance", "Performance", false, MopSettings.UpdateAll);
        public static Settings ModeBalanced = new Settings("modeBalanced", "Balanced", true, MopSettings.UpdateAll);
        public static Settings ModeQuality = new Settings("modeQuality", "Quality", false, MopSettings.UpdateAll);

        // GRAPHICS
        public static Settings EnableFramerateLimiter = new Settings("enableFramerateLimiter", "Enable Framerate Limiter", false, MopSettings.UpdateAll);
        public static Settings FramerateLimiter = new Settings("framerateLimiterNew", "Limit Framerate", 4, MopSettings.UpdateAll);
        public static readonly string[] FramerateLimiterText = { "20", "30", "40", "50", "60", "70", "80", "90", "100", "110", "120", "130", "140", "150", "160", "180", "190", "200" };
        public static Settings EnableShadowAdjusting = new Settings("enableShadowAdjusting", "Adjust Shadows", false, MopSettings.UpdateAll);
        public static Settings ShadowDistance = new Settings("shadowDistance", "Shadow Distance", 200, MopSettings.UpdateAll);
        public static Settings KeepRunningInBackground = new Settings("keepRunningInBackground", "Run Game in Background", true, MopSettings.ToggleBackgroundRunning);
        public static Settings DynamicDrawDistance = new Settings("dynamicDrawDistance", "Dynamic Draw Distance", false, MopSettings.UpdateAll);

        // MOD RULES
        public static Settings RulesAutoUpdate = new Settings("rulesAutoUpdate", "Rules Auto Update", true, MopSettings.UpdateAll);
        public static Settings RulesAutoUpdateFrequency = new Settings("rulesAutoUpdateFrequency", "Auto Update Frequency", 2);
        readonly string[] rulesAutoUpdateFrequencyText = { "On Restart", "Daily", "Every 2 days", "Weekly" };
        readonly Settings forceRuleUpdate = new Settings("forceRuleUpdate", "Force Update", ForceRuleFilesUpdate);
        readonly Settings rulesLearnMore = new Settings("rulesLearnMore", "Learn More", ExternalExecuting.OpenRulesWebsiteDialog);
        public static Settings DeleteUnusedRuleFiles = new Settings("deleteUnusedRuleFiles", "Delete unused rule files", false, MopSettings.UpdateAll);
        public static Settings VerifyRuleFiles = new Settings("verifyRuleFiles", "Verify Rule Files", true, MopSettings.ToggleVerifyRuleFiles);

        // OTHERS
        public static Settings RemoveEmptyBeerBottles = new Settings("removeEmptyBeerBottles", "Destroy empty bottles", false, MopSettings.UpdateAll);
        public static Settings RemoveEmptyItems = new Settings("removeEmptyItems", "Disable empty items", false, MopSettings.UpdateAll);

        // LOGGING
        readonly Settings openOutputLog = new Settings("openOutputLog", "Open output_log.txt", ExceptionManager.OpenOutputLog);
        readonly Settings openLastLog = new Settings("openLastLog", "Open session log folder", ExceptionManager.OpenCurrentSessionLogFolder);
        readonly Settings generateReport = new Settings("generateReport", "Generate mod report", ExceptionManager.GenerateReport);
        readonly Settings deleteAllLogs = new Settings("deleteAllLogs", "Delete all logs", ExceptionManager.DeleteAllLogs);

        readonly Color32 headerColor = new Color32(29, 29, 29, 255);

        public static Guid SessionID;

        /// <summary>
        /// All settings should be created here.
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
            modVersion = Version;
            SessionID = Guid.NewGuid();
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
            Settings.AddButton(this, homepage);
            //Settings.AddButton(this, patreon, new Color32(249, 104, 84, 255), new Color32(5, 45, 73, 255), new Color32(249, 104, 84, 255));
            Settings.AddButton(this, paypal, new Color32(37, 59, 128, 255), new Color32(41, 151, 216, 255), new Color32(34, 45, 101, 255));

            // Activating Objects
            Settings.AddHeader(this, "Activating Objects", headerColor);
            Settings.AddSlider(this, ActiveDistance, 0, 3, activeDistanceText);
            Settings.AddText(this, "Mode:");
            Settings.AddCheckBox(this, ModePerformance, "modes");
            Settings.AddCheckBox(this, ModeBalanced, "modes");
            Settings.AddCheckBox(this, ModeQuality, "modes");
            Settings.AddCheckBox(this, ModeSafe, "modes");

            // Graphics
            Settings.AddHeader(this, "Graphics", headerColor);
            Settings.AddCheckBox(this, EnableFramerateLimiter);
            Settings.AddSlider(this, FramerateLimiter, 0, FramerateLimiterText.Length - 1, FramerateLimiterText);
            Settings.AddCheckBox(this, EnableShadowAdjusting);
            Settings.AddSlider(this, ShadowDistance, 0, 4000);
            Settings.AddCheckBox(this, KeepRunningInBackground);
            Settings.AddCheckBox(this, DynamicDrawDistance);

            // Mod Rules
            Settings.AddHeader(this, "Mod Rules", headerColor);
            Settings.AddButton(this, rulesLearnMore);
            Settings.AddCheckBox(this, RulesAutoUpdate);
            Settings.AddCheckBox(this, VerifyRuleFiles);
            Settings.AddSlider(this, RulesAutoUpdateFrequency, 0, 3, rulesAutoUpdateFrequencyText);
            Settings.AddCheckBox(this, DeleteUnusedRuleFiles);
            Settings.AddButton(this, forceRuleUpdate, "This will force MOP to re-download all mod rule files.");

            // Others
            Settings.AddHeader(this, "Other", headerColor);
            Settings.AddCheckBox(this, RemoveEmptyBeerBottles);
            Settings.AddCheckBox(this, RemoveEmptyItems);

            // Logging
            Settings.AddHeader(this, "Logging", headerColor);
            Settings.AddHeader(this, "WARNING", new Color(0, 0, 0), Color.yellow);
            Settings.AddText(this, "If you're about to send the mod report, please attach BOTH output_log and ALL MOP logs from your session.");
            Settings.AddButton(this, openOutputLog);
            Settings.AddButton(this, openLastLog);
            Settings.AddButton(this, generateReport);
            Settings.AddButton(this, deleteAllLogs, new Color32(218, 39, 37, 255), new Color32(255, 62, 55, 255), new Color32(174, 33, 28, 255));

            // Changelog
            Settings.AddHeader(this, "Changelog", headerColor);
            Settings.AddText(this, GetChangelog());

            // Info
            Settings.AddHeader(this, "Information", headerColor);
            Settings.AddText(this, $"<color=yellow>MOP</color> {ModVersion}\n" +
                $"<color=yellow>ModLoader</color> {ModLoader.MSCLoader_Ver}\n" +
                $"{ExceptionManager.GetSystemInfo()}\n" +
                $"<color=yellow>Session ID:</color> {SessionID}\n" +
                $"\nCopyright © Konrad Figura 2019-{DateTime.Now.Year}");
        }
        #endregion

        public override void OnMenuLoad()
        {
            if (ModLoader.IsModPresent("CheatBox"))
            {
                ModConsole.Warning("[MOP] CheatBox is not supported by MOP! See FAQ for more info.");
            }

            modConfigPath = ModLoader.GetModConfigFolder(this).Replace('\\', '/');
            if (!MopSettings.HasFirstTimeWindowBeenShown())
            {
                ModUI.ShowMessage($"Welcome to Modern Optimization Plugin <color=yellow>{Version}</color>!\n\n" +
                    $"Consider supporting to the project using <color=blue>PayPal</color>!", "MOP");
                MopSettings.FirstTimeWindowShown();
            }

            FsmManager.ResetAll();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public override void ModSettingsLoaded()
        {
            if (modConfigPath == null)
                modConfigPath = ModLoader.GetModConfigFolder(this).Replace('\\', '/');
            MopSettings.UpdateAll();
            modVersion = Version;
            ModConsole.Print($"<color=green>MOP {ModVersion} initialized!</color>");
            new RulesManager();
            ConsoleCommand.Add(new ConsoleCommands());

            SaveManager.RestoreSaveInMainMenu();
        }

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void SecondPassOnLoad()
        {
            MopSettings.UpdateAll();

            // Create WorldManager game object
            GameObject worldManager = new GameObject("MOP");

            // Initialize CompatibiliyManager
            new CompatibilityManager();

            // Add WorldManager class
            worldManager.AddComponent<Hypervisor>();
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

            new RulesManager(true);
        }

        /// <summary>
        /// Gets changelog from changelog.txt and adds rich text elements.
        /// </summary>
        /// <returns></returns>
        string GetChangelog()
        {
            if (string.IsNullOrEmpty(Properties.Resources.changelog))
            {
                return "Dog ate my changelog :(";
            }

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

                if (line.Contains("(My Summer Car)"))
                {
                    line = line.Replace("(My Summer Car)", "<color=green>My Summer Car: </color>");
                }

                if (line.Contains("Rule Files API:"))
                {
                    line = line.Replace("Rule Files API:", "<color=cyan>Rule Files API:</color>");
                }

                output += line + "\n";
            }

            return output;
        }
    }
}
