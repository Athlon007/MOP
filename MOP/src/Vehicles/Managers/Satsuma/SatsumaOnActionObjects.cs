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

using UnityEngine;

using MOP.Common.Enumerations;

namespace MOP.Vehicles.Managers.SatsumaManagers
{ 
    class SatsumaOnActionObjects
    {
        // Satsuma objects that are being disabled on specific situation.
        public GameObject GameObject;
        public PlayMakerFSM FSM;
        public SatsumaEnableOn EnableOn;

        public SatsumaOnActionObjects(GameObject GameObject, SatsumaEnableOn EnableOn)
        {
            this.GameObject = GameObject;
            this.EnableOn = EnableOn;
        }

        public SatsumaOnActionObjects(PlayMakerFSM FSM, SatsumaEnableOn EnableOn)
        {
            this.FSM = FSM;
            this.EnableOn = EnableOn;
        }
    }
}
