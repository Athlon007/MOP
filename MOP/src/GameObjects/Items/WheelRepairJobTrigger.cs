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

using System.Collections;
using UnityEngine;

namespace MOP
{
    class WheelRepairJobTrigger : MonoBehaviour
    {
        bool isTriggerLoaded;

        void Start()
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(6, 5, 8);

            StartCoroutine(InitializeRoutine());
        }

        IEnumerator InitializeRoutine()
        {
            yield return new WaitForSeconds(2);
            isTriggerLoaded = true;
        }

        void OnTriggerEnter(Collider other)
        { 
            if (other.gameObject.name.StartsWith("wheel_"))
            {
                GameObject wheel = other.gameObject;
                ItemHook hook = wheel.GetComponent<ItemHook>();
                if (hook != null)
                {
                    hook.DontDisable = true;
                }
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (isTriggerLoaded) return;

            if (other.gameObject.name.StartsWith("wheel_"))
            {
                GameObject wheel = other.gameObject;
                ItemHook hook = wheel.GetComponent<ItemHook>();
                if (hook != null)
                {
                    hook.DontDisable = true;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.StartsWith("wheel_"))
            {
                GameObject wheel = other.gameObject;
                ItemHook hook = wheel.GetComponent<ItemHook>();
                if (hook != null)
                {
                    hook.DontDisable = false;
                }
            }
        }
    }
}
