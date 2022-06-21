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

namespace MOP.Vehicles.Cases
{
    class Jonnez : Vehicle
    {
        public Jonnez(string gameObjectName) : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Jonnez;

            gameObject.transform.Find("Kickstand").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            // Disable on restart for wheels script.
            Transform wheelsParent = transform.Find("Wheels");
            foreach (Transform wheel in wheelsParent.GetComponentsInChildren<Transform>())
            {
                if (!wheel.gameObject.name.StartsWith("Moped_wheel")) continue;
                wheel.gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }

            // Tries to fix shaking of the Jonnez.
            gameObject.transform.Find("LOD/PlayerTrigger").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
        }

        /// <summary>
        /// WORKAROUND FOR JONNEZ:
        /// Because onGroundDown for Jonnez doesn't work the same way as for others, it will check if the Jonnnez's engine torque.
        /// </summary>
        /// <returns></returns>
        public override bool IsOnGround()
        {
            return drivetrain.torque == 0;
        }
    }
}
