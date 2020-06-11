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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOP
{
    /// <summary>
    /// Controls all objects in the game world.
    /// </summary>
    class WorldManager : MonoBehaviour
    {
        public static WorldManager instance;

        readonly string[] vehicleArray =
        {
            "SATSUMA(557kg, 248)",
            "HAYOSIKO(1500kg, 250)",
            "JONNEZ ES(Clone)",
            "KEKMET(350-400psi)",
            "RCO_RUSCKO12(270)",
            "FERNDALE(1630kg)",
            "FLATBED",
            "GIFU(750/450psi)",
            "BOAT",
            "COMBINE(350-400psi)"
        };

        Transform player;
        Transform lostSpawner;

        List<Vehicle> vehicles;
        List<Place> places;
        WorldObjectList worldObjectList;

        bool isPlayerAtYard;
        bool inSectorMode;

        List<ItemHook> itemsToEnable = new List<ItemHook>();
        List<ItemHook> itemsToDisable = new List<ItemHook>();

        public WorldManager()
        {
            if (Rules.instance == null)
            {
                ModConsole.Error("[MOP] Rule Files haven't been loaded! Please exit to the main menu and start the game again.");
                return;
            }

            // Start the delayed initialization routine
            StartCoroutine(DelayedInitializaitonRoutine());
        }

        IEnumerator DelayedInitializaitonRoutine()
        {
            yield return new WaitForSeconds(2);

            // If the GT grille is attached, perform extra delay, until the "grille gt(Clone)" parent isn't "pivot_grille",
            // or until 5 seconds have passed.
            int counter = 0;
            GameObject gtGrille = GameObject.Find("grille gt(Clone)");
            if (gtGrille != null)
            {
                Transform gtGrilleTransform = gtGrille.transform;
                while (MopFsmManager.IsGTGrilleInstalled() && gtGrilleTransform.parent.name != "pivot_grille")
                {
                    yield return new WaitForSeconds(1);

                    // Escape the loop after 5 retries
                    counter++;
                    if (counter > 5)
                        break;
                }
            }

            Initialize();
        }

        void Initialize()
        {
            instance = this;

            ModConsole.Print("[MOP] Loading MOP...");

            // Initialize the worldObjectList list
            worldObjectList = new WorldObjectList();

            // Looking for player and yard
            player = GameObject.Find("PLAYER").transform;
            lostSpawner = GameObject.Find("LostSpawner").transform;

            // Add GameFixes MonoBehaviour.
            gameObject.AddComponent<GameFixes>();

            // Loading vehicles
            vehicles = new List<Vehicle>();
            foreach (string vehicle in vehicleArray)
            {
                try
                {
                    if (GameObject.Find(vehicle) == null) continue;

                    Vehicle newVehicle = null;
                    switch (vehicle)
                    {
                        case "SATSUMA(557kg, 248)":
                            newVehicle = new Satsuma(vehicle);
                            break;
                        case "BOAT":
                            newVehicle = new Boat(vehicle);
                            break;
                        case "COMBINE(350-400psi)":
                            newVehicle = new Combine(vehicle);
                            break;
                        default:
                            newVehicle = new Vehicle(vehicle);
                            break;
                    }

                    vehicles.Add(newVehicle);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, $"VEHICLE_LOAD_ERROR_{vehicle}");
                }
            }

            ModConsole.Print("[MOP] Vehicles loaded");

            // World Objects
            worldObjectList.Add("CABIN");
            worldObjectList.Add("COTTAGE", 400);
            worldObjectList.Add("DANCEHALL");
            worldObjectList.Add("PERAJARVI", 300);
            worldObjectList.Add("SOCCER");
            worldObjectList.Add("WATERFACILITY", 300);
            worldObjectList.Add("DRAGRACE", 1100);
            worldObjectList.Add("StrawberryField", 400);
            worldObjectList.Add("MAP/Buildings/DINGONBIISI", 400);
            worldObjectList.Add("RALLY/PartsSalesman", 400);

            ModConsole.Print("[MOP] Main world objects loaded");

            // Initialize places.
            places = new List<Place>
            {
                new Yard(),
                new Teimo(),
                new RepairShop(),
                new Inspection(),
                new Farm()
            };

            ModConsole.Print("[MOP] Initialized places");

            Transform buildings = GameObject.Find("Buildings").transform;

            // Find house of Teimo and detach it from Perajarvi, so it can be loaded and unloaded separately
            GameObject perajarvi = GameObject.Find("PERAJARVI");
            perajarvi.transform.Find("HouseRintama4").parent = buildings;
            // Same for chicken house.
            perajarvi.transform.Find("ChickenHouse").parent = buildings;

            // Chicken house (barn) close to player's house
            buildings.Find("ChickenHouse").parent = null;

            // Fix for church wall. Changing it's parent to NULL, so it will not be loaded or unloaded.
            // It used to be attached to CHURCH gameobject, 
            // but the Amis cars (yellow and grey cars) used to end up in the graveyard area.
            GameObject.Find("CHURCHWALL").transform.parent = null;

            // Fix for old house on the way from Perajarvi to Ventti's house (HouseOld5)
            perajarvi.transform.Find("HouseOld5").parent = buildings;

            // Fix for houses behind Teimo's
            perajarvi.transform.Find("HouseRintama3").parent = buildings;
            perajarvi.transform.Find("HouseSmall3").parent = buildings;

            // Perajarvi fixes for multiple objects with the same name.
            // Instead of being the part of Perajarvi, we're changing it to be the part of Buildings.
            Transform[] perajarviChilds = perajarvi.GetComponentsInChildren<Transform>();
            for (int i = 0; i < perajarviChilds.Length; i++)
            {
                // Fix for disappearing grain processing plant
                // https://my-summer-car.fandom.com/wiki/Grain_processing_plant
                if (perajarviChilds[i].gameObject.name.Contains("silo"))
                {
                    perajarviChilds[i].parent = buildings;
                    continue;
                }

                // Fix for Ventti's and Teimo's mailboxes (and pretty much all mailboxes that are inside of Perajarvi)
                if (perajarviChilds[i].gameObject.name == "MailBox")
                {
                    perajarviChilds[i].parent = buildings;
                    continue;
                }

                // Fix for greenhouses on the road from Perajarvi to Ventti's house
                if (perajarviChilds[i].name == "Greenhouse")
                {
                    perajarviChilds[i].parent = buildings;
                    continue;
                }
            }

            // Fix for cottage items disappearing when moved
            GameObject.Find("coffee pan(itemx)").transform.parent = null;
            GameObject.Find("lantern(itemx)").transform.parent = null;
            GameObject.Find("coffee cup(itemx)").transform.parent = null;
            GameObject.Find("camera(itemx)").transform.parent = null;
            GameObject.Find("COTTAGE/ax(itemx)").transform.parent = null;

            GameObject.Find("fireworks bag(itemx)").transform.parent = null;

            // Fix for fishing areas
            GameObject.Find("FishAreaAVERAGE").transform.parent = null;
            GameObject.Find("FishAreaBAD").transform.parent = null;
            GameObject.Find("FishAreaGOOD").transform.parent = null;
            GameObject.Find("FishAreaGOOD2").transform.parent = null;

            // Fix for strawberry field mailboxes
            GameObject.Find("StrawberryField").transform.Find("LOD/MailBox").parent = null;
            GameObject.Find("StrawberryField").transform.Find("LOD/MailBox").parent = null;

            // Fix for items left on cottage chimney clipping through it on first load of cottage
            GameObject.Find("COTTAGE").transform.Find("MESH/Cottage_chimney").parent = null;

            // Fix for floppies at Jokke's new house
            while (perajarvi.transform.Find("TerraceHouse/diskette(itemx)") != null)
            {
                Transform diskette = perajarvi.transform.Find("TerraceHouse/diskette(itemx)");
                if (diskette != null && diskette.parent != null)
                    diskette.parent = null;
            }

            // Fix for Jokke's house furnitures clipping through floor
            perajarvi.transform.Find("TerraceHouse/Apartments/Colliders").parent = null;

            // Applying a script to vehicles that can pick up and drive the player as a passanger to his house.
            // This script makes it so when the player enters the car, the parent of the vehicle is set to null.
            GameObject.Find("TRAFFIC").transform.Find("VehiclesDirtRoad/Rally/FITTAN").gameObject.AddComponent<PlayerTaxiManager>();
            GameObject.Find("NPC_CARS").transform.Find("KUSKI").gameObject.AddComponent<PlayerTaxiManager>();

            // Fixed Ventii bet resetting to default on cabin load.
            PlayMakerFSM cabinGameManagerUseFsm = GameObject.Find("CABIN").transform.Find("Cabin/Ventti/Table/GameManager").gameObject.GetPlayMakerByName("Use");
            FsmState loadGameVentti = cabinGameManagerUseFsm.FindFsmState("Load game");
            List<FsmStateAction> emptyActions = new List<FsmStateAction> { new CustomNullState() };
            loadGameVentti.Actions = emptyActions.ToArray();
            loadGameVentti.SaveActions();

            // Junk cars - setting Load game to null.
            for (int i = 1; GameObject.Find($"JunkCar{i}") != null; i++)
            {
                GameObject junk = GameObject.Find($"JunkCar{i}");
                PlayMakerFSM junkFsm = junk.GetComponent<PlayMakerFSM>();
                FsmState loadJunk = junkFsm.FindFsmState("Load game");
                loadJunk.Actions = new FsmStateAction[] { new CustomNullState() };
                loadJunk.SaveActions();

                worldObjectList.Add(junk.name);
            }

            // Fixes wasp hives resetting to on load values.
            GameObject[] wasphives = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "WaspHive").ToArray();
            foreach (GameObject wasphive in wasphives)
            { 
                wasphive.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }

            // Disabling the script that sets the kinematic state of Satsuma to False.
            GameObject hand = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/Hand");
            PlayMakerFSM pickUp = hand.GetPlayMakerByName("PickUp");
            
            FsmState stateDropPart = pickUp.FindFsmState("Drop part");
            stateDropPart.Actions[0] = new CustomNullState();
            stateDropPart.SaveActions();

            FsmState stateDropPart2 = pickUp.FindFsmState("Drop part 2");
            stateDropPart2.Actions[0] = new CustomNullState();
            stateDropPart2.SaveActions();

            FsmState stateToolPicked = pickUp.FindFsmState("Tool picked");
            stateToolPicked.Actions[2] = new CustomNullState();
            stateToolPicked.SaveActions();

            FsmState stateDropTool = pickUp.FindFsmState("Drop tool");
            stateDropTool.Actions[0] = new CustomNullState();
            stateDropTool.SaveActions();

            // Preventing mattres from being disabled.
            Transform mattres = GameObject.Find("DINGONBIISI").transform.Find("mattres");
            if (mattres != null)
                mattres.parent = null;

            // Item anti clip for cottage.
            GameObject area = new GameObject("MOP_ItemAntiClip");
            area.transform.position = new Vector3(-848.3f, -5.4f, 505.5f);
            area.transform.eulerAngles = new Vector3(0, 343.0013f, 0);
            area.AddComponent<ItemAntiClip>();

            // Z-fighting fix for wristwatch.
            try
            {
                GameObject.Find("PLAYER")
                    .transform.Find("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Watch/Animate/BreathAnim/WristwatchHand/Clock/Pivot/Hour/hour")
                    .gameObject.GetComponent<Renderer>().material.renderQueue = 3001;

                GameObject.Find("PLAYER")
                    .transform.Find("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Watch/Animate/BreathAnim/WristwatchHand/Clock/Pivot/Minute/minute")
                    .gameObject.GetComponent<Renderer>().material.renderQueue = 3002;
            }
            catch { }

            // Adds roll fix to the bus.
            GameObject.Find("BUS").AddComponent<BusRollFix>();

            ModConsole.Print("[MOP] Finished applying fixes");

            //Things that should be enabled when out of proximity of the house
            worldObjectList.Add("NPC_CARS", awayFromHouse: true);
            worldObjectList.Add("TRAFFIC", true);
            worldObjectList.Add("TRAIN", true);
            worldObjectList.Add("Buildings", true);
            worldObjectList.Add("TrafficSigns", true);
            worldObjectList.Add("StreetLights", true);
            worldObjectList.Add("HUMANS", true);
            worldObjectList.Add("TRACKFIELD", true);
            worldObjectList.Add("SkijumpHill", true);
            worldObjectList.Add("Factory", true);
            worldObjectList.Add("WHEAT", true);
            worldObjectList.Add("ROCKS", true);
            worldObjectList.Add("RAILROAD", true);
            worldObjectList.Add("AIRPORT", true);
            worldObjectList.Add("RAILROAD_TUNNEL", true);
            worldObjectList.Add("PierDancehall", true);
            worldObjectList.Add("PierRiver", true);
            worldObjectList.Add("PierStore", true);
            worldObjectList.Add("BRIDGE_dirt", true);
            worldObjectList.Add("BRIDGE_highway", true);
            worldObjectList.Add("BirdTower", 400);
            worldObjectList.Add("SwampColliders", true);
            worldObjectList.Add("RYKIPOHJA", true, false, false);
            worldObjectList.Add("COMPUTER", true, false, true);
            worldObjectList.Add("JOBS/HouseDrunkNew", true);

            ModConsole.Print("[MOP] Away from house world objects loaded");

            // Adding area check if Satsuma is in the inspection's area
            SatsumaInAreaCheck inspectionArea = GameObject.Find("INSPECTION").AddComponent<SatsumaInAreaCheck>();
            inspectionArea.Initialize(new Vector3(20, 20, 20));

            // Check for when Satsuma is on the lifter
            SatsumaInAreaCheck lifterArea = GameObject.Find("REPAIRSHOP/Lifter/Platform").AddComponent<SatsumaInAreaCheck>();
            lifterArea.Initialize(new Vector3(5, 5, 5));

            ModConsole.Print("[MOP] Satsuma triggers loaded");

            // Jokke's furnitures.
            // Only renderers are going to be toggled.
            if (GameObject.Find("tv(Clo01)") != null)
            {
                worldObjectList.Add("tv(Clo01)", 100, true);
                worldObjectList.Add("chair(Clo02)", 100, true);
                worldObjectList.Add("chair(Clo05)", 100, true);
                worldObjectList.Add("bench(Clo01)", 100, true);
                worldObjectList.Add("bench(Clo02)", 100, true);
                worldObjectList.Add("table(Clo02)", 100, true);
                worldObjectList.Add("table(Clo03)", 100, true);
                worldObjectList.Add("table(Clo04)", 100, true);
                worldObjectList.Add("table(Clo05)", 100, true);
                worldObjectList.Add("desk(Clo01)", 100, true);
                worldObjectList.Add("arm chair(Clo01)", 100, true);

                ModConsole.Print("[MOP] Jokke's furnitures found and loaded");
            }

            // Haybales.
            // First we null out the prevent it from reloading the position of haybales.
            GameObject haybalesParent = GameObject.Find("JOBS/HayBales");
            if (haybalesParent != null)
            {
                haybalesParent.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                // And now we add all child haybale to world objects.
                foreach (Transform haybale in haybalesParent.transform.GetComponentInChildren<Transform>())
                {
                    worldObjectList.Add(haybale.gameObject.name, 120);
                }
            }

            // Initialize Items class
            new Items();
            ModConsole.Print("[MOP] Items class loaded");

            HookPreSaveGame();

            ModConsole.Print("[MOP] Loading rules...");
            foreach (ToggleRule v in Rules.instance.ToggleRules)
            {
                try
                {
                    switch (v.ToggleMode)
                    {
                        default:
                            ModConsole.Error($"[MOP] Unrecognized toggle mode for {v.ObjectName}: {v.ToggleMode}.");
                            break;
                        case ToggleModes.Normal:
                            if (GameObject.Find(v.ObjectName) == null)
                            {
                                ModConsole.Error($"[MOP] Couldn't find world object {v.ObjectName}");
                                continue;
                            }

                            worldObjectList.Add(v.ObjectName);
                            break;
                        case ToggleModes.Renderer:
                            if (GameObject.Find(v.ObjectName) == null)
                            {
                                ModConsole.Error($"[MOP] Couldn't find world object {v.ObjectName}");
                                continue;
                            }

                            worldObjectList.Add(v.ObjectName, 200, true);
                            break;
                        case ToggleModes.Item:
                            GameObject g = GameObject.Find(v.ObjectName);

                            if (g == null)
                            {
                                ModConsole.Error($"[MOP] Couldn't find item {v.ObjectName}");
                                continue;
                            }

                            if (g.GetComponent<ItemHook>() == null)
                                g.AddComponent<ItemHook>();
                            break;
                        case ToggleModes.Vehicle:
                            if (Rules.instance.SpecialRules.IgnoreModVehicles) continue;

                            if (GameObject.Find(v.ObjectName) == null)
                            {
                                ModConsole.Error($"[MOP] Couldn't find vehicle {v.ObjectName}");
                                continue;
                            }

                            vehicles.Add(new Vehicle(v.ObjectName));
                            break;
                        case ToggleModes.VehiclePhysics:
                            if (Rules.instance.SpecialRules.IgnoreModVehicles) continue;

                            if (GameObject.Find(v.ObjectName) == null)
                            {
                                ModConsole.Error($"[MOP] Couldn't find vehicle {v.ObjectName}");
                                continue;
                            }
                            vehicles.Add(new Vehicle(v.ObjectName));
                            Vehicle veh = vehicles[vehicles.Count - 1];
                            veh.Toggle = veh.ToggleUnityCar;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, "TOGGLE_RULES_LOAD_ERROR");
                }
            }

            ModConsole.Print("[MOP] Rules loading complete!");

            // Initialzie sector manager
            ActivateSectors();

            // Add DynamicDrawDistance component.
            gameObject.AddComponent<DynamicDrawDistance>();

            if (!MopSettings.SafeMode)
                ToggleAll(false, ToggleAllMode.OnLoad);

            // Initialize the coroutines.
            currentLoop = LoopRoutine();
            StartCoroutine(currentLoop);
            StartCoroutine(ControlCoroutine());

            string finalMessage = "[MOP] MOD LOADED SUCCESFULLY!";
            float money = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value;
            finalMessage = Mathf.Approximately(69420.0f, 69420.9f) ? finalMessage.Rainbowmize() : $"<color=green>{finalMessage}</color>";
            ModConsole.Print(finalMessage);
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        /// <summary>
        /// Looks for gamobject named SAVEGAME, and hooks PreSaveGame into them.
        /// </summary>
        void HookPreSaveGame()
        {
            GameObject[] saveGames = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(obj => obj.name == "SAVEGAME").ToArray();

            try
            {
                int i = 0;
                for (; i < saveGames.Length; i++)
                {
                    bool useInnactiveFix = false;
                    bool isJail = false;

                    if (!saveGames[i].activeSelf)
                    {
                        useInnactiveFix = true;
                        saveGames[i].SetActive(true);
                    }

                    if (saveGames[i].transform.parent.name == "JAIL" && saveGames[i].transform.parent.gameObject.activeSelf == false)
                    {
                        useInnactiveFix = true;
                        isJail = true;
                        saveGames[i].transform.parent.gameObject.SetActive(true);
                    }

                    FsmHook.FsmInject(saveGames[i], "Mute audio", PreSaveGame);

                    if (useInnactiveFix)
                    {
                        if (isJail)
                        {
                            saveGames[i].transform.parent.gameObject.SetActive(false);
                            continue;
                        }

                        saveGames[i].SetActive(false);
                    }
                }

                // Hooking up on death save.
                GameObject onDeathSaveObject = new GameObject("MOP_OnDeathSave");
                onDeathSaveObject.transform.parent = GameObject.Find("Systems").transform.Find("Death/GameOverScreen");
                OnDeathBehaviour behaviour = onDeathSaveObject.AddComponent<OnDeathBehaviour>();
                behaviour.Initialize(PreSaveGame);
                i++;

                ModConsole.Print($"[MOP] Hooked {i} save points!");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, "SAVE_HOOK_ERROR");
            }
        }

        /// <summary>
        /// This void is initialized before the player decides to save the game.
        /// </summary>
        void PreSaveGame()
        {
            ModConsole.Print("[MOP] Initializing Pre-Save Actions...");
            MopSettings.IsModActive = false;
            StopCoroutine(currentLoop);
            ToggleAll(true, ToggleAllMode.OnSave);
            ModConsole.Print("[MOP] Pre-Save Actions Completed!");
        }

        /// <summary>
        /// This coroutine runs
        /// </summary>
        private IEnumerator currentLoop;
        IEnumerator LoopRoutine()
        {
            MopSettings.IsModActive = true;
            while (MopSettings.IsModActive)
            {
                ticks++;
                if (ticks > 1000)
                    ticks = 0;

                inSectorMode = SectorManager.instance != null && SectorManager.instance.PlayerInSector;
                isPlayerAtYard = MopSettings.ActiveDistance == 0 ? Vector3.Distance(player.position, places[0].GetTransform().position) < 100
                    : Vector3.Distance(player.position, places[0].GetTransform().position) < 100 * MopSettings.ActiveDistanceMultiplicationValue;

                // When player is in any of the sectors, MOP will act like the player is at yard.
                if (SectorManager.instance.PlayerInSector)
                    isPlayerAtYard = true;

                int half = worldObjectList.Count / 2;

                // Disable Satsuma engine renderer, if player is in Satsuma
                Satsuma.instance.ToggleEngineRenderers(!MopFsmManager.IsPlayerInSatsuma());
                yield return null;

                int i;
                // World Objects.
                for (i = 0; i < worldObjectList.Count; i++)
                {
                    if (i % worldObjectList.Count / 2 == 0)
                        yield return null;

                    try
                    {
                        WorldObject worldObject = worldObjectList.Get(i);

                        if (Rules.instance.SectorRulesContains(worldObject.gameObject.name))
                            continue;

                        // Should the object be disabled when the player leaves the house?
                        if (worldObject.AwayFromHouse)
                        {
                            if (worldObject.gameObject.name == "NPC_CARS" && inSectorMode)
                                continue;

                            if (worldObject.gameObject.name == "COMPUTER" && worldObject.gameObject.transform.Find("SYSTEM").gameObject.activeSelf)
                                continue;

                            worldObject.Toggle(worldObject.ReverseToggle ? isPlayerAtYard : !isPlayerAtYard);
                            continue;
                        }

                        // The object will be disables, if the player is in the range of that object.
                        worldObject.Toggle(IsEnabled(worldObject.transform, worldObject.Distance));
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, "WORLD_OBJECT_TOGGLE_ERROR");
                    }
                }

                // Safe mode prevents toggling elemenets that MAY case some issues (vehicles, items, etc.)
                if (MopSettings.SafeMode)
                {
                    yield return new WaitForSeconds(1);
                    continue;
                }

                // So we create two separate lists - one is meant to enable, and second is ment to disable them,
                // Why?
                // If we enable items before enabling vehicle inside of which these items are supposed to be, they'll fall through to ground.
                // And the opposite happens if we disable vehicles before disabling items.
                // So if we are disabling items, we need to do that BEFORE we disable vehicles.
                // And we need to enable items AFTER we enable vehicles.
                itemsToEnable.Clear();
                itemsToDisable.Clear();
                for (i = 0; i < Items.instance.ItemsHooks.Count; i++)
                {
                    if (half != 0)
                        if (i % half == 0) yield return null;

                    // Safe check if somehow the i gets bigger than array length.
                    if (i >= Items.instance.ItemsHooks.Count) break;

                    try
                    {
                        if (Items.instance.ItemsHooks[i] == null || Items.instance.ItemsHooks[i].gameObject == null)
                        {
                            // Remove item at the current i
                            Items.instance.ItemsHooks.RemoveAt(i);

                            // Decrease the i by 1, because the List has shifted, so the items will not be skipped.
                            // Then continue.
                            i--;
                            half = Items.instance.ItemsHooks.Count / 2;
                            continue;
                        }

                        bool toEnable = IsEnabled(Items.instance.ItemsHooks[i].transform, 150);
                        if (toEnable)
                            itemsToEnable.Add(Items.instance.ItemsHooks[i]);
                        else
                            itemsToDisable.Add(Items.instance.ItemsHooks[i]);
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, "ITEM_TOGGLE_GATHER_ERROR");
                    }
                }

                // Items To Disable
                int full = itemsToDisable.Count;
                if (full > 0)
                {
                    half = itemsToDisable.Count / 2;
                    for (i = 0; i < full; i++)
                    {
                        if (half != 0)
                            if (i % half == 0) yield return null;

                        try
                        {
                            itemsToDisable[i].Toggle(false);
                        }
                        catch (Exception ex)
                        {
                            ExceptionManager.New(ex, "ITEM_TOGGLE_ENABLE_ERROR");
                        }
                    }
                }

                // Vehicles (new)
                half = vehicles.Count / 2;
                for (i = 0; i < vehicles.Count; i++)
                {
                    if (half != 0)
                        if (i % half == 0) yield return null;

                    try
                    {
                        if (vehicles[i] == null)
                        {
                            ModConsole.Print($"[MOP] Vehicle {i} has been skipped, because it's missing.");
                            continue;
                        }

                        float distance = Vector3.Distance(player.transform.position, vehicles[i].transform.position);
                        float toggleDistance = MopSettings.ActiveDistance == 0
                            ? MopSettings.UnityCarActiveDistance : MopSettings.UnityCarActiveDistance * MopSettings.ActiveDistanceMultiplicationValue;

                        if (Rules.instance.SpecialRules.ExperimentalOptimization)
                        {
                            if (i == 0 && SatsumaInGarage.Instance.AreGarageDoorsClosed() 
                                       && SatsumaInGarage.Instance.IsSatsumaInGarage()
                                       && !Satsuma.instance.IsKeyInserted())
                            {
                                vehicles[i].ToggleUnityCar(false);
                                vehicles[i].Toggle(false);
                                continue;
                            }
                        }

                        switch (i)
                        {
                            // Satsuma
                            case 0:
                                Satsuma.instance.ToggleElements(distance);
                                vehicles[i].ToggleEventSounds(distance < 5);
                                break;
                            // Jonnez
                            case 2:
                                vehicles[i].ToggleEventSounds(distance < 2);
                                break;
                        }

                        vehicles[i].ToggleUnityCar(IsVehicleEnabled(distance, toggleDistance, true));
                        vehicles[i].Toggle(IsVehicleEnabled(distance));
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, $"VEHICLE_TOGGLE_ERROR_{i}");
                    }
                }

                // Items To Enable
                full = itemsToEnable.Count;
                if (full > 0)
                {
                    half = itemsToEnable.Count / 2;
                    for (i = 0; i < full; i++)
                    {
                        if (half != 0)
                            if (i % half == 0) yield return null;

                        try
                        {
                            itemsToEnable[i].Toggle(true);
                        }
                        catch (Exception ex)
                        {
                            ExceptionManager.New(ex, "ITEM_TOGGLE_ENABLE_ERROR");
                        }
                    }
                }

                // Places (New)
                full = places.Count;
                for (i = 0; i < full; ++i)
                {
                    if (i % full / 2 == 0)
                        yield return null;

                    try
                    {
                        if (Rules.instance.SectorRulesContains(places[i].GetName()))
                            continue;

                        places[i].ToggleActive(IsPlaceEnabled(places[i].GetTransform(), places[i].GetToggleDistance()));
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, $"PLACE_TOGGLE_ERROR_{i}");
                    }
                }

                yield return new WaitForSeconds(1);
            }
        }

        void Update()
        {
            if (!MopSettings.IsModActive || Satsuma.instance == null) return;
            Satsuma.instance.ForceFuckingRotation();
        }

        /// <summary>
        /// Checks if the object is supposed to be enabled by calculating the distance between player and target.
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <param name="toggleDistance">Distance below which the object should be enabled (default 200 units).</param>
        bool IsEnabled(Transform target, float toggleDistance = 200)
        {
            if (inSectorMode)
                toggleDistance *= MopSettings.ActiveDistance == 0 ? 0.5f : 0.1f;

            return Vector3.Distance(player.transform.position, target.position) < toggleDistance * MopSettings.ActiveDistanceMultiplicationValue;
        }

        /// <summary>
        /// Same as IsEnabled, but used for vehicles only
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        bool IsVehicleEnabled(float distance, float toggleDistance = 200, bool unityCar = false)
        {
            if (inSectorMode && !unityCar)
                toggleDistance = 30;

            return distance < toggleDistance;
        }

        bool IsPlaceEnabled(Transform target, float toggleDistance = 200)
        {
            return Vector3.Distance(player.transform.position, target.position) < toggleDistance * MopSettings.ActiveDistanceMultiplicationValue;
        }

        int ticks;
        int lastTick;
        int retries;
        const int MaxRetries = 3;

        /// <summary>
        /// Every 10 seconds check if the coroutine is still active.
        /// If not, try to restart it.
        /// It is checked by two values - ticks and lastTick
        /// Ticks are added by coroutine. If the value is different than the lastTick, everything is okay.
        /// If the ticks and lastTick is the same, that means coroutine stopped.
        /// </summary>
        /// <returns></returns>
        IEnumerator ControlCoroutine()
        {
            while (MopSettings.IsModActive)
            {
                yield return new WaitForSeconds(10);

                if (lastTick == ticks)
                {
                    if (retries > MaxRetries)
                    {
                        ModConsole.Error("[MOP] Restart attempt failed. Enabling Safe Mode.");
                        ModConsole.Error("[MOP] Please contact mod developer. Make sure you send output_log and last MOP crash log!");
                        MopSettings.EnableSafeMode();
                        try { ToggleAll(true); } catch { }
                        yield break;
                    }

                    retries++;
                    ModConsole.Warning($"[MOP] MOP has stopped working! Restart attempt {retries}/{MaxRetries}...");
                    StopCoroutine(currentLoop);
                    currentLoop = LoopRoutine();
                    StartCoroutine(currentLoop);
                }
                else
                {
                    lastTick = ticks;
                }
            }
        }

        public enum ToggleAllMode { Default, OnSave, OnLoad }

        /// <summary>
        /// Toggles on all objects.
        /// </summary>
        public void ToggleAll(bool enabled, ToggleAllMode mode = ToggleAllMode.Default)
        {
            // World objects
            for (int i = 0; i < worldObjectList.Count; i++)
            {
                try
                {
                    worldObjectList.Get(i).Toggle(enabled);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, "TOGGLE_ALL_WORLD_OBJECTS_ERROR");
                }
            }

            if (MopSettings.SafeMode) return;

            // Vehicles
            for (int i = 0; i < vehicles.Count; i++)
            {
                try
                {
                    vehicles[i].Toggle(enabled);

                    if (mode == ToggleAllMode.OnLoad)
                    {
                        vehicles[i].ForceToggleUnityCar(false);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, $"TOGGLE_ALL_VEHICLE_ERROR_{i}");
                }
            }

            // Disable SatsumaTrunk.
            if (mode == ToggleAllMode.OnSave)
            {
                try
                {
                    if (Satsuma.instance.Trunks != null)
                    {
                        foreach (var trunk in Satsuma.instance.Trunks)
                            trunk.OnGameSave();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, "TOGGLE_ALL_SATSUMA_TRUNK_ERROR");
                }
            }

            // Items
            for (int i = 0; i < Items.instance.ItemsHooks.Count; i++)
            {
                try
                {
                    Items.instance.ItemsHooks[i].Toggle(enabled);

                    // If we're saving, MOP forces on items the "SAVEGAME" event.
                    // This fixes an issue with items not getting saved, for some reason.
                    if (mode == ToggleAllMode.OnSave)
                    {
                        PlayMakerFSM fsm = Items.instance.ItemsHooks[i].gameObject.GetComponent<PlayMakerFSM>();
                        if (fsm != null)
                            fsm.SendEvent("SAVEGAME");
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, "TOGGLE_ALL_ITEMS_ERROR");
                }
            }

            // Places
            for (int i = 0; i < places.Count; i++)
            {
                try
                {
                    places[i].ToggleActive(enabled);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, $"TOGGLE_ALL_PLACES_{i}");
                }
            }

            // Force teleport kilju bottles.
            try
            {
                if (mode == ToggleAllMode.OnSave)
                {
                    Transform canTrigger = GameObject.Find("JOBS").transform.Find("HouseDrunkNew/BeerCampNew/BeerCamp/KiljuBuyer/CanTrigger");

                    // If canTrigger object is not located at new house, get one from the old Jokke's house.
                    if (canTrigger == null)
                        canTrigger = GameObject.Find("JOBS").transform.Find("HouseDrunk/BeerCampOld/BeerCamp/KiljuBuyer/CanTrigger");

                    canTrigger.gameObject.GetComponent<PlayMakerFSM>().SendEvent("STOP");
                }
            }
            catch (Exception ex) 
            {
                ExceptionManager.New(ex, "TOGGLE_ALL_JOBS_DRUNK");
            }

            // ToggleElements class of Satsuma.
            try
            {
                if (mode == ToggleAllMode.OnSave)
                {
                    Satsuma.instance.ToggleElements(0);
                }
                else
                {
                    Satsuma.instance.ToggleElements(enabled ? 1000 : 0);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, "TOGGLE_ALL_SATSUMA_TOGGLE_ELEMENTS");
            }
        }

        void ActivateSectors()
        {
            if (gameObject.GetComponent<SectorManager>() == null)
            {
                this.gameObject.AddComponent<SectorManager>();

#pragma warning disable IDE0017 // Simplify object initialization
                GameObject colliderCheck = new GameObject("MOP_PlayerCheck");
#pragma warning restore IDE0017 // Simplify object initialization
                colliderCheck.layer = 20;
                colliderCheck.transform.parent = GameObject.Find("PLAYER").transform;
                colliderCheck.transform.localPosition = Vector3.zero;
                BoxCollider collider = colliderCheck.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = new Vector3(.1f, 1, .1f);
                Rigidbody rb = colliderCheck.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }

        public Vehicle GetFlatbed()
        {
            return vehicles[6];
        }

        public Transform GetPlayer()
        {
            return player;
        }

        public WorldObjectList GetWorldObjectList()
        {
            return worldObjectList;
        }

        public Transform GetLostSpawner()
        {
            return lostSpawner;
        }

        public bool IsInSector()
        {
            return inSectorMode;
        }
    }
}
