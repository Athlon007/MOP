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
using System.Collections.Generic;

namespace MOP
{
    public class MOP : Mod
    {
        public override string ID => "MOP";
#if DEBUG
        public override string Name => "Modern Optimization Plugin (Debug)";
#else
        public override string Name => "MODERN OPTIMIZATION PLUGIN"; 
#endif
        public override string Author => "Athlon"; //Your Username
        public override string Version => "3.1"; //Version
        public const string SubVersion = "NIGHTLY-20210422"; // NIGHTLY-yyyymmdd | BETA_x | RC_
#if PRO
        public override string UpdateLink => "https://github.com/Athlon007/MOP";
        public override byte[] Icon => Properties.Resources.icon;
#endif

        #region Settings & Configuration
        // ModLoader configuration.
#if !PRO
        public override bool SecondPass => true;
        public override bool LoadInMenu => true;
#endif

        // Stores the config path of mod.
        static string modConfigPath;
        public static string ModConfigPath { get => modConfigPath; }

        // Stores version number of the mod.
        static string modVersion;
        public static string ModVersion { get => modVersion + (SubVersion != "" ? "_" + SubVersion : ""); }
        public static string ModVersionShort { get => modVersion; }

        // BUTTONS
#if PRO
        static internal SettingSlider ActiveDistance, FramerateLimiter, ShadowDistance, RulesAutoUpdateFrequency;
        static internal SettingRadioButtons PerformanceModes, Resolution;
        static internal SettingToggle EnableShadowAdjusting, KeepRunningInBackground,
                                      DynamicDrawDistance, RulesAutoUpdate, VerifyRuleFiles, DeleteUnusedRules,
                                      DestroyEmptyBottles, DisableEmptyItems;

        readonly string[] activeDistanceText = { "Close (0.75x)", "Normal (1x)", "Far (2x)", "Very Far (4x)" };
        readonly string[] rulesAutoUpdateFrequencyText = { "On Restart", "Daily", "Every 2 days", "Weekly" };
#else
        readonly Settings iFoundABug = new Settings("iFoundABug", "I found a bug", BugReporter.FileBugReport);
        readonly Settings faq = new Settings("faq", "FAQ", ExternalExecuting.OpenFAQDialog);
        readonly Settings wiki = new Settings("wiki", "Documentation", ExternalExecuting.OpenWikiDialog);
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
        readonly Settings deleteUnusedRules = new Settings("deleteUnusedRules", "Delete unused rules", RulesManager.DeleteUnused);

        // OTHERS
        public static Settings RemoveEmptyBeerBottles = new Settings("removeEmptyBeerBottles", "Destroy empty bottles", false, MopSettings.UpdateAll);
        public static Settings RemoveEmptyItems = new Settings("removeEmptyItems", "Disable empty items", false, MopSettings.UpdateAll);

        // LOGGING
        readonly Settings openOutputLog = new Settings("openOutputLog", "Open output_log.txt", ExceptionManager.OpenOutputLog);
        readonly Settings openLastLog = new Settings("openLastLog", "Open log folder", ExceptionManager.OpenCurrentSessionLogFolder);
        readonly Settings generateReport = new Settings("generateReport", "Generate mod report", ExceptionManager.GenerateReport);
        readonly Settings deleteAllLogs = new Settings("deleteAllLogs", "Delete all logs", ExceptionManager.DeleteAllLogs);

        readonly Color32 headerColor = new Color32(29, 29, 29, 255);
#endif

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
#if PRO
            SettingButton ifoundabug = modSettings.AddButton("iFoundABug", "<color=red>I FOUND A BUG</color>", () => BugReporter.FileBugReport());
            modSettings.AddButton("faq", "FAQ", () => ExternalExecuting.OpenFAQDialog());
            modSettings.AddButton("wiki", "WIKI", () => ExternalExecuting.OpenWikiDialog());
            modSettings.AddButton("homepage", "HOMEPAGE", () => ExternalExecuting.OpenHomepageDialog());
            modSettings.AddButton("paypal", "<color=aqua>PAYPAL</color>", () => ExternalExecuting.OpenPaypalDialog());

