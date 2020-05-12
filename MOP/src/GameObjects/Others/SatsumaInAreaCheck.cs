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
    class SatsumaInAreaCheck : MonoBehaviour
    {
        // Attaches to gameobject. This script checks if the Satsuma is in the object's area,
        // and sends the info to it about it.

        const string ReferenceItem = "gearbox";
        
        // Trigger collider
        BoxCollider collider;

        public void Initialize(Vector3 size)
        {
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
            collider.transform.position = transform.position;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.StartsWith(ReferenceItem))
            {
                Satsuma.instance.IsSatsumaInInspectionArea = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.StartsWith(ReferenceItem))
            {
                Satsuma.instance.IsSatsumaInInspectionArea = false;
            }
        }
    }

    class SatsumaInGarage : MonoBehaviour
    {
        // Attaches to gameobject. This script checks if the Satsuma is in the object's area,
        // and sends the info to it about it.

        public static SatsumaInGarage Instance;

        const string ReferenceItem = "car body";
        public static bool IsSatsumaInGarage;

        readonly Transform doorLeft;
        readonly Transform doorRight;

        public SatsumaInGarage()
        {
            Instance = this;

            doorLeft = GameObject.Find("GarageDoors").transform.Find("DoorLeft");
            doorRight = GameObject.Find("GarageDoors").transform.Find("DoorRight");
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.StartsWith(ReferenceItem))
            {
                IsSatsumaInGarage = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.StartsWith(ReferenceItem))
            {
                IsSatsumaInGarage = false;
            }
        }

        public bool AreGarageDoorsClose()
        {
            return doorLeft.localEulerAngles.z < 12 && (doorRight.localEulerAngles.z > 340 || doorRight.localEulerAngles.z < 10);
        }
    }
}
