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
        // This method is only used in certain special cases.
        // Current correct method is to use rule files!

        public static CompatibilityManager instance;

        // CarryMore
        // https://www.racedepartment.com/downloads/carry-more-backpack-alternative.22396/
        public bool CarryMore { get; private set; }
        public readonly Vector3 CarryMoreTempPosition = new Vector3(0.0f, -1000.0f, 0.0f);

        public CompatibilityManager()
        {
            instance = this;
            CarryMore = ModLoader.IsModPresent("CarryMore");
        }
    }
}
