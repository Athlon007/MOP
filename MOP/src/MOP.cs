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

using System;
using System.IO;
using UnityEngine;
using MSCLoader;

using MOP.FSM;
using MOP.Common;
using MOP.Helpers;
using MOP.Rules;

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
        public override string Version => "3.4"; //Version
        public const string SubVersion = "NIGHTLY-20211226"; // NIGHTLY-yyyymmdd | BETA_x | RC_
        public override byte[] Icon => Properties.Resources.icon;

        #region Settings & Configuration
        // Stores the config path of mod.
        static string modConfigPath;
        public static string ModConfigPath { get => modConfigPath; }

        // Stores version number of the mod.
        static string modVersion;
        public static string ModVersion { get => modVersion + (SubVersion != "" ? "_" + SubVersion : ""); }
        public static string ModVersionShort { get => modVersion; }

        // Settings
        static internal SettingsSliderInt ActiveDistance, FramerateLimiter, ShadowDistance, RulesAutoUpdateFrequency;
        static internal SettingsCheckBoxGroup ModePerformance, ModeBalanced, ModeQuality, ModeSafe;
        static internal SettingsCheckBox  KeepRunningInBackground, LimitFramerate, DynamicDrawDistance, 
                                          RulesAutoUpdate, VerifyRuleFiles, DeleteUnusedRules,
                                          DestroyEmptyBottles, DisableEmptyItems;

        readonly string[] activeDistanceText = { "Close (0.75x)", "Normal (1x)", "Far (2x)", "Very Far (4x)" }; 
        readonly string[] rulesAutoUpdateFrequencyText = { "On Restart", "Daily", "Every 2 days", "Weekly" };

        public static Guid SessionID;

        const string WelcomeMessage = "Welcome to Modern Optimization Plugin <color=yellow>{0}</color>!\n\n" +
                                      "Please consider supporting the project using <color=#3687D7>PayPal</color>.";
        const string WelcomeMessageFestive = "Merry Christmas and Happy New Year {1}!\n\n" +
                                             "Welcome to Modern Optimization Plugin <color=yellow>{0}</color>!\n" +
                                             "Please consider supporting the project using <color=#3687D7>PayPal</color>.";

        /// <summary>
        /// All settings should be created here.
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
            modVersion = Version;
            modConfigPath = ModLoader.GetModSettingsFolder(this);
            SessionID = Guid.NewGuid();
#if DEBUG
            Settings.AddHeader(this, "Shh...Don't leak my hard work ;)", Color.yellow, Color.black);