            // Activating objects.
            modSettings.AddHeader("DESPAWNING");
            ActiveDistance = modSettings.AddSlider("activateDistance", "ACTIVATE DISTANCE", 1, 0, 3, () => MopSettings.UpdateAll());
            ActiveDistance.gameObject.AddComponent<UITooltip>().toolTipText = "Distance uppon which objects will spawn.";
            ActiveDistance.TextValues = activeDistanceText;
            ActiveDistance.ChangeValueText();
            PerformanceModes = modSettings.AddRadioButtons("performanceModes", "PERFORMANCE MODE", 1, (_) => MopSettings.UpdateAll(), "PERFORMANCE", "BALANCED", "QUALITY", "SAFE");
            PerformanceModes.gameObject.AddComponent<UITooltip>().toolTipText = 
                "<color=yellow>PERFORMANCE</color>: <color=white>Visibly disables and enables objects</color>\n" +
                "<color=yellow>BALANCED (recommended)</color>: <color=white>Maintains balance between PERFORMANCE and QUALITY</color>\n" +
                "<color=yellow>QUALITY</color>: <color=white>Hides obvious on-screen spawning and despawning, at the cost of performance</color>\n" +
                "<color=yellow>SAFE</color>: <color=white>Despawns only minimum number of objects that are known to not cause any issues</color>";

            // Graphics
            modSettings.AddHeader("GRAPHICS");
            FramerateLimiter = modSettings.AddSlider("framerateLimiterUpdated", "FRAMERATE LIMITER", 21, 2, 21, () => MopSettings.UpdateAll());
            FramerateLimiter.ValueSuffix = "0 FPS";
            EnableShadowAdjusting = modSettings.AddToggle("enableShadowAdjusting", "ADJUST SHADOWS", false, () => MopSettings.UpdateAll());
            EnableShadowAdjusting.gameObject.AddComponent<UITooltip>().toolTipText = "Allows you to set the shadow render distance with the slider below.";
            ShadowDistance = modSettings.AddSlider("shadowDistance", "SHADOW DISTANCE", 2, 0, 20, () => MopSettings.UpdateAll());
            ShadowDistance.ValueSuffix = "00 Meters";
            KeepRunningInBackground = modSettings.AddToggle("keepRunningInBackground", "RUN IN BACKGROUND", true, MopSettings.ToggleBackgroundRunning);
            KeepRunningInBackground.gameObject.AddComponent<UITooltip>().toolTipText = "If disabled, game will pause when you ALT+TAB from the game.";
            DynamicDrawDistance = modSettings.AddToggle("dynamicDrawDistance", "DYNAMIC DRAW DISTANCE", false, () => MopSettings.UpdateAll());
            DynamicDrawDistance.gameObject.AddComponent<UITooltip>().toolTipText = "MOP will change the draw distance according to situation\n" +
                                                                                   "(ex. lower render distance while in interior)";
            modSettings.AddButton("changeResolution", "CHANGE RESOLUTION", () => { Resolution.gameObject.SetActive(!Resolution.gameObject.activeSelf); });
            List<string> resolutions = new List<string>();
            int selected = 0;
            int i = 0;
            foreach (var res in Screen.resolutions)
            {
                resolutions.Add(res.width + "x" + res.height);
                if (res.width == Screen.width && res.height == Screen.height)
                {
                    selected = i;
                }
                i++;
            }
            Resolution = modSettings.AddRadioButtons("resolution", "RESOLUTION", selected, () => {
                string s = Resolution.GetButtonLabelText(Resolution.Value);
                int width = int.Parse(s.Split('x')[0]);
                int height = int.Parse(s.Split('x')[1]);
                Screen.SetResolution(width, height, Screen.fullScreen); 
            }, resolutions.ToArray());
            Resolution.gameObject.SetActive(false);

