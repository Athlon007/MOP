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

using System.Collections.Generic;
using UnityEngine;

namespace MOP
{
    class Boat : Vehicle
    {
        // This class is used for boat only.
        // Even tho boat is not considered as UnityCar vehicle, 
        // we use vehicle toggling method, 
        // in order to eliminate boat teleporting back to it's on load spawn point.

        public Boat(string gameObject) : base(gameObject)
        {
            Toggle = ToggleActive;

            preventToggleOnObjects.Add(new PreventToggleOnObject(transform.Find("GFX/Motor/Pivot/FuelTank")));
            rb = this.gameObject.GetComponent<Rigidbody>();

            // Ignore Rule
            IgnoreRule vehicleRule = Rules.instance.IgnoreRules.Find(v => v.ObjectName == this.gameObject.name);
            if (vehicleRule != null)
            {
                if (vehicleRule.TotalIgnore)
                {
                    IsActive = false;
                    return;
                }

                Toggle = ToggleBoatPhysics;
            }
        }

        /// <summary>
        /// Enable or disable car
        /// </summary>
        new void ToggleActive(bool enabled)
        {
            if (gameObject == null || gameObject.activeSelf == enabled || !IsActive) return;

            // If we're disabling a car, set the audio child parent to TemporaryAudioParent, and save the position and rotation.
            // We're doing that BEFORE we disable the object.
            if (!enabled)
            {
                for (int i = 0; i < preventToggleOnObjects.Count; i++)
                    preventToggleOnObjects[i].ObjectTransform.parent = temporaryParent;

                Position = gameObject.transform.localPosition;
                Rotation = gameObject.transform.localRotation;
            }

            gameObject.SetActive(enabled);

            // Uppon enabling the file, set the localPosition and localRotation to the object's transform, and change audio source parents to Object
            // We're doing that AFTER we enable the object.
            if (enabled)
            {
                gameObject.transform.localPosition = Position;
                gameObject.transform.localRotation = Rotation;

                for (int i = 0; i < preventToggleOnObjects.Count; i++)
                    preventToggleOnObjects[i].ObjectTransform.parent = preventToggleOnObjects[i].OriginalParent;
            }
        }

        void ToggleBoatPhysics(bool enabled)
        {
            if ((gameObject == null) || (rb.detectCollisions == enabled) || !IsActive)
                return;

            rb.detectCollisions = enabled;
            rb.isKinematic = !enabled;
        }
    }
}
