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
using MOP.FSM;
using MOP.Vehicles.Managers;

namespace MOP.Vehicles.Cases
{
    internal class Flatbed : Vehicle
    {
        public Flatbed(string gameObjectName) : base(gameObjectName) 
        {
            transform.Find("Bed/LogTrigger").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            GameObject trailerLogUnderFloorCheck = new GameObject("MOP_TrailerLogUnderFloorFix");
            trailerLogUnderFloorCheck.transform.parent = gameObject.transform;
            trailerLogUnderFloorCheck.AddComponent<TrailerLogUnderFloor>();

            // Tractor connection.
            PlayMakerFSM detach = gameObject.GetPlayMaker("Detach");
            detach.Fsm.RestartOnEnable = false;
            Object.Destroy(detach.FsmVariables.GetFsmGameObject("DetachPivot").Value.gameObject.GetComponent<FixedJoint>());
        }

        internal override void ToggleActive(bool enabled)
        {
            if (IsAttached) return;
            if (gameObject == null || gameObject.activeSelf == enabled || !IsActive) return;

            // If we're disabling a car, set the audio child parent to TemporaryAudioParent, and save the position and rotation.
            // We're doing that BEFORE we disable the object.
            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);

                Position = transform.localPosition;
                Rotation = transform.localRotation;
            }

            gameObject.SetActive(enabled);

            // Uppon enabling the object, set the localPosition and localRotation to the object's transform, and change audio source parents to Object
            // We're doing that AFTER we enable the object.
            if (enabled)
            {
                MoveNonDisableableObjects(null);
            }
        }

        public bool IsAttached { get; set; }

        public void ToggleByKekmet(bool enabled)
        {
            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);

                Position = transform.localPosition;
                Rotation = transform.localRotation;
            }

            gameObject.SetActive(enabled);

            // Uppon enabling the object, set the localPosition and localRotation to the object's transform, and change audio source parents to Object
            // We're doing that AFTER we enable the object.
            if (enabled)
            {
                MoveNonDisableableObjects(null);
            }
        }
    }
}
