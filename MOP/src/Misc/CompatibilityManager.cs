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

using MSCLoader;
using UnityEngine;

namespace MOP
{
    class CompatibilityManager
    {
        // This script manages the compatibility between other mods

        public static CompatibilityManager instance;

        // CD Player Enhanced
        // https://www.racedepartment.com/downloads/cd-player-enhanced.19002/
        public bool CDPlayerEnhanced { get; private set; }

        // CarryMore
        // https://www.racedepartment.com/downloads/carry-more-backpack-alternative.22396/
        public bool CarryMore { get; private set; }
        public readonly Vector3 CarryMoreTempPosition = new Vector3(0.0f, -1000.0f, 0.0f);

        public CompatibilityManager()
        {
            instance = this;
            CDPlayerEnhanced = IsModPresent("CDPlayer");
            CarryMore = IsModPresent("CarryMore");
            //ModConsole.Print("[MOP] Compatibility Manager done");
        }

        /// <summary>
        /// Checks if mod is present by modID using ModLoader.IsModPresent.
        /// </summary>
        /// <param name="modID"></param>
        /// <returns></returns>
        bool IsModPresent(string modID)
        {
            bool isModPresent = ModLoader.IsModPresent(modID);

            if (isModPresent)
            {
                string modName = modID;

                foreach (var mod in ModLoader.LoadedMods)
                    if (mod.ID == modID)
                        modName = mod.Name;

                //ModConsole.Print($"[MOP] {modName} has been found!");
            }

            return isModPresent;
        }
    }
}
