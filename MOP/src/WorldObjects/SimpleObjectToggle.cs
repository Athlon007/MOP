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

using UnityEngine;

using MOP.Common;
using MOP.Common.Enumerations;

namespace MOP.WorldObjects
{
    class SimpleObjectToggle : GenericObject
    {
        public SimpleObjectToggle(GameObject gameObject, DisableOn disableOn = DisableOn.Distance, int distance = 200) : base(gameObject, distance, disableOn) { }

        public override void Toggle(bool enabled)
        {
            if (DisableOn.HasFlag(DisableOn.IgnoreInQualityMode) && MopSettings.Mode == PerformanceMode.Quality)
            {
                enabled = true;
            }

            if (DisableOn.HasFlag(DisableOn.IgnoreInBalancedAndAbove) && MopSettings.Mode >= PerformanceMode.Balanced)
            {
                enabled = true;
            }

            if (this.gameObject != null && this.gameObject.activeSelf != enabled)
            {
                this.gameObject.SetActive(enabled);
            }
        }
    }
}
