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

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaExhaustMufflerFix : MonoBehaviour
    {
        private GameObject triggerMufflers;
        private IEnumerator currentWaitTrigger;

        // Attached to MiscParts/pivot_exhaust_muffler.
        void Awake()
        {
            triggerMufflers = transform.root.Find("MiscParts/Triggers Mufflers").gameObject;

            if (transform.childCount > 0)
            {
                StartEnumerator();
            }
        }

        // https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnTransformChildrenChanged.html
        void OnTransformChildrenChanged()
        {
            StartEnumerator();
        }

        void StartEnumerator()
        {
            if (currentWaitTrigger == null)
            {
                currentWaitTrigger = WaitTriggerCheck();
                StartCoroutine(currentWaitTrigger);
            }
        }

        IEnumerator WaitTriggerCheck()
        {
            while (transform.childCount > 0)
            {
                yield return new WaitForSeconds(2);
            }

            yield return new WaitForSeconds(2);
            if (transform.childCount == 0)
            {
                triggerMufflers.SetActive(true);
            }

            currentWaitTrigger = null;
        }
    }
}
