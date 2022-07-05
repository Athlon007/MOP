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
using System.Linq;
using UnityEngine;

using MOP.Managers;
using MOP.Common.Enumerations;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaTriggerFixer : MonoBehaviour
    {
        // Basically waits couple of seconds and checks if the pivot attached to the trigger is empty.
        // If yes, enable trigger.

        static SatsumaTriggerFixer instance;
        public static SatsumaTriggerFixer Instance => instance;

        void Awake()
        {
            instance = this;

            Transform body = VehicleManager.Instance.GetVehicle(VehiclesTypes.Satsuma).transform.Find("Body");
            Transform block = GameObject.Find("block(Clone)").transform;

            AddTriggerToChildren(body);
            AddTriggerToChildren(block);
        }

        void AddTriggerToChildren(Transform parent)
        {
            foreach (var t in parent.GetComponentsInChildren<Transform>()
                .Where(g => g.gameObject.name.ToLower().StartsWith("trigger_")))
            {
                if (t.GetComponent<PlayMakerFSM>() != null)
                {
                    t.gameObject.AddComponent<SatsumaTrigger>();
                }
            }
        }

        internal void Check(SatsumaTrigger trigger)
        { 
            if (trigger == null) return;
            try
            {
                StartCoroutine(CheckTriggerChild(trigger));
            }
            catch { }
        }

        IEnumerator CheckTriggerChild(SatsumaTrigger trigger)
        {
            yield return new WaitForSeconds(2);

            if (trigger.Pivot.childCount == 0)
            {
                trigger.gameObject.SetActive(true);
            }
        }
    }
}
