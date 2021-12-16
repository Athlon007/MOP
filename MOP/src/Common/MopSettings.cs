﻿// Modern Optimization Plugin
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
using UnityEngine;

using MOP.Common.Enumerations;

namespace MOP.Common
{
    static class MopSettings
    {
        // This is the master switch of MOP. If deactivated, all functions will freeze.
        public static bool IsModActive { get; set; }

        public static PerformanceMode Mode;

        static float shadowDistanceOriginalValue;

        // Distance after which car physics is toggled.
        public const int UnityCarActiveDistance = 5;

        // Debugging functionality.
        public static bool GenerateToggledItemsListDebug;

        static int vsyncCount = -1;

        // Tracks if the game has been fully loaded at least once.
        public static bool LoadedOnce;

        internal static int Restarts = 0;
        internal const int MaxRestarts = 5;
        internal static bool RestartWarningShown = false;

        public static bool AttemptedToFixTheGame = false;
        public static bool AttemptedToFixTheGameRestart = false;

        internal static void UpdatePerformanceMode()
        {
            // MODES
            // Show the warning about safe mode, if the player disables safe mode and is not in main menu.
            bool dontUpdate = false;
            if (ModLoader.CurrentScene != CurrentScene.MainMenu)
            {
                if (Mode == PerformanceMode.Safe && MOP.PerformanceModes.Value != 3)
                {
                    ModPrompt.CreatePrompt("Safe Mode will be disabled after you quit to the Main Menu.", "MOP");
                    dontUpdate = true;
                }
                else if (Mode != PerformanceMode.Safe && MOP.PerformanceModes.Value == 3)
                {
                    ModPrompt.CreatePrompt("Safe Mode will be enabled after you quit to the Main Menu.", "MOP");
                    dontUpdate = true;
                }
            }

            if (!dontUpdate)
            {
                Mode = (PerformanceMode)MOP.PerformanceModes.Value;
            }
        }

        internal static void UpdateFramerateLimiter()
        {
            // Framerate limiter
            Application.targetFrameRate = (int)MOP.FramerateLimiter.Value != 21 ? (int)MOP.FramerateLimiter.Value * 10 : -1;
            if ((int)MOP.FramerateLimiter.Value == 21)
            {
                MOP.FramerateLimiter.valueText.text = "Disabled";
            }
        }

        internal static void UpdateShadows()
        {
            // Shadow distance.
            if (shadowDistanceOriginalValue == 0)
                shadowDistanceOriginalValue = QualitySettings.shadowDistance;
            QualitySettings.shadowDistance = MOP.EnableShadowAdjusting.Value ?
                                             MOP.ShadowDistance.Value * 100 : shadowDistanceOriginalValue;

            if (MOP.ShadowDistance.Value == 0)
            {
                MOP.ShadowDistance.valueText.text = "No Shadows";
            }
        }

        public static void UpdateMiscSettings()
        {
            ToggleBackgroundRunning();

            // Vsync fix.
            if (vsyncCount == -1)
                vsyncCount = QualitySettings.vSyncCount;
            else
                QualitySettings.vSyncCount = vsyncCount;

            System.GC.Collect(); // (and collect garbage)
        }

        /// <summary>
        /// Returns the value that is used to multiplify the active distance of an object.
        /// So for example, if the default active distance of the object is 200 units,
        /// and the multiplication value is 0.5, the actual active distance will be 100 units.
        /// </summary>
        public static float ActiveDistanceMultiplicationValue
        {
            get
            {
                switch ((int)MOP.ActiveDistance.Value)
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
        }

        public static int GetRuleFilesUpdateDaysFrequency()
        {
            switch ((int)MOP.RulesAutoUpdateFrequency.Value)
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

        public static void EnableSafeMode()
        {
            Mode = PerformanceMode.Safe;
        }

        internal static void ToggleBackgroundRunning()
        {
            Application.runInBackground = MOP.KeepRunningInBackground.Value;
        }

        public static bool IsMySummerCar => Application.productName == "My Summer Car";

        internal static bool IsConfilctingModPresent(out string conflictingModName)
        {
            string[] arr = { "KruFPS", "ImproveFPS", "OptimizeMSC", "ZZDisableAll", "DisableAll" };
            foreach (string a in arr)
            {
                if (ModLoader.GetMod(a) != null)
                {
                    conflictingModName = ModLoader.GetMod(a).Name;
                    return true;
                }
            }

            conflictingModName = "";
            return false;
        }

        internal static bool IsMSCLoader => GameObject.Find("MSCLoader Canvas").transform.Find("ModLoaderUI") == null;
    }
}
