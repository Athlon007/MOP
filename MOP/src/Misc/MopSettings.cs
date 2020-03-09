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

using UnityEngine;

namespace MOP
{
    class MopSettings
    {
        // This is the master switch of MOP. If deactivated, all functions will freeze.
        public static bool IsModActive { get; set; }

        //
        // ACTIVATING OBJECTS
        //
        public static int ActiveDistance { get; private set; }
        public static float ActiveDistanceMultiplicationValue { get; private set; }

        public static bool SafeMode { get; set; }

        //public static bool ToggleVehicles { get; private set; }

        //public static bool ToggleItems { get; private set; }

        static bool ignoreModVehicles = false;
        public static bool IgnoreModVehicles { get => ignoreModVehicles; }

        //
        // OTHERS
        //
        static bool removeEmptyBeerBottles = false;
        public static bool RemoveEmptyBeerBottles { get => removeEmptyBeerBottles; }

        static bool satsumaTogglePhysicsOnly = false;
        public static bool SatsumaTogglePhysicsOnly { get => satsumaTogglePhysicsOnly; }

        static bool toggleVehiclePhysicsOnly = false;
        public static bool ToggleVehiclePhysicsOnly { get => toggleVehiclePhysicsOnly; }

        //
        // MISCELLANEOUS
        //

        // Distance after which car physics are toggled
        public const int UnityCarActiveDistance = 5;

        public static void UpdateAll()
        {
            // Activating Objects
            ActiveDistance = int.Parse(MOP.activeDistance.GetValue().ToString());
            ActiveDistanceMultiplicationValue = GetActiveDistanceMultiplicationValue();
            SafeMode = (bool)MOP.safeMode.GetValue();
            //ToggleVehicles = (bool)MOP.toggleVehicles.GetValue();
            //ToggleItems = (bool)MOP.toggleItems.GetValue();
            ignoreModVehicles = (bool)MOP.ignoreModVehicles.GetValue();

            // Others
            removeEmptyBeerBottles = (bool)MOP.removeEmptyBeerBottles.GetValue();
            satsumaTogglePhysicsOnly = (bool)MOP.satsumaTogglePhysicsOnly.GetValue();
            toggleVehiclePhysicsOnly = (bool)MOP.toggleVehiclePhysicsOnly.GetValue();

            // Framerate limiter
            Application.targetFrameRate = (bool)MOP.enableFramerateLimiter.GetValue() ? int.Parse(MOP.framerateLimiter.GetValue().ToString()) : -1;
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
                    return 0.5f;
                default: // 1
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 4;
            }
        }
    }
}
 