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

using HutongGames.PlayMaker;
using MSCLoader;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOP
{
    class CashRegisterHook : MonoBehaviour
    {
        // This MonoBehaviour hooks to CashRegister GameObject
        // CashRegisterHook class by Konrad "Athlon" Figura

        IEnumerator currentRoutine;

        public CashRegisterHook()
        {
            FsmHook.FsmInject(this.gameObject, "Purchase", TriggerMinorObjectRefresh);
        }

        /// <summary>
        /// Starts the PurchaseCoroutine
        /// </summary>
        public void TriggerMinorObjectRefresh()
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            currentRoutine = PurchaseCoroutine();
            StartCoroutine(currentRoutine);
        }

        /// <summary>
        /// Injects the newly bought store items.
        /// </summary>
        /// <returns></returns>
        IEnumerator PurchaseCoroutine()
        {
            // Wait for few seconds to let all objects to spawn, and then inject the objects.
            yield return new WaitForSeconds(2);
            // Find shopping bags in the list
            GameObject[] items = FindObjectsOfType<GameObject>()
                .Where(gm => gm.name.ContainsAny("(itemx)", "(Clone)"))
                .ToArray();

            if (items.Length > 0)
            {
                int half = items.Length / 2;
                for (int i = 0; i < items.Length; i++)
                {
                    // Skip frame
                    if (i == half)
                        yield return null;

                    // Object already has ObjectHook attached? Ignore it.
                    if (items[i].GetComponent<ItemHook>() != null)
                        continue;

                    items[i].AddComponent<ItemHook>();

                    // Hook the TriggerMinorObjectRefresh to Confirm and Spawn all actions
                    if (items[i].name.Contains("shopping bag"))
                    {
                        FsmHook.FsmInject(items[i], "Confirm", TriggerMinorObjectRefresh);
                        FsmHook.FsmInject(items[i], "Spawn all", TriggerMinorObjectRefresh);
                    }

                    if (items[i].name.StartsWith("spark plug box"))
                    {
                        FsmHook.FsmInject(items[i], "Create Plug", WipeUseLoadOnSparkPlugs);
                    }

                    if (items[i].name.ContainsAny("alternator belt(Clone)", "oil filter(Clone)", "battery(Clone)"))
                    {
                        PlayMakerFSM fanbeltUse = items[i].GetPlayMakerByName("Use");
                        FsmState loadFanbelt = fanbeltUse.FindFsmState("Load");
                        List<FsmStateAction> emptyActions = new List<FsmStateAction> { new CustomNullState() };
                        loadFanbelt.Actions = emptyActions.ToArray();
                        loadFanbelt.SaveActions();
                    }
                }
                WipeUseLoadOnSparkPlugs();
            }
            currentRoutine = null;
        }

        public void WipeUseLoadOnSparkPlugs()
        {
            StartCoroutine(SparkPlugRoutine());
        }

        IEnumerator SparkPlugRoutine()
        {
            yield return new WaitForSeconds(.5f);
            GameObject[] plugs = GameObject.FindGameObjectsWithTag("PART").Where(g => g.name.EqualsAny("spark plug(Clone)", "light bulb(Clone)")).ToArray();
            for (int i = 0; i < plugs.Length; i++)
            {
                PlayMakerFSM fanbeltUse = plugs[i].GetPlayMakerByName("Use");
                FsmState loadFanbelt = fanbeltUse.FindFsmState("Load");
                List<FsmStateAction> emptyActions = new List<FsmStateAction> { new CustomNullState() };
                loadFanbelt.Actions = emptyActions.ToArray();
                loadFanbelt.SaveActions();

                FsmState state1 = fanbeltUse.FindFsmState("State 1");
                state1.Actions = emptyActions.ToArray();
                state1.SaveActions();

                if (plugs[i].GetComponent<ItemHook>() == null)
                    plugs[i].AddComponent<ItemHook>();
            }
        }
    }
}
