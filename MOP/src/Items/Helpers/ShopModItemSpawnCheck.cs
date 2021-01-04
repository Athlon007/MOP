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
using MOP.Common;

namespace MOP.Items.Helpers
{
    class ShopModItemSpawnCheck : MonoBehaviour
    {
        // Used for items bought via Item Shop mod
        // Adds the trigger collider at the Teimo's shop table.

        readonly string[] items = 
        { 
            "cd case(itemy)", "CD Rack(itemy)", "cd(itemy)" 
        };

        public ShopModItemSpawnCheck()
        {
            gameObject.layer = 2;
            BoxCollider col = this.gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2, 2, 2);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.ContainsAny(items) && other.gameObject.GetComponent<ItemBehaviour>() == null)
            {
                other.gameObject.AddComponent<ItemBehaviour>();
                GameObject gm = other.gameObject;

                // If the object is cd case, find cd inside of it and attach ItemHook to it.
                if (gm.name == "cd case(itemy)")
                    foreach (var t in gm.GetComponentsInChildren<Transform>(true))
                        if (t.gameObject.name == "cd(itemy)")
                            t.gameObject.AddComponent<ItemBehaviour>();
            }
        }
    }
}
