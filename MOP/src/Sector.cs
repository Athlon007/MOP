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

namespace MOP
{
    class Sector : MonoBehaviour
    {
        const string ReferenceObjectName = "MOP_PlayerCheck";
        string[] ignoreList;

        public void Initialize(Vector3 size, params string[] ignoreList)
        {
            // Set the layer to Ignore Raycast layer.
            gameObject.layer = 2;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;

            if (ignoreList != null)
                this.ignoreList = ignoreList;

            this.transform.parent = Hypervisor.Instance.gameObject.transform;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == ReferenceObjectName)
            {
                SectorManager.Instance.AddActiveSector(this);
                SectorManager.Instance.ToggleActive(false);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == ReferenceObjectName)
            {
                SectorManager.Instance.RemoveActiveSector(this);
                SectorManager.Instance.ToggleActive(true);
            }
        }

        public string[] GetIgnoreList()
        {
            return ignoreList;
        }
    }
}
