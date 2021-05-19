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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;
using MSCLoader;
using MSCLoader.Helper;

using MOP.Common;
using MOP.Items;
using MOP.Items.Cases;
using MOP.Items.Helpers;
using MOP.Helpers;
using MOP.FSM.Actions;

namespace MOP.Managers
{
    class ItemsManager
    {
        static ItemsManager instance;
        public static ItemsManager Instance { get => instance; }

        public ItemBehaviour this[int index] => itemHooks[index];

        public readonly string[] NameList =
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

        // List of ItemHooks that are a child of objects.
        List<ItemBehaviour> itemHooks = new List<ItemBehaviour>();

        CashRegisterBehaviour cashRegisterHook;
        Transform lostSpawner, landfillSpawn;

        // "Radiator hose3" stuff.
        PlayMakerFSM radiatorHose3Database;
        GameObject realRadiatorHose;

        /// <summary>
        /// Initialize Items class.
        /// </summary>
        public ItemsManager()
        {
            instance = this;
            lostSpawner = GameObject.Find("LostSpawner").transform;
            landfillSpawn = GameObject.Find("LANDFILL").transform.Find("LandfillSpawn");
        }

        internal void Initialize()
        {
            cashRegisterHook = GameObject.Find("STORE/StoreCashRegister/Register").AddComponent<CashRegisterBehaviour>();

            Transform spawner = GameObject.Find("Spawner").transform;

            InjectSpawnScripts(spawner.Find("CreateItems").gameObject);
            InjectSpawnScripts(spawner.Find("CreateSpraycans").gameObject);
            InjectSpawnScripts(spawner.Find("CreateShoppingbag").gameObject);
            InjectSpawnScripts(spawner.Find("CreateMooseMeat").gameObject);
            InjectSpawnScripts(GameObject.Find("fish trap(itemx)").transform.Find("Spawn").gameObject);

            GetCanTrigger();

            // Car parts order bill hook.
            try
            {
                GameObject storeLOD = GameObject.Find("STORE").transform.Find("LOD").gameObject;
                GameObject activateStore = storeLOD.transform.Find("ActivateStore").gameObject;
                bool lodLastState = storeLOD.activeSelf;
                bool activeStore = activateStore.activeSelf;

                if (!lodLastState)
                    storeLOD.SetActive(true);

                if (!activeStore)
                    activateStore.SetActive(true);

                GameObject postOrder = GameObject.Find("STORE").transform.Find("LOD/ActivateStore/PostOffice/PostOrderBuy").gameObject;
                bool isPostOrderActive = postOrder.activeSelf;
                postOrder.SetActive(true);
                FsmHook.FsmInject(postOrder, "State 3", cashRegisterHook.Packages);
                if (!isPostOrderActive)
                    postOrder.SetActive(false);

                storeLOD.SetActive(lodLastState);
                activateStore.SetActive(activateStore);
            }
            catch
            {
                throw new System.Exception("Couldn't inject PostOrderBuy object.");
            }

            InitializeList();

            // Hooks all bottles and adds adds BeerBottle script to them, which deletes the object and adds the ItemHook to them.
            Dictionary<string, string> names = new Dictionary<string, string>()
            {
                {
                    "BottleBeerFly",
                    "empty bottle(Clone)"
                },
                {
                    "BottleBoozeFly",
                    "empty bottle(Clone)"
                },
                {
                    "BottleSpiritFly",
                    "empty bottle(Clone)"
                },
                {
                    "CoffeeFly",
                    "empty cup(Clone)"
                },
                {
                    "MilkFly",
                    "empty pack(Clone)"
                },
                {
                    "VodkaShotFly",
                    "empty glass(Clone)"
                }
            };
            foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => names.ContainsKey(obj.name) && obj.GetComponent<PlayMakerFSM>() != null))
            {
                gameObject.AddComponent<ThrowableJunkBehaviour>();
            }

            // Hook the prefab of firewood.
            GameObject firewoodPrefab = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(f => f.name.EqualsAny("firewood") && f.GetComponent<PlayMakerFSM>() == null).gameObject;
            if (firewoodPrefab)
            {
                firewoodPrefab.AddComponent<ItemBehaviour>();
            }

            // Hook the prefab of log.
            GameObject logPrefab = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(f => f.name.EqualsAny("log") && f.GetComponent<PlayMakerFSM>() == null).gameObject;
            if (logPrefab)
            {
                logPrefab.AddComponent<ItemBehaviour>();
                logPrefab.transform.Find("log(Clone)").gameObject.AddComponent<ItemBehaviour>();
            }