            // Rules
            modSettings.AddHeader("RULES");
            SettingButton learnMore = modSettings.AddButton("rulesLearnMore", "LEARN MORE", () => ExternalExecuting.OpenRulesWebsiteDialog());
            learnMore.gameObject.AddComponent<UITooltip>().toolTipText = "Learn about how rules work.";
            RulesAutoUpdate = modSettings.AddToggle("rulesAutoUpdate", "UPDATE RULES AUTOMATICALLY", true);
            VerifyRuleFiles = modSettings.AddToggle("verifyRuleFiles", "VERIFY RULE FILES", true);
            RulesAutoUpdateFrequency = modSettings.AddSlider("ruleAutoUpdateFrequendy", "AUTO-UPDATE FREQUENCY", 2, 0, 3);
            RulesAutoUpdateFrequency.TextValues = rulesAutoUpdateFrequencyText;
            DeleteUnusedRules = modSettings.AddToggle("deleteUnusedRules", "DELETE UNUSED RULES AUTOMATICALLY", false);
            modSettings.AddButton("deleteUnusedRulesButton", "DELETE UNUSED RULES", RulesManager.DeleteUnused);


            // Other
            modSettings.AddHeader("OTHER");
            DestroyEmptyBottles = modSettings.AddToggle("destroyEmptyBottles", "DESTROY EMPTY BOTTLES", false);
            DisableEmptyItems = modSettings.AddToggle("disableEmptyItems", "DISABLE EMPTY ITEMS", false);

            // Logging
            modSettings.AddHeader("LOGGING");
            modSettings.AddText("If you want to file a bug report, use <color=yellow>I FOUND A BUG</color> button!");
            modSettings.AddButton("openLogFolder", "OPEN LOG FOLDER", "", () => ExceptionManager.OpenCurrentSessionLogFolder());
            modSettings.AddButton("generateModReprt", "GENERATE MOD REPORT", "", () => ExceptionManager.GenerateReport());
            modSettings.AddButton("deleteAllLogs", "DELETE ALL LOGS", "", () => ExceptionManager.DeleteAllLogs());

            // Changelog
            modSettings.AddHeader("CHANGELOG");
            modSettings.AddText(GetChangelog());

            // Info
            modSettings.AddHeader("INFO");
            modSettings.AddText($"<color=yellow>MOP</color> {ModVersion}\n" +
                $"<color=yellow>Mod Loader Pro</color> {ModLoader.Version}\n" +
                $"{ExceptionManager.GetSystemInfo()}\n" +
                $"<color=yellow>Session ID:</color> {SessionID}\n" +
                $"\nCopyright © Konrad Figura 2019-{DateTime.Now.Year}");
#else
            if (ModLoader.CheckIfExperimental())
            {
                Settings.AddHeader(this, "Warning: due to frequent updates in Experimental version of the game, " +
                    "expect more bugs, glitches and new features partially or completly not working.", Color.red, Color.black);
            }

            // Links and utilities
            Settings.AddButton(this, iFoundABug, new Color32(219, 182, 36, 255), new Color32(255, 226, 41, 255), new Color32(167, 139, 27, 255));
            Settings.AddButton(this, faq);
            Settings.AddButton(this, wiki);
            Settings.AddButton(this, homepage);
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
            Settings.AddButton(this, deleteUnusedRules, new Color32(218, 39, 37, 255), new Color32(255, 62, 55, 255), new Color32(174, 33, 28, 255));

            // Others
            Settings.AddHeader(this, "Other", headerColor);
            Settings.AddCheckBox(this, RemoveEmptyBeerBottles);
            Settings.AddCheckBox(this, RemoveEmptyItems);