#endif
            Settings.AddButton(this, "iFoundABug", "<color=red>I FOUND A BUG</color>", BugReporter.FileBugReport);
            Settings.AddButton(this, "faq", "FAQ", () => ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/faq"));
            Settings.AddButton(this, "wiki", "WIKI", () => ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/"));
            Settings.AddButton(this, "homepage", "HOMEPAGE", () => ShowDialog("http://athlon.kkmr.pl/"));
            Settings.AddButton(this, "github", "GITHUB", () => ShowDialog("https://github.com/Athlon007/MOP"));
            Settings.AddButton(this, "homepage", "NEXUSMODS", () => ShowDialog("https://www.nexusmods.com/mysummercar/mods/146"));
            Settings.AddButton(this, "paypal", "<color=aqua>PAYPAL</color>", () => ShowDialog("https://paypal.me/figurakonrad"));

            // Activating objects.
            Settings.AddHeader(this, "DESPAWNING");
            ActiveDistance = Settings.AddSlider(this, "activateDistance", "ACTIVATE DISTANCE", 0, 3, 1, textValues: activeDistanceText);
            ModePerformance = Settings.AddCheckBoxGroup(this, "modePerformance", "PERFORMANCE", false, "performanceMode", MopSettings.UpdatePerformanceMode);
            ModeBalanced = Settings.AddCheckBoxGroup(this, "modeBalanced", "BALANCED", true, "performanceMode", MopSettings.UpdatePerformanceMode);
            ModeQuality = Settings.AddCheckBoxGroup(this, "modeQuality", "QUALITY", false, "performanceMode", MopSettings.UpdatePerformanceMode);
            ModeSafe = Settings.AddCheckBoxGroup(this, "modeSafe", "<color=red>SAFE</color>", false, "performanceMode", MopSettings.UpdatePerformanceMode);

            // Graphics
            Settings.AddHeader(this, "GRAPHICS");
            LimitFramerate = Settings.AddCheckBox(this, "limitFramerate", "LIMIT FRAMERATE", false, MopSettings.UpdateFramerateLimiter);
            FramerateLimiter = Settings.AddSlider(this, "framerateLimiterUpdated", "FRAMERATE LIMITER (FPS)", 20, 200, 60, MopSettings.UpdateFramerateLimiter);
            ShadowDistance = Settings.AddSlider(this, "shadowDistance", "SHADOW DISTANCE (METERS, 200 DEFAULT)", 0, 2000, 200, MopSettings.UpdateShadows);
            KeepRunningInBackground = Settings.AddCheckBox(this, "keepRunningInBackground", "RUN IN BACKGROUND", true, MopSettings.ToggleBackgroundRunning);
            DynamicDrawDistance = Settings.AddCheckBox(this, "dynamicDrawDistance", "DYNAMIC DRAW DISTANCE", false);

            // Rules
            Settings.AddHeader(this, "RULES");
            Settings.AddButton(this, "rulesLearnMore", "LEARN MORE", () => ShowDialog("http://athlon.kkmr.pl/mop"));
            RulesAutoUpdate = Settings.AddCheckBox(this, "rulesAutoUpdate", "UPDATE RULES AUTOMATICALLY", true);
            VerifyRuleFiles = Settings.AddCheckBox(this, "verifyRuleFiles", "VERIFY RULE FILES", true);
            RulesAutoUpdateFrequency = Settings.AddSlider(this, "ruleAutoUpdateFrequendy", "AUTO-UPDATE FREQUENCY", 0, 3, 2, textValues: rulesAutoUpdateFrequencyText);
            DeleteUnusedRules = Settings.AddCheckBox(this, "deleteUnusedRules", "AUTOMATICALLY DELETE UNUSED RULES", false);
            Settings.AddButton(this, "deleteUnusedRulesButton", "DELETE UNUSED RULES", RulesManager.DeleteUnused);
            Settings.AddButton(this, "forceRulesUpdate", "FORCE UPDATE", ForceRuleFilesUpdate);

            // Other
            Settings.AddHeader(this, "OTHER");
            DestroyEmptyBottles = Settings.AddCheckBox(this, "destroyEmptyBottles", "DESTROY EMPTY BOTTLES", false);
            DisableEmptyItems = Settings.AddCheckBox(this, "disableEmptyItems", "DISABLE EMPTY ITEMS", false);

            // Logging
            Settings.AddHeader(this, "LOGGING");
            Settings.AddText(this, "If you want to file a bug report, use <color=yellow>I FOUND A BUG</color> button!");
            Settings.AddButton(this, "openLogFolder", "OPEN LOG FOLDER", ExceptionManager.OpenCurrentSessionLogFolder);
            Settings.AddButton(this, "generateModReprt", "GENERATE MOD REPORT", ExceptionManager.GenerateReport);
            Settings.AddButton(this, "deleteAllLogs", "DELETE ALL LOGS", ExceptionManager.DeleteAllLogs);

            // Changelog
            Settings.AddHeader(this, "CHANGELOG");
            Settings.AddText(this, GetChangelog());

            // Info
            Settings.AddHeader(this, "INFO");
            Settings.AddText(this, $"<color=yellow>MOP</color> {ModVersion}\n" +
                $"<color=yellow>MSCLoader</color> {ModLoader.MSCLoader_Ver}\n" +
                $"{ExceptionManager.GetSystemInfo()}\n" +
                $"<color=yellow>Session ID:</color> {SessionID}\n" +
                $"\nCopyright © Konrad Figura 2019-{DateTime.Now.Year}");
        }
        #endregion

        public override void MenuOnLoad()
        {
            RemoveUnusedFiles();

            if (!Version.StartsWith(MopSettings.Data.Version.ToString()))
            {
                MopSettings.Data.Version = Version;
                MopSettings.WriteData(MopSettings.Data);
                string message = DateTime.Now.Month == 12 && DateTime.Now.Day >= 20 ? WelcomeMessageFestive : WelcomeMessage;
                message = string.Format(message, Version, DateTime.Now.Year + 1);
                ModUI.ShowMessage(message, "MOP");
            }

            FsmManager.ResetAll();
            Resources.UnloadUnusedAssets();
            GC.Collect();

            GameObject bugReporter = new GameObject("MOP_BugReporter");
            bugReporter.AddComponent<BugReporter>();

            MopSettings.Restarts++;
            if (MopSettings.Restarts > MopSettings.MaxRestarts && !MopSettings.RestartWarningShown)
            {
                MopSettings.RestartWarningShown = true;
                ModUI.ShowYesNoMessage("You've reloaded the game without fully quitting it over 5 times.\n\n" +
                                       "It is recommended to fully quit the game after a while, so it would fully unload the memory.\n" +
                                       "Not doing so may lead to game-breaking glitches.\n\n" +
                                       "Would you like to do that now?", "MOP", Application.Quit);
            }

            if (MopSettings.GameFixStatus == Common.Enumerations.GameFixStatus.DoFix)
            {
                MopSettings.GameFixStatus = Common.Enumerations.GameFixStatus.Restarted;
                BugReporter.Instance.RestartGame();
                return;
            }

            MopSettings.GameFixStatus = Common.Enumerations.GameFixStatus.None;
        }

        public override void ModSettingsLoaded()
        {
            if (modConfigPath == null)
            {
                modConfigPath = ModLoader.GetModSettingsFolder(this);
            }
            MopSettings.UpdateFramerateLimiter();
            MopSettings.UpdatePerformanceMode();
            MopSettings.UpdateShadows();
            MopSettings.UpdateMiscSettings();

            new RulesManager();
            ConsoleCommand.Add(new ConsoleCommands());

            if (CompatibilityManager.IsConfilctingModPresent(out string modName))
            {
                ModUI.ShowMessage($"MOP does not work with <color=yellow>{modName}</color>. Please disable that mod first.", "MOP");
            }
            SaveManager.VerifySave();

            ModConsole.Log($"<color=green>MOP {ModVersion} initialized!</color>");
        }

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void PostLoad()
        {
            MopSettings.UpdateFramerateLimiter();
            MopSettings.UpdatePerformanceMode();
            MopSettings.UpdateShadows();
            MopSettings.UpdateMiscSettings();
            if (CompatibilityManager.IsConfilctingModPresent(out string modName))
            {
                ModConsole.LogError("MOP could not be loaded, because the following mod is present: " + modName);
                return;
            }

            // Create MOP game object
            GameObject mop = new GameObject("MOP");

            // Initialize CompatibiliyManager
            new CompatibilityManager();

            // Add Hypervisor class
            mop.AddComponent<Hypervisor>();

            SaveManager.AddSaveFlag();
        }

        static void ForceRuleFilesUpdate()
        {
            if (ModLoader.CurrentScene != CurrentScene.MainMenu)
            {
                ModUI.ShowMessage("You can only force update while in the main menu.");
                return;
            }

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

                if (line.Contains("(MSCLoader)"))
                {
                    line = line.Replace("(MSCLoader)", "<color=yellow>MSCLoader:</color>");
                }

                output += line + "\n";
            }

            return output;
        }

        public static void ShowDialog(string url) => ModUI.ShowYesNoMessage($"This will open the following link:\n" +
                                                                         $"<color=yellow>{url}</color>\n\n" +
                                                                         $"Are you sure you want to continue?",
                                                                         "MOP",
                                                                         () => System.Diagnostics.Process.Start(url));

        void RemoveUnusedFiles()
        {
            RemoveIfExists(Path.Combine(ModConfigPath, "LastModList.mop"));
            RemoveIfExists(Path.Combine(ModConfigPath, "LastUpdate.mop"));
            RemoveIfExists(Path.Combine(ModConfigPath, "RulesInfo.json"));
        }

        void RemoveIfExists(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }
}
