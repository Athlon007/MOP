﻿// Modern Optimization Plugin
// Copyright(C) 2019-2022 Athlon

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
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MSCLoader;

using MOP.Common;
using MOP.Common.Enumerations;
using MOP.FSM;
using MOP.Helpers;
using MOP.Rules;
using System.Text;
using System.Diagnostics;

namespace MOP
{
    public class MOP : Mod
    {
        public override string ID => "MOP";
        public override string Name => "MODERN OPTIMIZATION PLUGIN";
        public override string Author => "Athlon"; //Your Username
        public override string Version => "3.10.1"; //Version
        public const string SubVersion = ""; // NIGHTLY-yyyymmdd | BETA_x | RC_
#if PRO
        public const string Edition = "Mod Loader Pro";
#else
        public const string Edition = "MSCLoader";
        public override string Description => "The <color=yellow>ultimate</color> My Summer Car optimization project!";
        private bool menuLoaded;
#endif
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
#if PRO
        public override string UpdateLink => "https://github.com/Athlon007/MOP";

        static internal SettingSlider ActiveDistance, FramerateLimiter, ShadowDistance, RulesAutoUpdateFrequency;
        static internal SettingRadioButtons PerformanceModes, Resolution;
        static internal SettingToggle EnableShadowAdjusting, KeepRunningInBackground,
                                      DynamicDrawDistance, RulesAutoUpdate, VerifyRuleFiles, DeleteUnusedRules,
                                      DestroyEmptyBottles, DisableEmptyItems,
                                      AlwaysDisableSkidmarks;
        SettingText modeWarningText;
#else
        static internal SettingsSliderInt ActiveDistance, FramerateLimiter, RulesAutoUpdateFrequency, ShadowDistance;
        static internal SettingsCheckBoxGroup ModePerformance, ModeBalanced, ModeQuality, ModeSafe;
        static internal SettingsCheckBox KeepRunningInBackground, LimitFramerate, DynamicDrawDistance,
                                          RulesAutoUpdate, VerifyRuleFiles, DeleteUnusedRules,
                                          DestroyEmptyBottles, DisableEmptyItems,
                                          AlwaysDisableSkidmarks;
        static internal SettingsDropDownList GameResolution;
        SettingsDynamicText modeWarningText;
#endif

        // Constant text.
        const string WarningMode = "Some changes will be applied after the game restart.";
        readonly string[] activeDistanceText = { "Very Close (0.5x)", "Close (0.75x)", "Default (1x)", "Far (2x)", "Very Far (4x)" };
        readonly string[] rulesAutoUpdateFrequencyText = { "Every launch", "Daily", "Every 2 days", "Weekly" };

        const string WelcomeMessage = "Welcome to Modern Optimization Plugin {0}!\n" +
                                      "Consider supporting the project using PayPal, or NexusMods.\n\n" +
                                      "<b>BACKUP YOUR SAVE FILES!</b>";
        const string WelcomeMessageFestive = "Merry Christmas and Happy New Year {1}!\n\n" +
                                             "Welcome to Modern Optimization Plugin{0}!\n" +
                                             "Consider supporting the project using PayPal, or on NexusMods.";

        private const string FaqLink = "http://kfigura.nl/mop/wiki/#/faq";
        private const string DonateLink = "https://www.paypal.com/donate/?hosted_button_id=8VASR9RLLS76Y";
        private const string KofiLink = "https://ko-fi.com/athlon";
        private const string WikiLink = "http://kfigura.nl/mop/wiki/#/";
        private const string HomepageLink = "http://kfigura.nl/";
        private const string GitHubLink = "https://github.com/Athlon007/MOP";
        private const string NexusModsLink = "https://www.nexusmods.com/mysummercar/mods/146";
        private const string RuleFilesWikiLink = "http://kfigura.nl/mop/wiki/#/rulefiles";

