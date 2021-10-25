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
using MSCLoader.Helper;

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
        public override string Version => "3.3.5"; //Version
        public const string SubVersion = ""; // NIGHTLY-yyyymmdd | BETA_x | RC_
        public override string UpdateLink => "https://github.com/Athlon007/MOP";
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
        static internal SettingSlider ActiveDistance, FramerateLimiter, ShadowDistance, RulesAutoUpdateFrequency;
        static internal SettingRadioButtons PerformanceModes, Resolution;
        static internal SettingToggle EnableShadowAdjusting, KeepRunningInBackground,
                                      DynamicDrawDistance, RulesAutoUpdate, VerifyRuleFiles, DeleteUnusedRules,
                                      DestroyEmptyBottles, DisableEmptyItems;

        static SettingString lastVersion;

        readonly string[] activeDistanceText = { "Close (0.75x)", "Normal (1x)", "Far (2x)", "Very Far (4x)" };
        readonly string[] rulesAutoUpdateFrequencyText = { "On Restart", "Daily", "Every 2 days", "Weekly" };

        public static Guid SessionID;

        /// <summary>
        /// All settings should be created here.
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
            lastVersion = modSettings.AddString("lastVersion", "1.0");

            modVersion = Version;
            SessionID = Guid.NewGuid();
#if DEBUG
            modSettings.AddHeader("Shh...Don't leak my hard work ;)", Color.yellow, Color.black);
