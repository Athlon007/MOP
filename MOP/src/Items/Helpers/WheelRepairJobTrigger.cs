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

using System.Collections;
using UnityEngine;

namespace MOP.Items.Helpers
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

        void SetBehaviour(GameObject g, bool enabled)
        {
            ItemBehaviour hook = g.gameObject.GetComponent<ItemBehaviour>();
            if (hook != null)
            {
                hook.DontDisable = enabled;
            }
        }

        void OnTriggerEnter(Collider other)
        { 
            if (other.gameObject.name.StartsWith("wheel_"))
            {
                SetBehaviour(other.gameObject, true);
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (isTriggerLoaded) return;

            if (other.gameObject.name.StartsWith("wheel_"))
            {
                SetBehaviour(other.gameObject, true);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.StartsWith("wheel_"))
            {
                SetBehaviour(other.gameObject, false);
            }
        }
    }
}
