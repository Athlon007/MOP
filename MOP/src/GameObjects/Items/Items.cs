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

using MSCLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOP
{
    class Items
    {
        public static Items instance;

        // List of ObjectHooks attached to minor objects
        public List<ItemHook> ItemsHooks = new List<ItemHook>();

        readonly CashRegisterHook cashRegisterHook;

        /// <summary>
        /// Initialize MinorObjects class
        /// </summary>
        public Items()
        {
            instance = this;
            cashRegisterHook = GameObject.Find("STORE/StoreCashRegister/Register").AddComponent<CashRegisterHook>();
            InitializeList();

            // Uncle's beer case bottle despawner
            Transform uncleBeerCaseTransform = GameObject.Find("YARD").transform.Find("UNCLE/Home/UncleDrinking/Uncle/beer case(itemx)");
            if (uncleBeerCaseTransform == null)
            {
                ModConsole.Print("[MOP] Couldn't find uncle's beer case, so it will be skipped...");
                return;
            }

            uncleBeerCaseTransform.gameObject.AddComponent<UncleBeerCaseHook>();
        }

        /// <summary>
        /// Add object hook to the list
        /// </summary>
        /// <param name="newHook"></param>
        public void Add(ItemHook newHook)
        {
            ItemsHooks.Add(newHook);
        }

        /// <summary>
        /// Remove object hook from the list
        /// </summary>
        /// <param name="objectHook"></param>
        public void Remove(ItemHook objectHook)
        {
            if (ItemsHooks.Contains(objectHook))
            {
                ItemsHooks.Remove(objectHook);
            }
        }

        /// <summary>
        /// Lists all the game objects in the game's world and that are on the whitelist,
        /// then it adds ObjectHook to them
        /// </summary>
        void InitializeList()
        {
            // Get all minor objects from the game world (like beer cases, sausages)
            // Only items that are in the listOfMinorObjects list, and also contain "(itemx)" in their name will be loaded
            GameObject[] items = Object.FindObjectsOfType<GameObject>()
                .Where(gm => gm.name.ContainsAny("(itemx)", "(Clone)") && gm.activeSelf).ToArray();

            for (int i = 0; i < items.Length; i++)
            {
                try
                {
                    items[i].AddComponent<ItemHook>();

                    // Hook the TriggerMinorObjectRefresh to Confirm and Spawn all actions
                    if (items[i].name.Contains("shopping bag"))
                    {
                        FsmHook.FsmInject(items[i], "Confirm", cashRegisterHook.TriggerMinorObjectRefresh);
                        FsmHook.FsmInject(items[i], "Spawn all", cashRegisterHook.TriggerMinorObjectRefresh);
                    }

                    if (items[i].name.EqualsAny("spark plug box(Clone)", "car light bulb box(Clone)"))
                    {
                        FsmHook.FsmInject(items[i], "Create Plug", cashRegisterHook.WipeUseLoadOnSparkPlugs);
                    }
                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, "ITEM_LIST_LOAD_ERROR");
                }
            }

            // CD Player Enhanced compatibility
            if (ModLoader.IsModPresent("CDPlayer"))
            {
                GameObject[] cdEnchancedObjects = Object.FindObjectsOfType<GameObject>()
                .Where(gm => gm.name.ContainsAny("cd case(itemy)", "CD Rack(itemy)", "cd(itemy)") && gm.activeSelf).ToArray();

                for (int i = 0; i < cdEnchancedObjects.Length; i++)
                {
                    cdEnchancedObjects[i].AddComponent<ItemHook>();
                }

                // Create shop table check, for when the CDs are bought
                GameObject itemCheck = new GameObject("MOP_ItemAreaCheck");
                itemCheck.transform.position = GameObject.Find("SpawnItemStore").transform.position;
                itemCheck.AddComponent<ShopModItemSpawnCheck>();
            }

            // Get items from ITEMS object.
            GameObject itemsObject = GameObject.Find("ITEMS");
            for (int i = 0; i < itemsObject.transform.childCount; i++)
            {
                GameObject item = itemsObject.transform.GetChild(i).gameObject;

                try
                {
                    if (item.GetComponent<ItemHook>() != null)
                        continue;

                    item.AddComponent<ItemHook>();
                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, "ITEM_LIST_AT_ITEMS_LOAD_ERROR");
                }
            }

            // Also disable the on laod for that sunnuva bitch.
            itemsObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            // Fucking wheels.
            GameObject[] wheels = Object.FindObjectsOfType<GameObject>().Where(gm => gm.name == "wheel_regula" && gm.activeSelf).ToArray();
            foreach (GameObject wheel in wheels)
                wheel.AddComponent<ItemHook>();
        }

        /// <summary>
        /// Finds active game objects named "empty bottle(Clone)" and destroys them.
        /// </summary>
        public void DestroyBeerBottles()
        {
            GameObject[] beerBottles = GameObject.FindGameObjectsWithTag("PART")
                .Where(gm => gm.activeSelf && gm.name == "empty bottle(Clone)").ToArray();

            for (int i = 0; i < beerBottles.Length; i++)
            {
                GameObject.Destroy(beerBottles[i]);
            }
        }

        /// <summary>
        /// Hooks new empty bottles created by drinking beer with ItemHook
        /// </summary>
        public void HookEmptyBeerBottles()
        {
            GameObject[] beerBottles = GameObject.FindGameObjectsWithTag("PART")
                .Where(gm => gm.activeSelf && gm.name == "empty bottle(Clone)").ToArray();

            for (int i = 0; i < beerBottles.Length; i++)
            {
                beerBottles[i].AddComponent<ItemHook>();
            }
        }
    }
}
