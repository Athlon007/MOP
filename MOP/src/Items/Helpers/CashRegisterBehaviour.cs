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

using MSCLoader;
using System.Collections;
using System.Linq;
using UnityEngine;

using MOP.Common;
using MOP.Managers;

namespace MOP.Items.Helpers
{
    class CashRegisterBehaviour : MonoBehaviour
    {
        IEnumerator currentRoutine;

        public CashRegisterBehaviour()
        {
            FsmHook.FsmInject(this.gameObject, "Purchase", HookItems);
        }

        /// <summary>
        /// Used by Items.InitialieList().
        /// </summary>
        public void InitializeList()
        {
            currentRoutine = PurchaseCoroutine(true);
            StartCoroutine(currentRoutine);
        }

        /// <summary>
        /// Starts the PurchaseCoroutine
        /// </summary>
        public void HookItems()
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            currentRoutine = PurchaseCoroutine(false);
            StartCoroutine(currentRoutine);
        }

        #region Main Job
        /// <summary>
        /// Injects the newly bought store items.
        /// </summary>
        /// <param name="onLoad">If true, all waiting is skipeed.</param>
        /// <returns></returns>
        IEnumerator PurchaseCoroutine(bool onLoad)
        {
            // Wait for few seconds to let all objects to spawn, and then inject the objects.
            if (!onLoad)
                yield return new WaitForSeconds(2);

            // Find shopping bags in the list
            GameObject[] items = FindObjectsOfType<GameObject>()
                                .Where(gm => gm.name.ContainsAny(ItemsManager.Instance.NameList) && gm.name.ContainsAny("(itemx)", "(Clone)") & gm.GetComponent<ItemBehaviour>() == null)
                                .ToArray();

            if (items.Length > 0)
            {
                long half = items.Length >> 1;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == null) continue;

                    // Skip frame
                    if (i == half && !onLoad)
                        yield return null;

                    items[i].AddComponent<ItemBehaviour>();

                    // Hook the HookItems void to Confirm and Spawn all actions
                    if (items[i].name.Equals("shopping bag(itemx)"))
                    {
                        FsmHook.FsmInject(items[i], "Confirm", HookItems);
                        FsmHook.FsmInject(items[i], "Spawn all", HookItems);
                    }
                    else if (items[i].name.EqualsAny("spark plug box(Clone)", "car light bulb box(Clone)"))
                    {
                        FsmHook.FsmInject(items[i], "Create Plug", WipeUseLoadOnSparkPlugs);
                    }
                    else if (items[i].name.EqualsAny("alternator belt(Clone)", "oil filter(Clone)", "battery(Clone)"))
                    {
                        items[i].GetPlayMakerByName("Use").Fsm.RestartOnEnable = false;
                    }
                }
                WipeUseLoadOnSparkPlugs(items.Where(g => g.name.EqualsAny("spark plug(Clone)", "light bulb(Clone)")).ToArray());
            }
            currentRoutine = null;
        }
        #endregion
        #region Light Bulbs & Spark Plugs Hook
        public void WipeUseLoadOnSparkPlugs()
        {
            StartCoroutine(SparkPlugRoutine(null));
        }

        public void WipeUseLoadOnSparkPlugs(GameObject[] plugs = null)
        {
            StartCoroutine(SparkPlugRoutine(plugs));
        }

        IEnumerator SparkPlugRoutine(GameObject[] plugs)
        {
            yield return new WaitForSeconds(.5f);
            if (plugs == null)
                plugs = GameObject.FindGameObjectsWithTag("PART").Where(g => g.name.EqualsAny("spark plug(Clone)", "light bulb(Clone)")).ToArray();

            for (int i = 0; i < plugs.Length; i++)
            {
                plugs[i].GetPlayMakerByName("Use").Fsm.RestartOnEnable = false;

                if (plugs[i].GetComponent<ItemBehaviour>() == null)
                    plugs[i].AddComponent<ItemBehaviour>();
            }
        }
        #endregion
        #region Amis-Auto Packages
        IEnumerator packagesRoutine; 

        public void Packages()
        {
            if (packagesRoutine != null)
            {
                StopCoroutine(packagesRoutine);
            }

            packagesRoutine = PackagesCoroutine();
            StartCoroutine(packagesRoutine);
        }

        IEnumerator PackagesCoroutine()
        {
            yield return new WaitForSeconds(2);
            GameObject[] packages = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(g => g.name == "amis-auto ky package(xxxxx)" && g.activeSelf).ToArray();

            foreach (GameObject package in packages)
            {
                FsmHook.FsmInject(package, "State 1", HookItems);
            }
        }
        #endregion
        #region Fish Trap
        IEnumerator fishesRoutine;

        public void Fishes()
        {
            if (fishesRoutine != null)
            {
                StopCoroutine(fishesRoutine);
            }

            fishesRoutine = FishesCoroutine();
            StartCoroutine(fishesRoutine);
        }

        IEnumerator FishesCoroutine()
        {
            yield return new WaitForSeconds(2);
            GameObject[] fishes = Resources.FindObjectsOfTypeAll<GameObject>()
                                  .Where(g => g.name == "pike(itemx)" && g.activeSelf).ToArray();

            foreach (GameObject fish in fishes)
            {
                if (fish.GetComponent<ItemBehaviour>() == null)
                {
                    fish.AddComponent<ItemBehaviour>();
                }
            }
        }
        #endregion
    }
}
