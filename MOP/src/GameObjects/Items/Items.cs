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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOP
{
    class Items
    {
        public static Items instance;

        public readonly string[] BlackList = 
        {
            "airfilter", "alternator", "alternator belt", "amplifier", "antenna", "ax",
            "back panel", "basketball", "battery", "beer case", "berry box",
            "block", "bootlid", "booze", "box", "brake fluid",
            "brake lining", "brake master cylinder", "bucket", "bucket lid", "bucket seat driver",
            "bucket seat passenger", "bumper front", "bumper rear", "camera", "camshaft",
            "camshaft gear", "car jack", "carburator", "cd player", "center console gt",
            "cigarettes", "clock gauge", "clutch cover plate", "clutch disc", "clutch lining",
            "clutch master cylinder", "clutch pressure plate", "coffee cup", "coffee pan", "coil spring",
            "coolant", "crankshaft", "crankshaft pulley", "cylinder head", "dash cover leopard",
            "dash cover plush", "dash cover suomi", "dash cover zebra", "dashboard", "dashboard meters",
            "diesel", "digging bar", "dipper", "disc brake", "diskette",
            "distributor", "door left", "door right", "drive gear", "drum brake",
            "electrics", "empty", "empty plastic can", "engine plate", "exhaust dual tip",
            "exhaust muffler", "exhaust pipe", "extra gauges", "fender flare fl", "fender flare fr",
            "fender flare rl", "fender flare rr", "fender flare spoiler", "fender flares", "fender left",
            "fender right", "fiberglass hood", "fire extinguisher", "fire extinguisher holder", "firewood",
            "fireworks bag", "fish trap", "flashlight", "floor jack", "flywheel",
            "football", "front spoiler", "fuel mixture gauge", "fuel pump", "fuel strainer",
            "fuel tank", "fuel tank pipe", "fur dices", "garbage barrel", "gasoline",
            "gear linkage", "gear stick", "gearbox", "grill", "grill charcoal",
            "grille", "grille gt", "grilled pike", "ground coffee", "gt steering wheel",
            "halfshaft", "handbrake", "head gasket", "headers", "headlight left",
            "headlight right", "helmet", "hood", "hubcap", "inspection cover",
            "juice", "lantern", "light bulb",
            "log", "long coil spring", "main bearing1", "main bearing2", "main bearing3",
            "marker light left", "marker light right", "marker lights", "mosquito spray", "motor hoist",
            "motor oil", "mudflap fl", "mudflap fr", "mudflap rl", "mudflap rr",
            "n2o bottle", "n2o bottle holder", "n2o button panel", "n2o injectors", "n2o kit",
            "notepad", "oil filter", "oilpan", "parts magazine", "piston1",
            "piston2", "piston3", "piston4", "racing carburators", "racing exhaust",
            "racing flywheel", "racing harness", "racing muffler", "racing radiator", "radar buster",
            "radiator", "radiator hose1", "radiator hose2", "radiator hose3", "radio",
            "rally coil spring", "rally shock absorber", "rally steering wheel", "rally strut fl", "rally strut fr",
            "rally suspension kit", "ratchet set", "rear light left", "rear light right", "rear spoiler",
            "rear spoiler2", "register plate", "rocker cover", "rocker cover gt", "rocker shaft",
            "rpm gauge", "ruler", "sausages", "screwdriver", "seat cover leopard",
            "seat cover plush", "seat cover suomi", "seat cover zebra", "seat driver", "seat passenger",
            "seat rear", "shock absorber", "shopping bag", "side skirt left", "side skirt right",
            "sledgehammer", "sofa", "spanner set",
            "spark plug", "spark plug box", "sparkplug socket", "sparkplug wrench", "spindle fl",
            "spindle fr", "spirit", "sport steering wheel", "spray can", "starter",
            "steel headers", "steering column", "steering rack", "steering rod fl", "steering rod fr",
            "stock steering wheel", "strut fl", "strut fr", "sub frame", "subwoofer left",
            "subwoofer panel", "subwoofer right", "subwoofers", "sugar", "suitcase",
            "table", "tachometer", "teimo advert pile", "timing chain", "timing cover",
            "trail arm rl", "trail arm rr", "turbo", "tv remote control", "twin carburators",
            "two stroke fuel", "warning triangle", "water bucket", "water pump", "water pump pulley",
            "wheel cover leopard", "wheel cover plush", "wheel cover suomi", "wheel cover zebra", "wheelset hayosiko",
            "wheelset octo", "wheelset racing", "wheelset rally", "wheelset slot", "wheelset spoke",
            "wheelset steelwide", "wheelset turbine", "window grille", "windows black wrap", "wiring mess",
            "wishbone fl", "wishbone fr", "wood carrier", "xmas lights", "yeast", "pike", "macaron box", "milk", "potato chips",
            "pizza", "kilju"
        };


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

            // Car parts order bill hook.
            GameObject postOrder = GameObject.Find("STORE").transform.Find("LOD/ActivateStore/PostOffice/PostOrderBuy").gameObject;
            EnvelopeOrderBuyHook h = postOrder.AddComponent<EnvelopeOrderBuyHook>();
            h.Initialize(cashRegisterHook);

            // Fish trap spawner.
            FsmHook.FsmInject(GameObject.Find("fish trap(itemx)").transform.Find("Spawn").gameObject, "Create product", cashRegisterHook.Fishes);

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
                .Where(gm => gm.name.ContainsAny(BlackList) && gm.name.ContainsAny("(itemx)", "(Clone)") && gm.activeSelf).ToArray();

            for (int i = 0; i < items.Length; i++)
            {
                try
                {
                    items[i].AddComponent<ItemHook>();

                    // Hook the TriggerMinorObjectRefresh to Confirm and Spawn all actions
                    if (items[i].name.Equals("shopping bag(itemx)"))
                    {
                        FsmHook.FsmInject(items[i], "Confirm", cashRegisterHook.TriggerMinorObjectRefresh);
                        FsmHook.FsmInject(items[i], "Spawn all", cashRegisterHook.TriggerMinorObjectRefresh);
                    }
                    else if (items[i].name.EqualsAny("spark plug box(Clone)", "car light bulb box(Clone)"))
                    {
                        FsmHook.FsmInject(items[i], "Create Plug", cashRegisterHook.WipeUseLoadOnSparkPlugs);
                    }
                    else if (items[i].name.EqualsAny("alternator belt(Clone)", "oil filter(Clone)", "battery(Clone)"))
                    {
                        PlayMakerFSM fanbeltUse = items[i].GetPlayMakerByName("Use");
                        FsmState loadFanbelt = fanbeltUse.FindFsmState("Load");
                        List<FsmStateAction> emptyActions = new List<FsmStateAction> { new CustomNullState() };
                        loadFanbelt.Actions = emptyActions.ToArray();
                        loadFanbelt.SaveActions();
                    }

                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, "ITEM_LIST_LOAD_ERROR");
                }
            }
            cashRegisterHook.WipeUseLoadOnSparkPlugs();

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
            else
            {
                GameObject[] cdItems = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name.ContainsAny("cd case(item", "cd(item")).ToArray();
                foreach (GameObject cd in cdItems)
                    cd.AddComponent<ItemHook>();
            }

            // Get items from ITEMS object.
            GameObject itemsObject = GameObject.Find("ITEMS");
            for (int i = 0; i < itemsObject.transform.childCount; i++)
            {
                GameObject item = itemsObject.transform.GetChild(i).gameObject;
                if (item.name == "CDs") continue;

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

            // Also disable the restart for that sunnuva bitch.
            itemsObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            // Fucking wheels.
            GameObject[] wheels = Object.FindObjectsOfType<GameObject>().
                Where(gm => gm.name.EqualsAny("wheel_regula", "wheel_offset") && gm.activeSelf).ToArray();
            foreach (GameObject wheel in wheels)
                wheel.AddComponent<ItemHook>();

            // Tires trigger at Fleetari's.
            GameObject wheelTrigger = new GameObject("MOP_WheelTrigger");
            wheelTrigger.transform.localPosition = new Vector3(1555.49f, 4.8f, 737);
            wheelTrigger.transform.localEulerAngles = new Vector3(1.16f, 335, 1.16f);
            wheelTrigger.AddComponent<WheelRepairJobTrigger>();

            // Hook up the envelope.
            GameObject[] envelopes = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name.EqualsAny("envelope(xxxxx)", "lottery ticket(xxxxx)")).ToArray();
            foreach (var g in envelopes)
                g.AddComponent<ItemHook>();

            // Unparent all childs of CDs object.
            Transform cds = GameObject.Find("ITEMS").transform.Find("CDs");
            if (cds != null)
            {
                for (int i = 0; i < cds.childCount; i++)
                    cds.GetChild(i).parent = null;
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
                Object.Destroy(beerBottles[i]);
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