        private static GameObject MopLoadScreenPrefab { get; set; }
#if PRO
        public MOP()
        {
            if (CompatibilityManager.IsMSCLoader())
            {
                ModUI.ShowMessage("You are trying to use MOP version for <color=yellow>Mod Loader Pro</color>.\n\n" +
                                  "Please install MOP version for <color=yellow>MSCLoader</color>!", "MOP - Error");
                return;
            }
        }
#endif

        public override void ModSettings()
        {
            // Get resoultions.
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

            // Set modVersion and other.
            modVersion = Version;
            modConfigPath = ModLoader.GetModSettingsFolder(this);

#if PRO
            modSettings.AddButton("iFoundABug", "<color=red>I FOUND A BUG</color>", BugReporter.FileBugReport);
            modSettings.AddButton("faq", "FAQ", () => ShowDialog(FaqLink));
            modSettings.AddButton("wiki", "WIKI", () => ShowDialog(WikiLink));
            modSettings.AddButton("homepage", "HOMEPAGE", () => ShowDialog(HomepageLink));
            modSettings.AddButton("github", "GITHUB", () => ShowDialog(GitHubLink));
            modSettings.AddButton("homepage", "NEXUSMODS", () => ShowDialog(NexusModsLink));
            modSettings.AddButton("paypal", "<color=#254280>DONATE</color>", OnDonateButtonClick);

            // Activating objects.
            modSettings.AddHeader("DESPAWNING");
            ActiveDistance = modSettings.AddSlider("activateDistanceNew", "ACTIVATE DISTANCE", 2, 0, 4);
            ActiveDistance.AddTooltip("Distance upon which objects will spawn.");
            ActiveDistance.TextValues = activeDistanceText;
            ActiveDistance.ChangeValueText();
            PerformanceModes = modSettings.AddRadioButtons("performanceModes", "PERFORMANCE MODE", 1,
                                                          () => { MopSettings.UpdatePerformanceMode(); UpdateSettingsUI(); },
                                                          "PERFORMANCE", "BALANCED", "QUALITY", "<color=red>SAFE</color>");
            PerformanceModes.AddTooltip(
                "<color=yellow>PERFORMANCE</color>: <color=white>Visibly disables and enables objects</color>\n" +
                "<color=yellow>BALANCED (recommended)</color>: <color=white>Maintains balance between PERFORMANCE and QUALITY</color>\n" +
                "<color=yellow>QUALITY</color>: <color=white>Hides obvious on-screen spawning and despawning, at the cost of performance</color>\n" +
                "<color=yellow>SAFE</color>: <color=white>Despawns only minimum number of objects that are known to not cause any issues</color>");
            modeWarningText = modSettings.AddText("");

            // Graphics
            modSettings.AddHeader("GRAPHICS");
            FramerateLimiter = modSettings.AddSlider("framerateLimiterUpdated", "FRAMERATE LIMITER", 21, 2, 21, () => { MopSettings.UpdateFramerateLimiter(); UpdateSettingsUI(); });
            FramerateLimiter.ValueSuffix = "0 FPS";
            EnableShadowAdjusting = modSettings.AddToggle("enableShadowAdjusting", "ADJUST SHADOWS", false, () => { MopSettings.UpdateShadows(); ShadowDistance.gameObject.SetActive(EnableShadowAdjusting.Value); });
            EnableShadowAdjusting.AddTooltip("Allows you to set the shadow render\ndistance with the slider below.");
            ShadowDistance = modSettings.AddSlider("shadowDistance", "SHADOW DISTANCE", 2, 0, 20, () => { MopSettings.UpdateShadows(); UpdateSettingsUI(); });
            ShadowDistance.ValueSuffix = "00 Meters";
            ShadowDistance.gameObject.SetActive(EnableShadowAdjusting.Value);
            KeepRunningInBackground = modSettings.AddToggle("keepRunningInBackground", "RUN GAME IN BACKGROUND", true, MopSettings.ToggleBackgroundRunning);
            KeepRunningInBackground.AddTooltip("If unchecked, the game will\nbe paused when the game's\nwindow looses focus.");
            DynamicDrawDistance = modSettings.AddToggle("dynamicDrawDistance", "DYNAMIC DRAW DISTANCE", true);
            DynamicDrawDistance.AddTooltip("MOP will adjust the draw distance\naccording to the current situation\n(ex. lower it while inside of a building).");
            modSettings.AddButton("changeResolution", "RESOLUTION", () => { Resolution.gameObject.SetActive(!Resolution.gameObject.activeSelf); });

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
            SettingButton learnMore = modSettings.AddButton("rulesLearnMore", "LEARN MORE", () => ShowDialog(RuleFilesWikiLink));
            learnMore.AddTooltip("Learn about how rules work.");
            RulesAutoUpdate = modSettings.AddToggle("rulesAutoUpdate", "UPDATE RULES AUTOMATICALLY", true, WarningDisableAutoUpdate);
            RulesAutoUpdateFrequency = modSettings.AddSlider("ruleAutoUpdateFrequendy", "AUTO-UPDATE FREQUENCY", 2, 0, 3);
            RulesAutoUpdateFrequency.TextValues = rulesAutoUpdateFrequencyText;
            VerifyRuleFiles = modSettings.AddToggle("verifyRuleFiles", "VERIFY RULE FILES", true);
            DeleteUnusedRules = modSettings.AddToggle("deleteUnusedRules", "AUTOMATICALLY DELETE UNUSED RULES", false);
            modSettings.AddButton("deleteUnusedRulesButton", "DELETE UNUSED RULES", RulesManager.DeleteUnused);
            modSettings.AddButton("forceRulesUpdate", "FORCE UPDATE", ForceRuleFilesUpdate);

            // Other
            modSettings.AddHeader("OTHER");
            DestroyEmptyBottles = modSettings.AddToggle("destroyEmptyBottles", "DESTROY EMPTY BOTTLES", false);
            DestroyEmptyBottles.AddTooltip("Empty bottles, created by player drinking, won't spawn.");
            DisableEmptyItems = modSettings.AddToggle("disableEmptyItems", "DISABLE EMPTY ITEMS", false);
            DisableEmptyItems.AddTooltip("Items, which status changes to 'EMPTY',\nwill be automatically disabled,\nin order to save on PC resources.");
            AlwaysDisableSkidmarks = modSettings.AddToggle("alwaysDisableSkidmarks", "DISABLE SKIDMARKS", false);
            AlwaysDisableSkidmarks.AddTooltip("Skidmarks cause a massive memory leak,\nevery time you brake/accelerate hard.\nDisabling them mitigates that problem.");

            // Logging
            modSettings.AddHeader("LOGGING");
            modSettings.AddText("If you want to file a bug report, use <color=red>I FOUND A BUG</color> button!");
            modSettings.AddButton("openLogFolder", "OPEN LOGS FOLDER", "", ExceptionManager.OpenCurrentSessionLogFolder);
            modSettings.AddButton("generateModReprt", "GENERATE MOD REPORT", "", ExceptionManager.GenerateReport);
            modSettings.AddButton("deleteAllLogs", "DELETE ALL LOGS", "", ExceptionManager.DeleteAllLogs);

            // Supporters
            modSettings.AddHeader("SUPPORTERS");
            modSettings.AddText("<color=yellow>" + GetSupporters() + "</color>" +
                "\n\nDo you want to see your name here? Send a donate!");

            // Changelog
            modSettings.AddHeader("CHANGELOG");
            modSettings.AddText(GetChangelog(false));

            // Info
            modSettings.AddHeader("INFO");
            modSettings.AddText(GetFooter());

            UpdateSettingsUI();
#else
            Settings.AddButton(this, "iFoundABug", "<color=red>I FOUND A BUG</color>", BugReporter.FileBugReport);
            Settings.AddButton(this, "linkFAQ", "FAQ", () => ShowDialog(FaqLink));
            Settings.AddButton(this, "linkWiki", "WIKI", () => ShowDialog(WikiLink));
            Settings.AddButton(this, "linkHomepage", "HOMEPAGE", () => ShowDialog(HomepageLink));
            Settings.AddButton(this, "linkGithub", "GITHUB", () => ShowDialog(GitHubLink));
            Settings.AddButton(this, "linkNexusmods", "NEXUSMODS", () => ShowDialog(NexusModsLink));
            Settings.AddButton(this, "linkDonate", "DONATE", OnDonateButtonClick, new Color32(37, 59, 128, 255), new Color(1, 1, 1));

            // Activating objects.
            Settings.AddHeader(this, "DESPAWNING");
            ActiveDistance = Settings.AddSlider(this, "activateDistanceNew", "ACTIVATE DISTANCE", 0, 4, 2, textValues: activeDistanceText);
            Settings.AddText(this, "PERFORMANCE MODE");
            ModePerformance = Settings.AddCheckBoxGroup(this, "modePerformance", "PERFORMANCE", false, "performanceMode", MopSettings.UpdatePerformanceMode);
            ModeBalanced = Settings.AddCheckBoxGroup(this, "modeBalanced", "BALANCED", true, "performanceMode", MopSettings.UpdatePerformanceMode);
            ModeQuality = Settings.AddCheckBoxGroup(this, "modeQuality", "QUALITY", false, "performanceMode", MopSettings.UpdatePerformanceMode);
            ModeSafe = Settings.AddCheckBoxGroup(this, "modeSafe", "<color=red>SAFE</color>", false, "performanceMode", MopSettings.UpdatePerformanceMode);
            modeWarningText = Settings.AddDynamicText(this, "");

            // Graphics
            Settings.AddHeader(this, "GRAPHICS");
            LimitFramerate = Settings.AddCheckBox(this, "limitFramerate", "LIMIT FRAMERATE", false, MopSettings.UpdateFramerateLimiter);
            FramerateLimiter = Settings.AddSlider(this, "framerateLimiterUpdated", "FRAMERATE LIMITER (FPS)", 20, 200, 60, MopSettings.UpdateFramerateLimiter);
            ShadowDistance = Settings.AddSlider(this, "shadowDistance", "SHADOW DISTANCE (METERS, 200 DEFAULT)", 0, 2000, 200, MopSettings.UpdateShadows);
            KeepRunningInBackground = Settings.AddCheckBox(this, "keepRunningInBackground", "RUN GAME IN BACKGROUND", true, MopSettings.ToggleBackgroundRunning);
            Settings.AddText(this, "If unchecked, the game will be paused when the game's window looses focus.");
            DynamicDrawDistance = Settings.AddCheckBox(this, "dynamicDrawDistance", "DYNAMIC DRAW DISTANCE", true);
            Settings.AddText(this, "MOP will adjust the draw distance, according to the current situation\n(ex. lower it while inside of a building).");

            GameResolution = Settings.AddDropDownList(this, "", "RESOLUTION", resolutions.ToArray(), selected, () =>
            {
                // Can't use resolution.GetSelectedItemName(), as the selected item name gets updated AFTER the OnSelectionChanged is called.
                var res = Screen.resolutions[GameResolution.GetSelectedItemIndex()];
                string s = res.width + "X" + res.height;
                int width = int.Parse(s.Split('X')[0]);
                int height = int.Parse(s.Split('X')[1]);
                Screen.SetResolution(width, height, Screen.fullScreen);
                ModConsole.Log("[MOP] Setting resolution to " + s);
            });


            // Rules
            Settings.AddHeader(this, "RULES");
            Settings.AddButton(this, "rulesLearnMore", "LEARN MORE", () => ShowDialog(RuleFilesWikiLink));
            RulesAutoUpdate = Settings.AddCheckBox(this, "rulesAutoUpdate", "UPDATE RULES AUTOMATICALLY", true, WarningDisableAutoUpdate);
            RulesAutoUpdateFrequency = Settings.AddSlider(this, "ruleAutoUpdateFrequendy", "AUTO-UPDATE FREQUENCY", 0, 3, 2, textValues: rulesAutoUpdateFrequencyText);
            VerifyRuleFiles = Settings.AddCheckBox(this, "verifyRuleFiles", "VERIFY RULE FILES", true);
            DeleteUnusedRules = Settings.AddCheckBox(this, "deleteUnusedRules", "AUTOMATICALLY DELETE UNUSED RULES", false);
            Settings.AddButton(this, "deleteUnusedRulesButton", "DELETE UNUSED RULES", RulesManager.DeleteUnused);
            Settings.AddButton(this, "forceRulesUpdate", "FORCE UPDATE", ForceRuleFilesUpdate);

            // Other
            Settings.AddHeader(this, "OTHER");
            DestroyEmptyBottles = Settings.AddCheckBox(this, "destroyEmptyBottles", "DESTROY EMPTY BOTTLES", false);
            Settings.AddText(this, "Empty bottles, created by player drinking, won't spawn.");
            DisableEmptyItems = Settings.AddCheckBox(this, "disableEmptyItems", "DISABLE EMPTY ITEMS", false);
            Settings.AddText(this, "Items, which status changes to 'EMPTY', will be automatically disabled, in order to save on PC resources.");
            AlwaysDisableSkidmarks = Settings.AddCheckBox(this, "alwaysDisableSkidmarks", "DISABLE SKIDMARKS", false);
            Settings.AddText(this, "Skidmarks cause a massive memory leak, every time you brake/accelerate hard. Disabling them mitigates that problem.");

            // Logging
            Settings.AddHeader(this, "LOGGING");
            Settings.AddText(this, "If you want to file a bug report, use <color=red><b>I FOUND A BUG</b></color> button!");
            Settings.AddButton(this, "openLogFolder", "OPEN LOGS FOLDER", ExceptionManager.OpenCurrentSessionLogFolder);
            Settings.AddButton(this, "generateModReprt", "GENERATE MOD REPORT", ExceptionManager.GenerateReport);
            Settings.AddButton(this, "deleteAllLogs", "DELETE ALL LOGS", ExceptionManager.DeleteAllLogs);

            // Supporters
            Settings.AddHeader(this, "SUPPORTERS");
            Settings.AddText(this, "<color=yellow>" + GetSupporters() + "</color>" +
                "\n\nDo you want to see your name here? Send a donate!");

            // Changelog
            Settings.AddHeader(this, "CHANGELOG");
            Settings.AddText(this, GetChangelog(false));

            // Info
            Settings.AddHeader(this, "INFO");
            Settings.AddText(this, GetFooter());
#endif
        }
        #endregion

