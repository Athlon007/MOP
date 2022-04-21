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

using MOP.Common;
using MOP.Common.Enumerations;
using MOP.Common.Interfaces;
using UnityEngine;

namespace MOP.WorldObjects
{
    internal class SkidmarkObject : GenericObject, IDisableUnderCondition
    {
        public SkidmarkObject(GameObject gameObject, int distance = 200, DisableOn disableOn = DisableOn.Distance) : base(gameObject, distance, disableOn)
        {
        }

        public bool IsConditionMet()
        {
            if (MopSettings.Mode == PerformanceMode.Performance)
            {
                return true;
            }

            if (MOP.AlwaysDisableSkidmarks.GetValue())
            {
                return true;
            }

            return false;
        }

        public override void Toggle(bool enabled)
        {
            this.gameObject.SetActive(!IsConditionMet());
        }
    }
}
