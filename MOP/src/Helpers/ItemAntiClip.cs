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

namespace MOP.Helpers
{
    class ItemAntiClip : MonoBehaviour
    {
        // It attaches to a gameobject trigger under the cottage.
        // It's purpose is to catch items that fall under the cottage floor and teleport them Y+1 position.

        void Start()
        {
            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(5.35f, 5f, 5f);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "PART")
            {
                Vector3 pos = other.gameObject.transform.position + Vector3.up;
                other.gameObject.transform.position = pos;  
            }
        }
    }
}
