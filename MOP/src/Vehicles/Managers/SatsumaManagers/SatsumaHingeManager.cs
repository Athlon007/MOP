﻿// Modern Optimization Plugin
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

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaHingeManager : MonoBehaviour
    {
        private Quaternion initialLocalRotation;
        private Vector3 initialLocalPosition;

        private Quaternion localRotationOnDisable;
        private Vector3 localPositionOnDisable;

        private bool hasDisabled;

        bool isAssembled;

        void Awake()
        {
            GetDefaultPosition();
            isAssembled = transform.parent != null;
        }

        void GetDefaultPosition()
        {
            this.initialLocalRotation = this.transform.localRotation;
            this.initialLocalPosition = this.transform.localPosition;
        }

        void OnDisable()
        {
            if (!isAssembled) return;

            this.localRotationOnDisable = this.transform.localRotation;
            this.transform.localRotation = this.initialLocalRotation;

            this.localPositionOnDisable = this.transform.localPosition;
            this.transform.localPosition = this.initialLocalPosition;

            this.hasDisabled = true;
        }

        void Update()
        {
            if (transform.parent == null && isAssembled)
                isAssembled = false;

            if (transform.parent != null && transform.parent.gameObject.name != "ItemPivot" && !isAssembled)
            {
                GetDefaultPosition();
                isAssembled = true;
            }

            if (isAssembled && this.hasDisabled)
            {
                this.hasDisabled = false;
                this.transform.localRotation = this.localRotationOnDisable;
                this.transform.localPosition = this.localPositionOnDisable;
            }
        }
    }
}