            foreach (var f in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "log(Clone)"))
            {
                f.AddComponent<ItemBehaviour>();
            }

            radiatorHose3Database = GameObject.Find("Database").transform.Find("DatabaseMechanics/RadiatorHose3").GetComponent<PlayMakerFSM>();
            GameObject attachedHose = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "radiator hose3(xxxxx)");
            realRadiatorHose = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "radiator hose3(Clone)").gameObject;
            GameObject dummy = GameObject.Instantiate(realRadiatorHose); // used for spawning the radiator hose3 after it gets detached.

            UnityEngine.Object.Destroy(dummy.GetComponent<ItemBehaviour>());
            dummy.SetActive(false);
            dummy.name = dummy.name.Replace("(Clone)(Clone)", "(Clone)");

            Transform t = SaveManager.GetRadiatorHose3Transform();

            if (!attachedHose.activeSelf)
            {
                realRadiatorHose.transform.position = t.position;
                realRadiatorHose.transform.rotation = t.rotation;
                realRadiatorHose.SetActive(true);
            }
            radiatorHose3Database.FsmVariables.GameObjectVariables.First(g => g.Name == "SpawnThis").Value = realRadiatorHose;
        }

        /// <summary>
        /// Add object hook to the list
        /// </summary>
        /// <param name="newHook"></param>
        public void Add(ItemBehaviour newHook)
        {
            itemHooks.Add(newHook);
        }

        /// <summary>
        /// Remove object hook from the list
        /// </summary>
        /// <param name="objectHook"></param>
        public void Remove(ItemBehaviour objectHook)
        {
            if (itemHooks.Contains(objectHook))
            {
                itemHooks.Remove(objectHook);
            }
        }

        public void RemoveAt(int index)
        {
            if (itemHooks.Contains(itemHooks[index])) itemHooks.Remove(itemHooks[index]);
        }

        public int Count => itemHooks.Count;

        /// <summary>
        /// Lists all the game objects in the game's world and that are on the whitelist,
        /// then it adds ObjectHook to them
        /// </summary>
        void InitializeList()
        {
            cashRegisterHook.InitializeList();

            // CD Player Enhanced compatibility
            if (ModLoader.GetMod("CDPlayer") != null)
            {
                GameObject[] cdEnchancedObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(gm => gm.name.ContainsAny("cd case(itemy)", "CD Rack(itemy)", "cd(itemy)") && gm.activeSelf).ToArray();

                for (int i = 0; i < cdEnchancedObjects.Length; i++)
                {
                    cdEnchancedObjects[i].AddComponent<ItemBehaviour>();
                }

                // Create shop table check, for when the CDs are bought
                GameObject itemCheck = new GameObject("MOP_ItemAreaCheck");
                itemCheck.transform.localPosition = new Vector3(-1551.303f, 4.883998f, 1182.904f);
                itemCheck.transform.localEulerAngles = new Vector3(0f, 58.00043f, 0f);
                itemCheck.transform.localScale = new Vector3(1f, 1f, 1f);
                itemCheck.AddComponent<ShopModItemSpawnCheck>();
            }
            else
            {
                GameObject[] cdItems = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name.ContainsAny("cd case(item", "cd(item")).ToArray();
                foreach (GameObject cd in cdItems)
                    cd.AddComponent<ItemBehaviour>();
            }

            // Get items from ITEMS object.
            GameObject itemsObject = GameObject.Find("ITEMS");
            for (int i = 0; i < itemsObject.transform.childCount; i++)
            {
                GameObject item = itemsObject.transform.GetChild(i).gameObject;
                if (item.name == "CDs") continue;

                try
                {
                    if (item.GetComponent<ItemBehaviour>() != null)
                        continue;

                    item.AddComponent<ItemBehaviour>();
                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, false, "ITEM_LIST_AT_ITEMS_LOAD_ERROR");
                }
            }

            // Also disable the restart for that sunnuva bitch.
            itemsObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            // Fucking wheels.
            GameObject[] wheels = UnityEngine.Object.FindObjectsOfType<GameObject>().
                Where(gm => gm.name.EqualsAny("wheel_regula", "wheel_offset") && gm.activeSelf).ToArray();
            foreach (GameObject wheel in wheels)
                wheel.AddComponent<ItemBehaviour>();

            // Tires trigger at Fleetari's.
            GameObject wheelTrigger = new GameObject("MOP_WheelTrigger");
            wheelTrigger.transform.localPosition = new Vector3(1555.49f, 4.8f, 737);
            wheelTrigger.transform.localEulerAngles = new Vector3(1.16f, 335, 1.16f);
            wheelTrigger.AddComponent<WheelRepairJobTrigger>();

            // Hook up the envelope.
            GameObject[] envelopes = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name.EqualsAny("envelope(xxxxx)", "lottery ticket(xxxxx)")).ToArray();
            foreach (var g in envelopes)
                g.AddComponent<ItemBehaviour>();

            // Unparent all childs of CDs object.
            Transform cds = GameObject.Find("ITEMS").transform.Find("CDs");
            if (cds)
            {
                for (int i = 0; i < cds.childCount; i++)
                    cds.GetChild(i).parent = null;
            }
        }

        GameObject canTrigger;
        /// <summary>
        /// Returns the position of Jokke's kilju can trigger.
        /// </summary>
        public GameObject GetCanTrigger()
        {
            if (!canTrigger)
            {
                canTrigger = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "CanTrigger");
            }

            return canTrigger;
        }

        public Transform LostSpawner => lostSpawner;
        public Transform LandfillSpawn => landfillSpawn;

        internal void SetCurrentRadiatorHose(GameObject g)
        {
            realRadiatorHose = g;
        }

        internal GameObject GetRadiatorHose3()
        {
            return realRadiatorHose;
        }

        internal void OnSave()
        {
            // Fuck you ToplessGun.
            radiatorHose3Database.FsmVariables.GameObjectVariables.First(g => g.Name == "SpawnThis").Value = realRadiatorHose;
        }

        internal List<ItemBehaviour> All()
        {
            return itemHooks;
        }

        void InjectSpawnScripts(GameObject gm)
        {
            PlayMakerFSM[] createFSMs = gm.GetComponents<PlayMakerFSM>();
            for (int i = 0; i < createFSMs.Length; i++)
            {
                try
                {
                    PlayMakerFSM fsm = createFSMs[i];

                    FsmState state = fsm.GetState("Create");
                    if (state == null)
                    {
                        state = fsm.GetState("Create product");
                    }

                    if (state == null)
                    {
                        ModConsole.LogError($"FSM {i} has no Create or Create product");
                        continue;
                    }

                    FsmGameObject go = fsm.FsmVariables.GetFsmGameObject("New");

                    state.AddAction(new CustomAddItemBehaviour(go));
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "INJECT_SPAWN_SCRIPTS_ERROR");
                }
            }
        }
    }
}
