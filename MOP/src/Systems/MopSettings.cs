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

using System.IO;
using UnityEngine;

namespace MOP
{
    static class MopSettings
    {
        // This is the master switch of MOP. If deactivated, all functions will freeze.
        public static bool IsModActive { get; set; }

        // ACTIVATING OBJECTS
        public static int ActiveDistance { get; private set; }
        public static float ActiveDistanceMultiplicationValue { get; private set; }

        public static bool SafeMode { get; private set; }
        public static bool RemoveEmptyBeerBottles { get; private set; }
        public static bool SatsumaTogglePhysicsOnly { get; private set; }

        // RULE FILES
        public static bool RuleFilesAutoUpdateEnabled { get => (bool)MOP.RulesAutoUpdate.GetValue(); }
        
        // Distance after which car physics is toggled.
        public const int UnityCarActiveDistance = 5;

        // Debugging functionality.
        public static bool SectorDebugMode;

        public static void UpdateAll()
        {
            // Activating Objects
            ActiveDistance = int.Parse(MOP.ActiveDistance.GetValue().ToString());
            ActiveDistanceMultiplicationValue = GetActiveDistanceMultiplicationValue();
            SafeMode = (bool)MOP.SafeMode.GetValue();

            // Others
            RemoveEmptyBeerBottles = (bool)MOP.RemoveEmptyBeerBottles.GetValue();
            SatsumaTogglePhysicsOnly = (bool)MOP.SatsumaTogglePhysicsOnly.GetValue();

            // Framerate limiter
            Application.targetFrameRate = (bool)MOP.EnableFramerateLimiter.GetValue() ? int.Parse(MOP.FramerateLimiter.GetValue().ToString()) : -1;
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
                default:
                    return 7;
                case 0:
                    return 1;
                case 1:
                    return 2;
            }
        }

        public static void AgreeData()
        {
            File.Create($"{MOP.ModConfigPath}/DataAgreed.mop");
        }

        public static bool DataSendingAgreed()
        {
            return File.Exists($"{MOP.ModConfigPath}/DataAgreed.mop");
        }

        public static void EnableSafeMode()
        {
            SafeMode = true;
        }
    }
}
