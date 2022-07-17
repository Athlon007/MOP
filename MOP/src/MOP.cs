// Modern Optimization Plugin
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
using System.IO;
using UnityEngine;
using MSCLoader;

using MOP.FSM;
using MOP.Common;
using MOP.Helpers;
using MOP.Rules;
#if PRO
using System.Collections.Generic;
#endif

namespace MOP
{
    public class MOP : Mod
    {
        public override string ID => "MOP";
        public override string Name => "MODERN OPTIMIZATION PLUGIN";
        public override string Author => "Athlon"; //Your Username
        public override string Version => "3.8"; //Version
        public const string SubVersion = "BETA_1"; // NIGHTLY-yyyymmdd | BETA_x | RC_
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


        public GameObject MopBundle { get; private set; }

        // Settings
#if PRO
        public override string UpdateLink => "https://github.com/Athlon007/MOP";

        static internal SettingSlider ActiveDistance, FramerateLimiter, ShadowDistance, RulesAutoUpdateFrequency;
        static internal SettingRadioButtons PerformanceModes, Resolution;
        static internal SettingToggle EnableShadowAdjusting, KeepRunningInBackground,
                                      DynamicDrawDistance, RulesAutoUpdate, VerifyRuleFiles, DeleteUnusedRules,
                                      DestroyEmptyBottles, DisableEmptyItems, LazySectorUpdating,
                                      AlwaysDisableSkidmarks, FastLoading;
        SettingText modeWarningText;
#else
        static internal SettingsSliderInt ActiveDistance, FramerateLimiter, ShadowDistance, RulesAutoUpdateFrequency;
        static internal SettingsCheckBoxGroup ModePerformance, ModeBalanced, ModeQuality, ModeSafe;
        static internal SettingsCheckBox KeepRunningInBackground, LimitFramerate, DynamicDrawDistance,
                                          RulesAutoUpdate, VerifyRuleFiles, DeleteUnusedRules,
                                          DestroyEmptyBottles, DisableEmptyItems,
                                          AlwaysDisableSkidmarks, FastLoading;
        SettingsDynamicText modeWarningText;
#endif

        // Constant text.
        const string WarningMode = "Some changes will be applied after game reload.";
        readonly string[] activeDistanceText = { "Close (0.75x)", "Normal (1x)", "Far (2x)", "Very Far (4x)" };
        readonly string[] rulesAutoUpdateFrequencyText = { "Every launch", "Daily", "Every 2 days", "Weekly" };
        const string WelcomeMessage = "Welcome to Modern Optimization Plugin <color=yellow>{0}</color>!\n\n" +
                                      "Please consider supporting the project using <color=#3687D7>PayPal</color>, or on <color=orange>NexusMods</color>.";
        const string WelcomeMessageFestive = "Merry Christmas and Happy New Year {1}!\n\n" +
                                             "Welcome to Modern Optimization Plugin <color=yellow>{0}</color>!\n" +
                                             "Please consider supporting the project using <color=#3687D7>PayPal</color>, or on <color=orange>NexusMods</color>.";