        public override void MenuOnLoad()
        {
#if !PRO
            menuLoaded = true;
#endif
            RemoveUnusedFiles();

            if (!Version.Contains(MopSettings.Data.Version.ToString()))
            {
                MopSettings.Data.Version = Version;
                MopSettings.WriteData(MopSettings.Data);
                string message = DateTime.Now.Month == 12 && DateTime.Now.Day >= 20 ? WelcomeMessageFestive : WelcomeMessage;
                message = string.Format(message, Version, DateTime.Now.Year + 1);
#if PRO
                ModPrompt prompt = ModPrompt.CreateCustomPrompt();
                prompt.DestroyOnDisable = false;
                prompt.Text = $"{message}";
                prompt.Title = "MOP";
                prompt.AddButton("OK", () => { GameObject.Destroy(prompt.gameObject); });
                var btnDonate = prompt.AddButton("<color=#169BD7>DONATE</color>", () => { OnWelcomeDonateClick(); GameObject.Destroy(prompt.gameObject); });
                var btnChangelog = prompt.AddButton("CHANGELOG", () => {
                    ModPrompt.CreatePrompt(GetChangelog(true), $"MOP {ModVersion.Replace("_", " ")} - Changelog", () => { prompt.gameObject.SetActive(true); });
                });
#else
                ModUI.ShowCustomMessage(message, "MOP",
                    new MsgBoxBtn[] {
                        ModUI.CreateMessageBoxBtn("OK"),
                        ModUI.CreateMessageBoxBtn(
                            "DONATE",
                            OnWelcomeDonateClick,
                            new Color32(37, 59, 128, 255),
                            new Color(1, 1, 1)),
                        ModUI.CreateMessageBoxBtn("CHANGELOG", () =>
                        {
                            ModUI.ShowMessage(GetChangelog(true), $"MOP {ModVersion.Replace("_", " ")} - Changelog ");
                        }, true) },
                    new MsgBoxBtn[] { });
                // We must add an extra empty MsgBoxBtn array,
                // as the MSCLoader's ShowCustomMessage is so shit,
                // the buttons won't work otherwise.
#endif
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
                ModUI.ShowMessage($"The game has been reloaded over {MopSettings.MaxRestarts} times, which may cause issues with the game's physics.", "MOP");
            }

            if (MopSettings.GameFixStatus == GameFixStatus.DoFix)
            {
                MopSettings.GameFixStatus = GameFixStatus.Restarted;
                BugReporter.Instance.RestartGame();
                return;
            }

            MopSettings.GameFixStatus = GameFixStatus.None;

            modeWarningText.SetValue("");

            if (!string.IsNullOrEmpty(MOP.SubVersion))
            {
                ModConsole.Log("<size=60><color=magenta>\n\nWARNING!\nYOU ARE USING A PRE-RELEASE VERSION OF MOP!\n\n</color></size>");
            }

            LoadAssetBundle();
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
#if PRO
        public override void PostLoad()
#else
        public override bool SecondPass => true;
        public override void SecondPassOnLoad()
#endif
        {
            MopSettings.UpdateFramerateLimiter();
            MopSettings.UpdatePerformanceMode();
            MopSettings.UpdateShadows();
            MopSettings.UpdateMiscSettings();
            if (CompatibilityManager.IsConfilctingModPresent(out string modName))
            {
                ModConsole.LogError($"MOP could not be loaded, because the following mod is present: <color=yellow>{modName}</color>");
                return;
            }

            // Create MOP game object
            GameObject mop = new GameObject("MOP");

            // Initialize CompatibiliyManager
            CompatibilityManager.Initialize();

            // Add Hypervisor class
            mop.AddComponent<Hypervisor>();

            SaveManager.AddSaveFlag();

            modeWarningText.SetValue(WarningMode);
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
        string GetChangelog(bool useWelcomeScreenFormatting)
        {
            if (string.IsNullOrEmpty(Properties.Resources.changelog))
            {
                return "Dog ate my changelog :(";
            }

            StringBuilder sb = new StringBuilder();

            string[] changelog = Properties.Resources.changelog.Split('\n');

            bool skipNext = false;
            for (int i = 0; i < changelog.Length; i++)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }
                string line = changelog[i];

                // If line starts with ###, make it look like a header of section.
                if (line.StartsWith("###"))
                {
                    line = line.Replace("###", "");
                    if (useWelcomeScreenFormatting)
                    {
                        line = $"<color=yellow>{line}</color>";
                        skipNext = true;
                    }
                    else
                    {
                        line = $"<color=yellow><size=24>{line}</size></color>";
                    }
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

                line = line.Replace("(Beta)", "<color=orange>Beta: </color>")
                        .Replace("(My Summer Car Bug)", "<color=green>My Summer Car Bug: </color>")
                        .Replace("(My Summer Car)", "<color=green>My Summer Car: </color>")
                        .Replace("Rule Files API:", "<color=cyan>Rule Files API:</color>")
                        .Replace("(MSCLoader)", "<color=yellow>MSCLoader:</color>")
                        .Replace("(Mod Loader Pro)", "<color=yellow>Mod Loader Pro:</color>");

                sb.AppendLine(line);

                if (i >= 20 && useWelcomeScreenFormatting)
                {
                    sb.Append("..and more.");
                    break;
                }
            }

            return sb.ToString();
        }

        public static void ShowDialog(string url)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                System.Diagnostics.Process.Start(url);
                return;
            }

