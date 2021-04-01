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
using System.Linq;
using UnityEngine;

using MOP.Common.Enumerations;

namespace MOP.Common
{
    static class MopSettings
    {
        // This is the master switch of MOP. If deactivated, all functions will freeze.
        public static bool IsModActive { get; set; }
        

        // ACTIVATING OBJECTS
        public static int ActiveDistance { get; private set; }
        public static float ActiveDistanceMultiplicationValue { get; private set; }
        public static PerformanceMode Mode = PerformanceMode.Balanced;

        // GRAPHICS
        public static bool DynamicDrawDistance { get; private set; }
        static float shadowDistanceOriginalValue;
        
        // OTHER
        public static bool RemoveEmptyBeerBottles { get; private set; }
        public static bool RemoveEmptyItems { get; private set; }

        // RULE FILES
        public static bool RuleFilesAutoUpdateEnabled { get => (bool)MOP.RulesAutoUpdate.GetValue(); }
        public static bool RuleFilesUpdateChecked;
        
        // Distance after which car physics is toggled.
        public const int UnityCarActiveDistance = 5;

        // Debugging functionality.
        public static bool GenerateToggledItemsListDebug;

        static int vsyncCount = -1;

        const string firstTimeWindowFile = "FirstTimeLaunch.txt";
        readonly static string firstTimePath = $"{MOP.ModConfigPath}/{firstTimeWindowFile}";

        // Tracks if the game has been fully loaded at east once.
        public static bool LoadedOnce;

        public static void UpdateAll()
        {
            // Activating Objects
            ActiveDistance = int.Parse(MOP.ActiveDistance.GetValue().ToString());
            ActiveDistanceMultiplicationValue = GetActiveDistanceMultiplicationValue();

            // MODES
            // Show the warning about safe mode, if the player disables safe mode and is not in main menu.
            if (ModLoader.GetCurrentScene() != CurrentScene.MainMenu && IsModActive)
            {
                if (Mode == PerformanceMode.Safe && !(bool)MOP.ModeSafe.GetValue())
                {
                    ModUI.ShowMessage("Safe Mode will be disabled after the restart.", "MOP");
                }
                else if (Mode != PerformanceMode.Safe && (bool)MOP.ModeSafe.GetValue())
                {
                    ModUI.ShowMessage("Safe Mode will be enabled after the restart.", "MOP");
                }
            }

            if ((bool)MOP.ModeSafe.GetValue() && ModLoader.GetCurrentScene() != CurrentScene.MainMenu)
            {
                Mode = PerformanceMode.Safe;
            }
            else if ((bool)MOP.ModePerformance.GetValue())
            {
                Mode = PerformanceMode.Performance;
            }
            else if ((bool)MOP.ModeBalanced.GetValue())
            {
                Mode = PerformanceMode.Balanced;
            }
            else if ((bool)MOP.ModeQuality.GetValue())
            {
                Mode = PerformanceMode.Quality;
            }

            // GRAPHICS
            DynamicDrawDistance = (bool)MOP.DynamicDrawDistance.GetValue();

            // Others
            RemoveEmptyBeerBottles = (bool)MOP.RemoveEmptyBeerBottles.GetValue();
            RemoveEmptyItems = (bool)MOP.RemoveEmptyItems.GetValue();

            // Framerate limiter
            Application.targetFrameRate = (bool)MOP.EnableFramerateLimiter.GetValue() ? int.Parse(MOP.FramerateLimiterText[int.Parse(MOP.FramerateLimiter.GetValue().ToString())]) : 200;

            // Shadow distance.
            if (shadowDistanceOriginalValue == 0)
                shadowDistanceOriginalValue = QualitySettings.shadowDistance;
            QualitySettings.shadowDistance = (bool)MOP.EnableShadowAdjusting.GetValue() ? 
                                             float.Parse(MOP.ShadowDistance.GetValue().ToString()) : shadowDistanceOriginalValue;

            ToggleBackgroundRunning();

            // Vsync fix.
            if (vsyncCount == -1)
                vsyncCount = QualitySettings.vSyncCount;
            else
                QualitySettings.vSyncCount = vsyncCount;

            System.GC.Collect();
        }

        /// <summary>
        /// Returns the value that is used to multiplify the active distance of an object.
        /// So for example, if the default active distance of the object is 200 units, 
        /// and the multiplication value is 0.5, the actual active distance will be 100 units.
        /// </summary>
        static float GetActiveDistanceMultiplicationValue()
        {
            switch (ActiveDistance)
            {
                case 0:
                    return 0.75f;
                default: // 1
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 4;
            }
        }

        public static int GetRuleFilesUpdateDaysFrequency()
        {
            switch (int.Parse(MOP.RulesAutoUpdateFrequency.GetValue().ToString()))
            {
                case 0:
                    return 0;
                case 1:
                    return 1;
                case 2:
                    return 2;
                default:
                    return 7;
            }
        }

        /// <summary>
        /// Creates the file that marks that the first time launch window has been showed.
        /// On next launch it will be checked by HasFirstTimeWindowBeenShown() function.
        /// </summary>
        public static void FirstTimeWindowShown()
        {
            using (StreamWriter sw = new StreamWriter(firstTimePath))
            {
                string version = MOP.ModVersion;
                if (version.Count(x => x == '.') > 1)
                {
                    int lastDot = version.LastIndexOf('.');
                    version = version.Substring(0, lastDot);
                }

                sw.Write(version);
            }
        }

        /// <summary>
        /// Checks if the first time launch window has been displayed already.
        /// </summary>
        /// <returns></returns>
        public static bool HasFirstTimeWindowBeenShown()
        { 
            if (!File.Exists(firstTimePath))
            {
                return false;
            }

            using (StreamReader sr = new StreamReader(firstTimePath))
            {
                if (sr.ReadToEnd().Length == 0)
                {
                    return false;
                }

                return MOP.ModVersion.StartsWith(sr.ReadToEnd());
            }
        }

        public static void EnableSafeMode()
        {
            Mode = PerformanceMode.Safe;
        }

        public static void ToggleVerifyRuleFiles()
        {
            if (!(bool)MOP.VerifyRuleFiles.GetValue())
            {
                ModUI.ShowMessage("<color=yellow><b>Warning!</b></color>\n\n" +
                    "For safety reasons, MOP verifies all rule files. Disabling rule file verification " +
                    "may lead to dangerous rule files being installed, that potentially may harm performance, " +
                    "or even damage your save game.\n\n" +
                    "Do not disable this, unless you're a mod maker.", "MOP");
            }
        }

        public static void ToggleBackgroundRunning()
        {
            Application.runInBackground = (bool)MOP.KeepRunningInBackground.GetValue();
        }

        public static bool IsMySummerCar()
        {
            return Application.productName == "My Summer Car";
        }
    }
}
