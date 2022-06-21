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

using MOP.Common.Enumerations;
using MOP.FSM;
using MOP.Vehicles.Managers;

namespace MOP.Vehicles.Cases
{
    class Gifu : Vehicle
    {
        public Gifu(string gameObjectName) : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Gifu;

            Transform knobs = gameObject.transform.Find("Dashboard/Knobs");
            foreach (PlayMakerFSM knobsFSMs in knobs.GetComponentsInChildren<PlayMakerFSM>())
                knobsFSMs.Fsm.RestartOnEnable = false;

            // Fix resetting of shit tank.
            gameObject.transform.Find("ShitTank").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            // Odometer fix.
            transform.Find("Dashboard/Odometer").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            // Air pressure fix.
            transform.Find("Simulation/Airbrakes").GetPlayMaker("Air Pressure").Fsm.RestartOnEnable = false;

            // Hand throttle.
            gameObject.AddComponent<GifuHandThrottle>();
        }
    }
}