            // Logging
            Settings.AddHeader(this, "Logging", headerColor);
            Settings.AddHeader(this, "WARNING", new Color(0, 0, 0), Color.yellow);
            Settings.AddText(this, "If you want to file a bug report, use <color=yellow>I FOUND A BUG</color> button!");
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
#endif
        }
        #endregion
#if PRO
        public override void MenuOnLoad()
#else
        public override void OnMenuLoad()
#endif
        {
            if (ModLoader.IsModPresent("CheatBox"))
            {
                ModConsole.Warning("[MOP] CheatBox is not supported by MOP! See FAQ for more info.");
            }

#if PRO
            modConfigPath = ModLoader.GetModSettingsFolder(this, true);
#else
            modConfigPath = ModLoader.GetModConfigFolder(this).Replace('\\', '/');
#endif
            if (!MopSettings.HasFirstTimeWindowBeenShown())
            {
                ModUI.ShowMessage($"Welcome to Modern Optimization Plugin <color=yellow>{Version}</color>!\n\n" +
                    $"Consider supporting to the project using <color=#3687D7>PayPal</color>,\n" +
                    $"or with <color=#ADAD46>Bitcoins</color>.", "MOP");
                MopSettings.FirstTimeWindowShown();
            }

#if !PRO
            if (!MopSettings.HasMLPWarningBeenShown() && !MopSettings.IsModLoaderPro())
            {
                ModUI.ShowYesNoMessage("MOP 3.3 will drop support for MSCLoader, and will require <color=yellow>MSC Mod Loader Pro</color>.\n\nWould you like to download it now?", "MOP", ExternalExecuting.ModLoaderPro);
                MopSettings.MLPWarningShown();
            }

            if (MopSettings.IsModLoaderPro())
            {
                ModUI.ShowYesNoMessage("You are using MOP version for MSCLoader, while Mod Loader Pro version is available.\n\nWould you like to download MLP version now?", ExternalExecuting.DownloadMOPPro);
            }
#endif

            FsmManager.ResetAll();
            Resources.UnloadUnusedAssets();
            GC.Collect();

            GameObject bugReporter = new GameObject("MOP_BugReporter");
            bugReporter.AddComponent<BugReporter>();
        }

        public override void ModSettingsLoaded()
        {
            if (modConfigPath == null)
            {
#if PRO
                modConfigPath = ModLoader.GetModSettingsFolder(this, true);
#else
                modConfigPath = ModLoader.GetModConfigFolder(this).Replace('\\', '/');
#endif
            }
            MopSettings.UpdateAll();
            modVersion = Version;
            ModConsole.Print($"<color=green>MOP {ModVersion} initialized!</color>");
            new RulesManager();
            ConsoleCommand.Add(new ConsoleCommands());

            SaveManager.RestoreSaveInMainMenu();

#if PRO
            if (FramerateLimiter.Value == 21)
            {
                FramerateLimiter.valueText.text = "Disabled";
            }
            if (ShadowDistance.Value == 0)
            {
                ShadowDistance.valueText.text = "No Shadows";
            }
#endif
            SaveManager.VerifySave();
        }

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
#if PRO
        public override void PostLoad()
#else
        public override void SecondPassOnLoad()
#endif
        {
            MopSettings.UpdateAll();

            // Create WorldManager game object
            GameObject worldManager = new GameObject("MOP");

            // Initialize CompatibiliyManager
            new CompatibilityManager();

            // Add WorldManager class
            worldManager.AddComponent<Hypervisor>();
        }

#if PRO
        public override void ModSettingsOpen()
        {
            int selected = 0;
            int i = 0;
            foreach (var res in Screen.resolutions)
            {
                if (res.width == Screen.width && res.height == Screen.height)
                {
                    selected = i;
                }
                i++;
            }

            Resolution.Value = selected;
        }

        public override void ModSettingsClose()
        {
            Resolution.gameObject.SetActive(false);
        }
#endif

        static void ForceRuleFilesUpdate()
        {
#if PRO
            if (ModLoader.CurrentScene == CurrentScene.MainMenu)
            {
                ModPrompt.CreatePrompt("You can only force update while in main menu.");
                return;
            }
#else
            if (ModLoader.GetCurrentScene() != CurrentScene.MainMenu)
            {
                ModUI.ShowMessage("You can only force update while in main menu.");
                return;
            }
#endif

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

                if (line.Contains("(Beta)"))
                {
                    line = line.Replace("(Beta)", "<color=orange>Beta: </color>");
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

                if (line.Contains("(Mod Loader Pro)"))
                {
                    line = line.Replace("(Mod Loader Pro)", "<color=yellow>Mod Loader Pro:</color>");
                }

                output += line + "\n";
            }

            return output;
        }
    }
}
