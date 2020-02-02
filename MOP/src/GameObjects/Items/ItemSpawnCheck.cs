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
    class ItemSpawnCheck : MonoBehaviour
    {
        // Used for items bought via Item Shop mod

        public ItemSpawnCheck()
        {
            BoxCollider col = this.gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2, 2, 2);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.ContainsAny("cd case(itemy)", "CD Rack(itemy)", "cd(itemy)") && other.gameObject.GetComponent<ItemHook>() == null)
            {
                MSCLoader.ModConsole.Print(other.gameObject.name);
                other.gameObject.AddComponent<ItemHook>();

                GameObject obj = other.gameObject;

                // If the obj is cd case, find cd inside of it and attach ItemHook to it.
                if (obj.name == "cd case(itemy)")
                    foreach (var t in obj.GetComponentsInChildren<Transform>(true))
                        if (t.gameObject.name == "cd(itemy)")
                            t.gameObject.AddComponent<ItemHook>();
            }
        }
    }
}