            ModUI.ShowYesNoMessage($"This will open the following link:\n" +
                                $"<color=yellow>{url}</color>\n\n" +
                                $"Are you sure you want to continue?",
                                "MOP",
                                () => System.Diagnostics.Process.Start(url));
        }

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

#if PRO
        void UpdateSettingsUI()
        {
            // UI Update.
            if ((int)FramerateLimiter.Value == 21)
            {
                FramerateLimiter.valueText.text = "Disabled";
            }
            if (ShadowDistance.Value == 0)
            {
                ShadowDistance.valueText.text = "No Shadows";
            }

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

            modVersion = Version;
        }
#endif

        string GetFooter()
        {
            return $"<color=yellow>MOP</color> {ModVersion.Replace("_", " ")}\n" +
#if PRO
                    $"<color=yellow>Mod Loader Pro</color> {ModLoader.Version}\n" +
#else
                    $"<color=yellow>MSCLoader</color> {ModLoader.MSCLoader_Ver}\n" +
#endif
                    $"{ExceptionManager.GetSystemInfo()}\n" +
                    $"\nCopyright © Konrad Figura 2019-{DateTime.Now.Year}";
        }

        void WarningDisableAutoUpdate()
        {
            if (!RulesAutoUpdate.GetValue())
            {
                ModUI.ShowMessage("<color=yellow>Warning!</color>\n\n" +
                                  "Disabling rule files auto update means newly installed mods might not work, or MOP might break.", "MOP");
            }
        }
#if !PRO
        public override void OnModEnabled()
        {
            if (!menuLoaded)
            {
                MenuOnLoad();
                ModSettingsLoaded();
            }
        }
#endif