#endif
            SettingButton ifoundabug = modSettings.AddButton("iFoundABug", "<color=red>I FOUND A BUG</color>", () => BugReporter.FileBugReport());
            modSettings.AddButton("faq", "FAQ", () => ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/faq"));
            modSettings.AddButton("wiki", "WIKI", () => ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/"));
            modSettings.AddButton("homepage", "HOMEPAGE", () => ShowDialog("http://athlon.kkmr.pl/"));
            modSettings.AddButton("homepage", "NEXUSMODS", () => ShowDialog("https://www.nexusmods.com/mysummercar/mods/146"));
            modSettings.AddButton("paypal", "<color=aqua>PAYPAL</color>", () => ShowDialog("https://paypal.me/figurakonrad"));

            // Activating objects.
            modSettings.AddHeader("DESPAWNING");
            ActiveDistance = modSettings.AddSlider("activateDistance", "ACTIVATE DISTANCE", 1, 0, 3);
            ActiveDistance.gameObject.AddComponent<UITooltip>().toolTipText = "Distance uppon which objects will spawn.";
            ActiveDistance.TextValues = activeDistanceText;
            ActiveDistance.ChangeValueText();
            PerformanceModes = modSettings.AddRadioButtons("performanceModes", "PERFORMANCE MODE", 1, MopSettings.UpdatePerformanceMode, "PERFORMANCE", "BALANCED", "QUALITY", "<color=red>SAFE</color>");
            PerformanceModes.gameObject.AddComponent<UITooltip>().toolTipText =
                "<color=yellow>PERFORMANCE</color>: <color=white>Visibly disables and enables objects</color>\n" +
                "<color=yellow>BALANCED (recommended)</color>: <color=white>Maintains balance between PERFORMANCE and QUALITY</color>\n" +
                "<color=yellow>QUALITY</color>: <color=white>Hides obvious on-screen spawning and despawning, at the cost of performance</color>\n" +
                "<color=yellow>SAFE</color>: <color=white>Despawns only minimum number of objects that are known to not cause any issues</color>";

            // Graphics
            modSettings.AddHeader("GRAPHICS");
            FramerateLimiter = modSettings.AddSlider("framerateLimiterUpdated", "FRAMERATE LIMITER", 21, 2, 21, MopSettings.UpdateFramerateLimiter);
            FramerateLimiter.ValueSuffix = "0 FPS";
            EnableShadowAdjusting = modSettings.AddToggle("enableShadowAdjusting", "ADJUST SHADOWS", false, () => { MopSettings.UpdateShadows(); ShadowDistance.gameObject.SetActive(EnableShadowAdjusting.Value); } );
            EnableShadowAdjusting.gameObject.AddComponent<UITooltip>().toolTipText = "Allows you to set the shadow render distance with the slider below.";
            ShadowDistance = modSettings.AddSlider("shadowDistance", "SHADOW DISTANCE", 2, 0, 20, () => MopSettings.UpdateShadows());
            ShadowDistance.ValueSuffix = "00 Meters";
            ShadowDistance.gameObject.SetActive(EnableShadowAdjusting.Value);
            KeepRunningInBackground = modSettings.AddToggle("keepRunningInBackground", "RUN IN BACKGROUND", true, MopSettings.ToggleBackgroundRunning);
            KeepRunningInBackground.gameObject.AddComponent<UITooltip>().toolTipText = "If disabled, game will pause when you ALT+TAB from the game.";
            DynamicDrawDistance = modSettings.AddToggle("dynamicDrawDistance", "DYNAMIC DRAW DISTANCE", false);
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
                ++i;
            }
            Resolution = modSettings.AddRadioButtons("resolution", "RESOLUTION", selected, () =>
            {
                string s = Resolution.GetButtonLabelText(Resolution.Value);
                int width = int.Parse(s.Split('x')[0]);
                int height = int.Parse(s.Split('x')[1]);
                Screen.SetResolution(width, height, Screen.fullScreen);
            }, resolutions.ToArray());
            Resolution.gameObject.SetActive(false);

            // Rules
            modSettings.AddHeader("RULES");
            SettingButton learnMore = modSettings.AddButton("rulesLearnMore", "LEARN MORE", () => ShowDialog("http://athlon.kkmr.pl/mop"));
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
        }
        #endregion
        public override void MenuOnLoad()
        {
            modSettings.LoadSettings();
            modConfigPath = ModLoader.GetModSettingsFolder(this, true);
            if (!Version.StartsWith(lastVersion.Value.ToString()))
            {
                lastVersion.Value = Version;
                modSettings.SaveSettings();
                ModPrompt.CreatePrompt($"Welcome to Modern Optimization Plugin <color=yellow>{Version}</color>!\n\n" +
                    $"Please consider supporting the project using <color=#3687D7>PayPal</color>,\n" +
                    $"or with <color=#ADAD46>Bitcoins</color>.", "MOP");
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
                ModPrompt prompt = ModPrompt.CreateCustomPrompt();
                prompt.Text = "You've reloaded game without fully quitting it over 5 times.\n\n" +
                                       "It is recommended to fully quit the game after a while, so it would fully unload the memory.\n" +
                                       "Not doing that may lead to game breaking glitches.";
                prompt.Title = "MOP";
                prompt.AddButton("OK", null);
                prompt.AddButton("QUIT GAME", () => Application.Quit());
            }

            if (MopSettings.AttemptedToFixTheGame && !MopSettings.AttemptedToFixTheGameRestart)
            {
                MopSettings.AttemptedToFixTheGameRestart = true;
                BugReporter.Instance.RestartGame();
                return;
            }
            MopSettings.AttemptedToFixTheGame = false;
            MopSettings.AttemptedToFixTheGameRestart = false;
        }

        public override void ModSettingsLoaded()
        {
            if (modConfigPath == null)
            {
                modConfigPath = ModLoader.GetModSettingsFolder(this, true);
            }
            MopSettings.UpdateFramerateLimiter();
            MopSettings.UpdatePerformanceMode();
            MopSettings.UpdateShadows();
            MopSettings.UpdateMiscSettings();
            modVersion = Version;
            ModConsole.Log($"<color=green>MOP {ModVersion} initialized!</color>");
            new RulesManager();
            ConsoleCommand.Add(new ConsoleCommands());

            if (FramerateLimiter.Value == 21)
            {
                FramerateLimiter.valueText.text = "Disabled";
            }
            if (ShadowDistance.Value == 0)
            {
                ShadowDistance.valueText.text = "No Shadows";
            }

            if (MopSettings.IsConfilctingModPresent(out string modName))
            {
                ModPrompt.CreatePrompt($"MOP does not work with <color=yellow>{modName}</color>. Please disable that mod first.", "MOP");
            }
            SaveManager.VerifySave();
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
            if (MopSettings.IsConfilctingModPresent(out string modName))
            {
                ModConsole.LogError("MOP could not be loaded, because the following mod is present: " + modName);
                return;
            }

            // Create WorldManager game object
            GameObject worldManager = new GameObject("MOP");

            // Initialize CompatibiliyManager
            new CompatibilityManager();

            // Add WorldManager class
            worldManager.AddComponent<Hypervisor>();

            SaveManager.AddSaveFlag();
        }

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

        static void ForceRuleFilesUpdate()
        {
            if (ModLoader.CurrentScene == CurrentScene.MainMenu)
            {
                ModPrompt.CreatePrompt("You can only force update while in main menu.");
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

        public static void ShowDialog(string url) => ModPrompt.CreateYesNoPrompt($"This will open the following link:\n" +
                                                                         $"<color=yellow>{url}</color>\n\n" +
                                                                         $"Are you sure you want to continue?",
                                                                         "MOP",
                                                                         () => ModHelper.OpenWebsite(url));
    }
}
