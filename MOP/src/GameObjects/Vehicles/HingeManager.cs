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

using UnityEngine;

namespace MOP
{
    class HingeManager : MonoBehaviour
    {
        // This script is responsible for resetting the door hinges, when vehiclee gets despawned.
        // Script from https://answers.unity.com/questions/368249/hingejoint-stops-working-properly-after-disableena.html

        private Quaternion initialLocalRotation;
        private Vector3 initialLocalPosition;

        private Quaternion localRotationOnDisable;
        private Vector3 localPositionOnDisable;

        private bool hasDisabled;

        void Awake()
        {
            this.initialLocalRotation = this.transform.localRotation;
            this.initialLocalPosition = this.transform.localPosition;
        }

        void OnDisable()
        {
            this.localRotationOnDisable = this.transform.localRotation;
            this.transform.localRotation = this.initialLocalRotation;

            this.localPositionOnDisable = this.transform.localPosition;
            this.transform.localPosition = this.initialLocalPosition;

            this.hasDisabled = true;
        }

        void Update()
        {
            if (this.hasDisabled)
            {
                this.hasDisabled = false;
                this.transform.localRotation = this.localRotationOnDisable;
                this.transform.localPosition = this.localPositionOnDisable;
            }
        }
    }
}
