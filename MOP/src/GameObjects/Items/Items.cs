using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MOP
{
    class Items
    {
        // List of all whitelisted objects that can appear on the minorObjects list
        // Note: batteries aren't included
        public string[] blackList = { "ax", "booze", "brake fluid", "cigarettes", "coffee pan", "coffee cup", "coolant", "diesel",
        "empty plastic can", "fire extinguisher", "gasoline", "grill", "grill charcoal", "ground coffee", "juice", "kilju", "lamp", 
        "macaronbox", "milk", "moosemeat", "mosquito spray", "motor oil", "oilfilter", "pike", "pizza", "ratchet set", "potato chips", 
        "sausages", "sugar", "spanner set", "spray can", "two stroke fuel", "wiring mess", "wood carrier", "yeast", "shopping bag", 
        "flashlight", "beer case", "fireworks bag", "lantern", "dipper", "coffee pan", "fireworks bag", "camera", "water bucket", 
        "car jack", "warning triangle", "spirit" };

        public string[] whiteList = { "grille gt" };

        // List of ObjectHooks attached to minor objects
        public List<ItemHook> ItemsHooks = new List<ItemHook>();

        public static Items instance;

        /// <summary>
        /// Initialize MinorObjects class
        /// </summary>
        public Items()
        {
            instance = this;
            InitializeList();
            GameObject.Find("STORE/StoreCashRegister/Register").AddComponent<CashRegisterHook>();
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

            GameObject[] minorObjects = Object.FindObjectsOfType<GameObject>()
                .Where(gm => gm.name.ContainsAny(blackList)
                && gm.name.ContainsAny("(itemx)", "(Clone)") && gm.activeSelf).ToArray();

            for (int i = 0; i < minorObjects.Length; i++)
            {
                minorObjects[i].AddComponent<ItemHook>();
            }
        }

        /// <summary>
        /// Finds active game objects named "empty bottle(Clone)" and destroys them.
        /// </summary>
        public void DestroyBeerBottles()
        {
            GameObject[] beerBottles = Object.FindObjectsOfType<GameObject>()
                .Where(gm => gm.activeSelf && gm.name == "empty bottle(Clone)").ToArray();

            for (int i = 0; i < beerBottles.Length; i++)
            {
                GameObject.Destroy(beerBottles[i]);
            }
        }
    }
}
