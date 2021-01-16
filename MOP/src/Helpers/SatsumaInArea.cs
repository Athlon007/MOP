// Modern Optimization Plugin
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

using MOP.Vehicles.Cases;

namespace MOP.Helpers
{
    class SatsumaInArea : MonoBehaviour
    {
        // Attaches to gameobject. This script checks if the Satsuma is in the object's area,
        // and sends the info to it about it.

        GameObject referenceObject;
        
        // Trigger collider
        BoxCollider collider;
        bool isParcFerme;

        public void Initialize(Vector3 size)
        {
            referenceObject = Satsuma.Instance.GetCarBody();
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
            collider.transform.position = transform.position;

            isParcFerme = this.gameObject.name == "MOP_ParcFermeTrigger";
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == referenceObject)
            {
                Satsuma.Instance.IsSatsumaInInspectionArea = true;
                if (isParcFerme)
                {
                    Satsuma.Instance.IsSatsumaInParcFerme = true;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject == referenceObject)
            {
                Satsuma.Instance.IsSatsumaInInspectionArea = false;
                if (isParcFerme)
                {
                    Satsuma.Instance.IsSatsumaInParcFerme = false;
                }
            }
        }
    }
}
