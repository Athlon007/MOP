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

using System.Collections;
using UnityEngine;

using MOP.Vehicles;
using MOP.Vehicles.Cases;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaHingeManager : MonoBehaviour
    {
        private Quaternion initialLocalRotation;
        private Vector3 initialLocalPosition;

        private Quaternion localRotationOnDisable;
        private Vector3 localPositionOnDisable;

        private bool isDisabeld, isAssembled;

        void Awake()
        {
            GetDefaultPosition();
            isAssembled = IsAssembledToTheCar();
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

            this.isDisabeld = true;
        }

        void OnEnable()
        {
            StartCoroutine(HingeFix());
        }

        void Update()
        {
            if (!IsAssembledToTheCar() && isAssembled)
                isAssembled = false;

            if (IsAssembledToTheCar() && transform.parent.gameObject.name != "ItemPivot" && !isAssembled)
            {
                GetDefaultPosition();
                isAssembled = true;
            }

            if (isAssembled && this.isDisabeld)
            {
                this.isDisabeld = false;
                this.transform.localRotation = this.localRotationOnDisable;
                this.transform.localPosition = this.localPositionOnDisable;
            }
        }

        bool IsAssembledToTheCar()
        {
            return transform.root != null && transform.root == Satsuma.Instance.transform;
        }

        IEnumerator HingeFix()
        {
            yield return new WaitForSeconds(1);
            FixedJoint[] fixedJoints = gameObject.GetComponents<FixedJoint>();
            HingeJoint[] hingeJoints = gameObject.GetComponents<HingeJoint>();

            while (fixedJoints.Length > 1)
                Destroy(fixedJoints[0]);

            while (hingeJoints.Length > 1)
                Destroy(hingeJoints[0]);
        }
    }
}