        public static GameObject LoadAssetBundle()
        {
            if (MopLoadScreenPrefab == null)
            {
#if PRO
                AssetBundle bundle = ModAssets.LoadBundle(Properties.Resources.mop);
#else
                AssetBundle bundle = LoadAssets.LoadBundle(Properties.Resources.mop);
#endif
                MopLoadScreenPrefab = bundle.LoadAsset<GameObject>("MOP_Canvas.prefab");
                bundle.Unload(false);
            }

            return MopLoadScreenPrefab;
        }

        private string GetSupporters()
        {
            return Properties.Resources.donates;
        }

        private void OnWelcomeDonateClick()
        {
            OnDonateButtonClick();
        }

        private void OnDonateButtonClick()
        {
#if PRO
            ModPrompt prompt = ModPrompt.CreateCustomPrompt();
            prompt.Title = "DONATE";
            prompt.Text = "Choose your donation method:";
            prompt.AddButton("PAYPAL", () => Process.Start(DonateLink));
            prompt.AddButton("KO-FI", () => Process.Start(KofiLink));
            prompt.AddButton("CANCEL", null);
#else
            ModUI.ShowCustomMessage("Choose your donation method:", "DONATE", new MsgBoxBtn[]
            {
                ModUI.CreateMessageBoxBtn("PAYPAL", () => Process.Start(DonateLink)),
                ModUI.CreateMessageBoxBtn("KO-FI", () => Process.Start(KofiLink))
            }, new MsgBoxBtn[]
            {
                ModUI.CreateMessageBoxBtn("CANCEL")
            });
#endif
        }
    }
}
