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
using UnityEngine;
using MOP.Items;

namespace MOP.Common
{
    class CompatibilityManager
    {
        // This script manages the compatibility with other mods
       
        // Use this method of ensuring compatibility ONLY in specific cases,
        // that are not supported by Rule Files.

        // CarryMore
        // https://www.racedepartment.com/downloads/carry-more-backpack-alternative.22396/
        static bool CarryMore { get; set; }
        
        // Advanced Backpack
        static bool AdvancedBackpack { get; set; }
        static readonly Vector3 AdvancedBackpackPosition = new Vector3(630f, 10f, 1140f);
        const int AdvancedBackpackDistance = 30;

        public CompatibilityManager()
        {
            CarryMore = ModLoader.GetMod("CarryMore") != null;
            AdvancedBackpack = ModLoader.GetMod("AdvancedBackpack") != null;
        }

        public static bool IsInBackpack(ItemBehaviour behaviour)
        {
            if (CarryMore)
            {
                return behaviour.transform.position.y < -900;
            }
            else if (AdvancedBackpack)
            {
                return Vector3.Distance(behaviour.transform.position, AdvancedBackpackPosition) < AdvancedBackpackDistance;
            }

            return false;
        }
    }
}