        public static GameObject MopLoadScreenPrefab { get; private set; }
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
            modVersion = Version;
            modConfigPath = ModLoader.GetModSettingsFolder(this);
#if DEBUG
            Settings.AddHeader(this, "Shh...Don't leak my hard work ;)", Color.yellow, Color.black);
#endif

#if PRO
            modSettings.AddButton("iFoundABug", "<color=red>I FOUND A BUG</color>", BugReporter.FileBugReport);
            modSettings.AddButton("faq", "FAQ", () => ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/faq"));
            modSettings.AddButton("wiki", "WIKI", () => ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/"));
            modSettings.AddButton("homepage", "HOMEPAGE", () => ShowDialog("http://athlon.kkmr.pl/"));
            modSettings.AddButton("github", "GITHUB", () => ShowDialog("https://github.com/Athlon007/MOP"));
            modSettings.AddButton("homepage", "NEXUSMODS", () => ShowDialog("https://www.nexusmods.com/mysummercar/mods/146"));
            modSettings.AddButton("paypal", "<color=#254280>DONATE</color>", () => ShowDialog("https://www.paypal.com/donate/?hosted_button_id=8VASR9RLLS76Y"));

            // Activating objects.
            modSettings.AddHeader("DESPAWNING");
            ActiveDistance = modSettings.AddSlider("activateDistance", "ACTIVATE DISTANCE", 1, 0, 3);
            ActiveDistance.AddTooltip("Distance uppon which objects will spawn.");
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
            EnableShadowAdjusting.AddTooltip("Allows you to set the shadow render distance with the slider below.");
            ShadowDistance = modSettings.AddSlider("shadowDistance", "SHADOW DISTANCE", 2, 0, 20, () => { MopSettings.UpdateShadows(); UpdateSettingsUI(); });
            ShadowDistance.ValueSuffix = "00 Meters";
            ShadowDistance.gameObject.SetActive(EnableShadowAdjusting.Value);
            KeepRunningInBackground = modSettings.AddToggle("keepRunningInBackground", "RUN GAME IN BACKGROUND", true, MopSettings.ToggleBackgroundRunning);
            KeepRunningInBackground.AddTooltip("If unchecked, game will be paused when the game's window looses focus.");
            DynamicDrawDistance = modSettings.AddToggle("dynamicDrawDistance", "DYNAMIC DRAW DISTANCE", true);
            DynamicDrawDistance.AddTooltip("MOP will adjust the draw distance according to the current situation\n(ex. lower it while inside of a building).");
            modSettings.AddButton("changeResolution", "RESOLUTION", () => { Resolution.gameObject.SetActive(!Resolution.gameObject.activeSelf); });
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
            SettingButton learnMore = modSettings.AddButton("rulesLearnMore", "LEARN MORE", () => ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/rulefiles"));
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
            DisableEmptyItems = modSettings.AddToggle("disableEmptyItems", "DISABLE EMPTY ITEMS", false);
            AlwaysDisableSkidmarks = modSettings.AddToggle("alwaysDisableSkidmarks", "DISABLE SKIDMARKS", false);
            AlwaysDisableSkidmarks.AddTooltip("Skidmarks cause a massive memory leak,\nevery time you brake/accelerate hard.\nDisabling them mitigates that problem.");
            FastLoading = modSettings.AddToggle("fastLoading", "FAST LOADING", false);
            FastLoading.AddTooltip("Decreases the time it takes for MOP to load.\nThis can lead to engine parts\nor textures not loading as intended.");

            // Logging
            modSettings.AddHeader("LOGGING");
            modSettings.AddText("If you want to file a bug report, use <color=red>I FOUND A BUG</color> button!");
            modSettings.AddButton("openLogFolder", "OPEN LOGS FOLDER", "", ExceptionManager.OpenCurrentSessionLogFolder);
            modSettings.AddButton("generateModReprt", "GENERATE MOD REPORT", "", ExceptionManager.GenerateReport);
            modSettings.AddButton("deleteAllLogs", "DELETE ALL LOGS", "", ExceptionManager.DeleteAllLogs);

            // Supporters
            modSettings.AddHeader("SUPPORTERS");
            modSettings.AddText("<color=yellow>" + Properties.Resources.donates + "</color>" +
                "\n\nDo you want to see your name here? Send a donate and proof of transaction to MOP's author.");

            // Changelog
            modSettings.AddHeader("CHANGELOG");
            modSettings.AddText(GetChangelog());

            // Info
            modSettings.AddHeader("INFO");
            modSettings.AddText(GetFooter());

            UpdateSettingsUI();
#else
            Settings.AddButton(this, "iFoundABug", "<color=red>I FOUND A BUG</color>", BugReporter.FileBugReport);
            Settings.AddButton(this, "linkFAQ", "FAQ", () => ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/faq"));
            Settings.AddButton(this, "linkWiki", "WIKI", () => ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/"));
            Settings.AddButton(this, "linkHomepage", "HOMEPAGE", () => ShowDialog("http://athlon.kkmr.pl/"));
            Settings.AddButton(this, "linkGithub", "GITHUB", () => ShowDialog("https://github.com/Athlon007/MOP"));
            Settings.AddButton(this, "linkNexusmods", "NEXUSMODS", () => ShowDialog("https://www.nexusmods.com/mysummercar/mods/146"));
            Settings.AddButton(this, "linkDonate", "DONATE", () => ShowDialog("https://www.paypal.com/donate/?hosted_button_id=8VASR9RLLS76Y"), new Color32(37, 59, 128, 255), new Color(1, 1, 1));

            // Activating objects.
            Settings.AddHeader(this, "DESPAWNING");
            ActiveDistance = Settings.AddSlider(this, "activateDistance", "ACTIVATE DISTANCE", 0, 3, 1, textValues: activeDistanceText);
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
            Settings.AddText(this, "If unchecked, game will be paused when the game's window looses focus.");
            DynamicDrawDistance = Settings.AddCheckBox(this, "dynamicDrawDistance", "DYNAMIC DRAW DISTANCE", true);
            Settings.AddText(this, "MOP will adjust the draw distance according to the current situation\n(ex. lower it while inside of a building).");

            // Rules
            Settings.AddHeader(this, "RULES");
            Settings.AddButton(this, "rulesLearnMore", "LEARN MORE", () => ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/rulefiles"));
            RulesAutoUpdate = Settings.AddCheckBox(this, "rulesAutoUpdate", "UPDATE RULES AUTOMATICALLY", true, WarningDisableAutoUpdate);
            RulesAutoUpdateFrequency = Settings.AddSlider(this, "ruleAutoUpdateFrequendy", "AUTO-UPDATE FREQUENCY", 0, 3, 2, textValues: rulesAutoUpdateFrequencyText);
            VerifyRuleFiles = Settings.AddCheckBox(this, "verifyRuleFiles", "VERIFY RULE FILES", true);
            DeleteUnusedRules = Settings.AddCheckBox(this, "deleteUnusedRules", "AUTOMATICALLY DELETE UNUSED RULES", false);
            Settings.AddButton(this, "deleteUnusedRulesButton", "DELETE UNUSED RULES", RulesManager.DeleteUnused);
            Settings.AddButton(this, "forceRulesUpdate", "FORCE UPDATE", ForceRuleFilesUpdate);

            // Other
            Settings.AddHeader(this, "OTHER");
            DestroyEmptyBottles = Settings.AddCheckBox(this, "destroyEmptyBottles", "DESTROY EMPTY BOTTLES", false);
            DisableEmptyItems = Settings.AddCheckBox(this, "disableEmptyItems", "DISABLE EMPTY ITEMS", false);
            AlwaysDisableSkidmarks = Settings.AddCheckBox(this, "alwaysDisableSkidmarks", "DISABLE SKIDMARKS", false);
            Settings.AddText(this, "Skidmarks cause a massive memory leak, every time you brake/accelerate hard. Disabling them mitigates that problem.");
            FastLoading = Settings.AddCheckBox(this, "fastLoading", "FAST LOADING", false);
            Settings.AddText(this, "Decreases the time it takes for MOP to load. This can lead to engine parts or textures not loading as intended.");

            // Logging
            Settings.AddHeader(this, "LOGGING");
            Settings.AddText(this, "If you want to file a bug report, use <color=red><b>I FOUND A BUG</b></color> button!");
            Settings.AddButton(this, "openLogFolder", "OPEN LOGS FOLDER", ExceptionManager.OpenCurrentSessionLogFolder);
            Settings.AddButton(this, "generateModReprt", "GENERATE MOD REPORT", ExceptionManager.GenerateReport);
            Settings.AddButton(this, "deleteAllLogs", "DELETE ALL LOGS", ExceptionManager.DeleteAllLogs);

            // Supporters
            Settings.AddHeader(this, "SUPPORTERS");
            Settings.AddText(this, "<color=yellow>" + Properties.Resources.donates + "</color>" +
                "\n\nDo you want to see your name here? Send a donate and proof of transaction to MOP's author.");

            // Changelog
            Settings.AddHeader(this, "CHANGELOG");
            Settings.AddText(this, GetChangelog());

            // Info
            Settings.AddHeader(this, "INFO");
            Settings.AddText(this,  GetFooter());
#endif
        }
#endregion

        public override void MenuOnLoad()
        {
#if !PRO
            menuLoaded = true;
#endif
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
                ModUI.ShowYesNoMessage($"Game has been reloaded over {MopSettings.MaxRestarts} times, which may cause issues with game physics.", "MOP", Application.Quit);
            }

            if (MopSettings.GameFixStatus == Common.Enumerations.GameFixStatus.DoFix)
            {
                MopSettings.GameFixStatus = Common.Enumerations.GameFixStatus.Restarted;
                BugReporter.Instance.RestartGame();
                return;
            }
            
            MopSettings.GameFixStatus = Common.Enumerations.GameFixStatus.None;

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

                line = line.Replace("(Beta)", "<color=orange>Beta: </color>");
                line = line.Replace("(My Summer Car Bug)", "<color=green>My Summer Car Bug: </color>");
                line = line.Replace("(My Summer Car)", "<color=green>My Summer Car: </color>");
                line = line.Replace("Rule Files API:", "<color=cyan>Rule Files API:</color>");
                line = line.Replace("(MSCLoader)", "<color=yellow>MSCLoader:</color>");
                line = line.Replace("(Mod Loader Pro)", "<color=yellow>Mod Loader Pro:</color>");

                output += line + "\n";
            }

            return output;
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
            return  $"<color=yellow>MOP</color> {ModVersion.Replace("_", " ")}\n" +
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
            if (RulesAutoUpdate.GetValue() == false)
            {
                ModUI.ShowMessage("<color=yellow>Warning!</color>\n\n" +
                                  "Disabling rule files auto update means newly installed mods might not work.", "MOP");
            }
        }
        
        long last = 0;
        TextMesh fps;
        TextMesh fpsShadow;
        long[] differenceAverage = new long[128];
        int differenceCounter = 0;
        public override void Update()
        {
            if (!RulesManager.Instance.SpecialRules.ShowGarbageMemoryUsage) return;

            if (fps == null)
            {
                GameObject fpsObject = GameObject.Find("GUI").transform.Find("HUD/FPS/HUDValue").gameObject;
                PlayMakerFSM[] fsms = fpsObject.GetComponents<PlayMakerFSM>();
                foreach (var fsm in fsms)
                    fsm.enabled = false;

                fps = fpsObject.GetComponent<TextMesh>();
                fpsShadow = fpsObject.transform.Find("HUDValueShadow").GetComponent<TextMesh>();
            }
            long mem = GC.GetTotalMemory(false);
            long diff = mem - last;
            differenceAverage[differenceCounter] = diff;
            differenceCounter++;
            if (differenceCounter >= differenceAverage.Length) differenceCounter = 0;

            long averageDiff = 0;
            int divBy = 0;
            for (int i = 0; i < differenceAverage.Length; ++i)
            {
                if (differenceAverage[i] <= 0) continue;
                averageDiff += differenceAverage[i];
                divBy++;
            }

            averageDiff /= divBy;

            string text = string.Format("{0, -10} {1, 10}", mem, averageDiff);
            fps.text = text;
            fpsShadow.text = text;
            last = mem;
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

        private void LoadAssetBundle()
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
        }
    }
}
