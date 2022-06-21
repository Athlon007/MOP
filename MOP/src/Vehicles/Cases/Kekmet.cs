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

using MOP.Managers;
using MOP.Common.Enumerations;
using MOP.FSM;
using MOP.Vehicles.Managers;

namespace MOP.Vehicles.Cases
{
    internal class Kekmet : Vehicle
    {
        Flatbed flatbed;

        public Kekmet(string gameObjectName) : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Kekmet;

            flatbed = VehicleManager.Instance.GetVehicle(VehiclesTypes.Flatbed) as Flatbed;

            transform.Find("Dashboard/HourMeter").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            // Hand Throttle.
            gameObject.AddComponent<KekmetHandThrottle>();

            // Trailer connection.
            transform.Find("Trailer/Hook").GetPlayMaker("Distance").Fsm.RestartOnEnable = false;
        }

        internal override void ToggleActive(bool enabled)
        {
            if (gameObject == null || gameObject.activeSelf == enabled || !IsActive) return;
            
            flatbed.IsAttached = FsmManager.IsTrailerAttached();

            // If we're disabling a car, set the audio child parent to TemporaryAudioParent, and save the position and rotation.
            // We're doing that BEFORE we disable the object.
            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);

                Position = transform.localPosition;
                Rotation = transform.localRotation;
            }

            gameObject.SetActive(enabled);

            if (flatbed.IsAttached)
            {
                flatbed.ToggleByKekmet(enabled);
            }

            // Uppon enabling the object, set the localPosition and localRotation to the object's transform, and change audio source parents to Object
            // We're doing that AFTER we enable the object.
            if (enabled)
            {
                MoveNonDisableableObjects(null);
            }
        }

        protected override void DisableHooksResetting()
        {
            // Hook HookFront and HookRear
            // Get hooks first
            Transform hookFront = transform.Find("Frontloader/ArmPivot/Arm/LoaderPivot/Loader/RopePoint/HookFront");
            Transform hookRear = transform.Find("HookRear");

            // If hooks exists, attach the RopeHookUp and RopeUnhook to appropriate states
            if (hookFront != null)
            {
                fsmHookFront = hookFront.GetComponent<PlayMakerFSM>();
                fsmHookFront.Fsm.RestartOnEnable = false;
            }

            if (hookRear != null)
            {
                fsmHookRear = hookRear.GetComponent<PlayMakerFSM>();
                fsmHookRear.Fsm.RestartOnEnable = false;
            }
        }
    }
}
