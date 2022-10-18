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
using MOP.Rules;
using MOP.Rules.Types;

namespace MOP.Vehicles.Cases
{
    class Boat : Vehicle
    {
        // This class is used for boat only.
        // Even tho boat is not considered as UnityCar vehicle, 
        // we use vehicle toggling method, 
        // in order to eliminate boat teleporting back to it's on load spawn point.

        Transform collidersParent;

        public Boat(string gameObject) : base(gameObject)
        {
            vehicleType = VehiclesTypes.Boat;
            Toggle = ToggleActive;

            transform.Find("GFX/Motor/Pivot/FuelTank").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            rb = this.gameObject.GetComponent<Rigidbody>();

            // Ignore Rule
            IgnoreRule vehicleRule = RulesManager.Instance.GetList<IgnoreRule>().Find(v => v.ObjectName == this.gameObject.name);
            if (vehicleRule != null)
            {
                if (vehicleRule.TotalIgnore)
                {
                    IsActive = false;
                    return;
                }

                Toggle = ToggleBoatPhysics;
            }

            this.dummyCar = new LOD.LodObject(this.gameObject);
        }

        /// <summary>
        /// Enable or disable car
        /// </summary>
        internal override void ToggleActive(bool enabled)
        {
            if (gameObject == null || gameObject.activeSelf == enabled || !IsActive) return;

            // If we're disabling a car, set the audio child parent to TemporaryAudioParent, and save the position and rotation.
            // We're doing that BEFORE we disable the object.
            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);

                colliders.parent = temporaryParent;
                dummyCar?.ToggleActive(true, transform);
            }

            gameObject.SetActive(enabled);

            // Uppon enabling the file, set the localPosition and localRotation to the object's transform, and change audio source parents to Object
            // We're doing that AFTER we enable the object.
            if (enabled)
            {
                MoveNonDisableableObjects(null);

                colliders.parent = collidersParent;
                colliders.localPosition = colliderPosition;
            }
        }

        /// <summary>
        /// Toggles boat's physics.
        /// </summary>
        /// <param name="enabled"></param>
        private void ToggleBoatPhysics(bool enabled)
        {
            if ((gameObject == null) || (rb.detectCollisions == enabled) || !IsActive)
                return;

            rb.detectCollisions = enabled;
            rb.isKinematic = !enabled;
        }

        /// <summary>
        /// Boat does not have UnityCar component. Therefore, we don't disable it.
        /// </summary>
        /// <param name="enabled"></param>
        public override void ToggleUnityCar(bool enabled)
        {
            // Method intentionally left empty.
        }

        public override void ForceToggleUnityCar(bool enabled)
        {
            // Method intentionally left empty.
        }

        protected override void LoadCarElements()
        {
            // Method intentionally left empty.
        }

        protected override void LoadColliders()
        {
            colliders = transform.Find("GFX/Colliders");
            colliderPosition = colliders.localPosition;
            collidersParent = colliders.parent;
        }
    }
}
