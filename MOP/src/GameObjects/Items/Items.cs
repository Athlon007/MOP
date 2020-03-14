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

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MOP
{
    class Items
    {
        public static Items instance;

        // List of all whitelisted objects that can appear on the minorObjects list
        // Note: batteries aren't included
        public string[] blackList = { 
        "ax", "booze", "brake fluid", "cigarettes", "coffee pan", "coffee cup", "coolant", "diesel",
        "empty plastic can", "fire extinguisher", "gasoline", "grill", "grill charcoal", "ground coffee", 
        "juice", "kilju", "lamp", "macaronbox", "milk", "moosemeat", "mosquito spray", "motor oil", 
        "oilfilter", "pike", "pizza", "ratchet set", "potato chips", "sausages", "sugar", "spanner set", 
        "spray can", "two stroke fuel", "wiring mess", "wood carrier", "yeast", "shopping bag", "flashlight", 
        "beer case", "fireworks bag", "lantern", "dipper", "coffee pan", "fireworks bag", "camera", 
        "water bucket", "car jack", "warning triangle", "spirit", "diskette", "empty", "empty bottle" };

        public string[] whiteList = { "grille gt" };

        // List of ObjectHooks attached to minor objects
        public List<ItemHook> ItemsHooks = new List<ItemHook>();


        /// <summary>
        /// Initialize MinorObjects class
        /// </summary>
        public Items()
        {
            instance = this;
            InitializeList();
            GameObject.Find("STORE/StoreCashRegister/Register").AddComponent<CashRegisterHook>();

            // Uncle's beer case bottle despawner
            GameObject.Find("YARD/UNCLE/Home/UncleDrinking/Uncle/beer case(itemx)").AddComponent<UncleBeerCaseHook>();
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
                .Where(gm => gm.name.ContainsAny(blackList)
                && gm.name.ContainsAny("(itemx)", "(Clone)") && gm.activeSelf).ToArray();

            for (int i = 0; i < items.Length; i++)
            {
                items[i].AddComponent<ItemHook>();
            }

            // CD Player Enhanced compatibility
            if (CompatibilityManager.instance.CDPlayerEnhanced)
            {
                GameObject[] cdEnchancedObjects = Object.FindObjectsOfType<GameObject>()
                .Where(gm => gm.name.ContainsAny("cd case(itemy)", "CD Rack(itemy)", "cd(itemy)") && gm.activeSelf).ToArray();

                for (int i = 0; i < cdEnchancedObjects.Length; i++)
                {
                    cdEnchancedObjects[i].AddComponent<ItemHook>();
                }

                // Create shop table check, for when the CDs are bought
                GameObject itemCheck = new GameObject("MopItemAreaCheck");
                itemCheck.transform.position = GameObject.Find("SpawnItemStore").transform.position;
                itemCheck.AddComponent<ShopModItemSpawnCheck>();
            }

            // Actual Mop mod support
            if (CompatibilityManager.instance.ActualMop)
            {
                GameObject.Find("mop(Clone)").AddComponent<ItemHook>();
            }
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
