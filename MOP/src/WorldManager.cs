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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace MOP
{
    /// <summary>
    /// Controls all objects in the game world.
    /// </summary>
    class WorldManager : MonoBehaviour
    {
        public static WorldManager instance;

        Transform player;

        // Vehicles
        List<Vehicle> vehicles;

        // Places
        Teimo teimo;
        RepairShop repairShop;
        Yard yard;

        // World Objects
        WorldObjectList worldObjectList;

        // Area Checks
        SatsumaInAreaCheck inspectionArea;

        bool IsPlayerOnYard { get; set; }

        public WorldManager()
        {
            // Start the delayed initialization routine
            StartCoroutine(DelayedInitializaitonRoutine());
        }

        IEnumerator DelayedInitializaitonRoutine()
        {
            yield return new WaitForSeconds(2);

            // If the GT grille is attached, perform extra delay, until the "grille gt(Clone)" parent isn't "pivot_grille".
            // Or until 5 seconds have passed.
            int ticks = 0;
            Transform gtGrilleTransform = GameObject.Find("grille gt(Clone)").transform;
            while (MopFsmManager.IsGTGrilleInstalled() && gtGrilleTransform.parent.name != "pivot_grille")
            {
                yield return new WaitForSeconds(1);
                
                // Escape the loop after 5 retries
                ticks++;
                if (ticks > 5)
                    break;
            }

            Initialize();
        }

        void Initialize()
        {
            instance = this;

            // Initialize the worldObjectList list
            worldObjectList = new WorldObjectList();

            // Looking for player and yard
            player = GameObject.Find("PLAYER").transform;

            // Loading vehicles
            vehicles = new List<Vehicle>();
            vehicles.Add(new Satsuma("SATSUMA(557kg, 248)"));
            vehicles.Add(new Vehicle("HAYOSIKO(1500kg, 250)"));
            vehicles.Add(new Vehicle("JONNEZ ES(Clone)"));
            vehicles.Add(new Vehicle("KEKMET(350-400psi)"));
            vehicles.Add(new Vehicle("RCO_RUSCKO12(270)"));
            vehicles.Add(new Vehicle("FERNDALE(1630kg)"));
            vehicles.Add(new Vehicle("FLATBED"));
            vehicles.Add(new Vehicle("GIFU(750/450psi)"));

            if (!MopSettings.IgnoreModVehicles)
            {
                // Drivable Fury mod
                if (CompatibilityManager.instance.DrivableFury)
                {
                    vehicles.Add(new Vehicle("FURY(1630kg)"));
                }

                // Second Ferndale
                if (CompatibilityManager.instance.SecondFerndale)
                {
                    vehicles.Add(new Vehicle("SECONDFERNDALE(1630kg)"));
                }

                // GAZ 24 Volga
                if (CompatibilityManager.instance.Gaz)
                {
                    vehicles.Add(new Vehicle("GAZ24(1420kg)"));
                }

                // Police Ferndale
                if (CompatibilityManager.instance.PoliceFerndale)
                {
                    vehicles.Add(new Vehicle("POLICEFERNDALE(1630kg)"));
                }
            }

            // World Objects
            worldObjectList.Add("CABIN");
            worldObjectList.Add("COTTAGE", 400);
            worldObjectList.Add("DANCEHALL");
            worldObjectList.Add("INSPECTION");
            worldObjectList.Add("PERAJARVI");
            worldObjectList.Add("RYKIPOHJA", 400);
            worldObjectList.Add("SOCCER");
            worldObjectList.Add("WATERFACILITY");
            worldObjectList.Add("DRAGRACE", 1100);
            if (!CompatibilityManager.instance.JetSky && !CompatibilityManager.instance.Moonshinestill)
                worldObjectList.Add("BOAT");
            worldObjectList.Add("StrawberryField", 400);

            ModConsole.Print("[MOP] Main world objects loaded");

            // Initialize shops
            teimo = new Teimo();
            repairShop = new RepairShop();
            yard = new Yard();

            ModConsole.Print("[MOP] Initialized places");

            Transform buildings = GameObject.Find("Buildings").transform;

            // Find house of Teimo and detach it from Perajarvi, so it can be loaded and unloaded separately
            // It shouldn't cause any issues, but that needs testing.
            GameObject perajarvi = GameObject.Find("PERAJARVI");
            perajarvi.transform.Find("HouseRintama4").parent = buildings;
            // Same for chicken house
            perajarvi.transform.Find("ChickenHouse").parent = buildings;

            // Chicken house (barn) close to player's house
            GameObject.Find("Buildings").transform.Find("ChickenHouse").parent = null;

            // Fix for church wall. Changing it's parent to NULL, so it will not be loaded or unloaded.
            // It used to be changed to CHURCH gameobject, 
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

            // Removes the mansion from the Buildings, so the tires will not land under the mansion.
            GameObject.Find("autiotalo").transform.parent = null;

            // Fix for cottage items disappearing when moved
            GameObject.Find("coffee pan(itemx)").transform.parent = null;
            GameObject.Find("lantern(itemx)").transform.parent = null;
            GameObject.Find("coffee cup(itemx)").transform.parent = null;
            GameObject.Find("camera(itemx)").transform.parent = null;

            GameObject.Find("fireworks bag(itemx)").transform.parent = null;

            // Fix for fishing areas
            GameObject.Find("FishAreaAVERAGE").transform.parent = null;
            GameObject.Find("FishAreaBAD").transform.parent = null;
            GameObject.Find("FishAreaGOOD").transform.parent = null;
            GameObject.Find("FishAreaGOOD2").transform.parent = null;

            // Fix for strawberry field mailboxes
            GameObject.Find("StrawberryField").transform.Find("LOD/MailBox").parent = null;
            GameObject.Find("StrawberryField").transform.Find("LOD/MailBox").parent = null;

            // Ryhipohja fixes
            Transform rykipohja = GameObject.Find("RYKIPOHJA").transform;
            rykipohja.Find("HouseRintama2").parent = buildings;
            rykipohja.Find("ChickenHouse 3").parent = buildings;
            rykipohja.Find("HouseOld5").parent = buildings;

            // Fix for items left on cottage chimney clipping through it on first load of cottage
            GameObject.Find("COTTAGE").transform.Find("MESH/Cottage_chimney").parent = null;

            // Fix for floppies at Jokke's new house
            perajarvi.transform.Find("TerraceHouse/diskette(itemx)").parent = null;
            perajarvi.transform.Find("TerraceHouse/diskette(itemx)").parent = null;

            // Fix for Jokke's house furnitures clipping through floor
            perajarvi.transform.Find("TerraceHouse/Apartments/Colliders").parent = null;

            ModConsole.Print("[MOP] Finished applying fixes");

            //Things that should be enabled when out of proximity of the house
            worldObjectList.Add("NPC_CARS", awayFromHouse: true);
            worldObjectList.Add("TRAFFIC", true);
            worldObjectList.Add("TRAIN", true);
            worldObjectList.Add("Buildings", true);
            worldObjectList.Add("TrafficSigns", true);
            worldObjectList.Add("StreetLights", true);
            worldObjectList.Add("HUMANS", true);
            worldObjectList.Add("HayBales", true);
            worldObjectList.Add("TRACKFIELD", true);
            worldObjectList.Add("SkijumpHill", true);
            worldObjectList.Add("Factory", true);
            worldObjectList.Add("SWAMP", true);
            worldObjectList.Add("WHEAT", true);
            worldObjectList.Add("ROCKS", true);
            worldObjectList.Add("RAILROAD", true);
            worldObjectList.Add("AIRPORT", true);
            worldObjectList.Add("RAILROAD_TUNNEL", true);
            worldObjectList.Add("PierDancehall", true);
            worldObjectList.Add("PierRiver", true);
            worldObjectList.Add("PierStore", true);

            ModConsole.Print("[MOP] Away from house world objects loaded");

            // Adding area check if Satsuma is in the inspection's area
            inspectionArea = GameObject.Find("INSPECTION").AddComponent<SatsumaInAreaCheck>();
            inspectionArea.Initialize(new Vector3(20, 20, 20));

            // Check for when Satsuma is on the lifter
            SatsumaInAreaCheck lifterArea = GameObject.Find("REPAIRSHOP/Lifter/Platform").AddComponent<SatsumaInAreaCheck>();
            lifterArea.Initialize(new Vector3(5, 5, 5));

            ModConsole.Print("[MOP] Satsuma Area Checks loaded");

            // Jokke's furnitures.
            // Only renderers will be toggled
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

            // Initialize Items class
            new Items();

            ModConsole.Print("[MOP] Items class loaded");

            HookPreSaveGame();

            // Initialize the coroutines 
            StartCoroutine(LoopRoutine());
            StartCoroutine(ControlCoroutine());

            ModConsole.Print("[MOP] MOD LOADED SUCCESFULLY!");
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
                        saveGames[i].SetActive(false);

                        if (isJail)
                        {
                            saveGames[i].transform.parent.gameObject.SetActive(false);
                        }
                    }
                }

                ModConsole.Print($"[MOP] Hooked {i} save points!");
            }
            catch (Exception ex)
            {
                ModConsole.Error(ex.ToString());
            }
        }

        /// <summary>
        /// This void is initialized before the player decides to save the game.
        /// </summary>
        void PreSaveGame()
        {
            MopSettings.IsModActive = false;
            ToggleActiveAll();
        }

        /// <summary>
        /// This coroutine runs
        /// </summary>
        IEnumerator LoopRoutine()
        {
            MopSettings.IsModActive = true;
            while (MopSettings.IsModActive)
            {
                ticks++;
                if (ticks > 1000)
                    ticks = 0;

                IsPlayerOnYard = MopSettings.ActiveDistance == 0 ? Vector3.Distance(player.position, yard.transform.position) < 100 
                    : Vector3.Distance(player.position, yard.transform.position) < 100 * MopSettings.ActiveDistanceMultiplicationValue;

                int half = worldObjectList.Count / 2;
                int i = 0;

                try
                {
                    // Go through the list worldObjectList list
                    for (i = 0; i < half; i++)
                    {
                        // Should the object be disabled when the player leaves the house?
                        if (worldObjectList.Get(i).AwayFromHouse)
                        {
                            worldObjectList.Get(i).Toggle(!IsPlayerOnYard);
                            continue;
                        }

                        // Fix for inspection area being unloaded after the successfull inspection,
                        // making game not save the car inspection status.
                        if (worldObjectList.Get(i).gameObject.name == "INSPECTION" && inspectionArea.InspectionPreventUnload)
                            continue;

                        // The object will be disables, if the player is in the range of that object.
                        worldObjectList.Get(i).Toggle(IsEnabled(worldObjectList.Get(i).transform, worldObjectList.Get(i).Distance));
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex);
                }

                yield return null;

                try
                {
                    for (; i < worldObjectList.Count; i++)
                    {
                        // Should the object be disabled when the player leaves the house?
                        if (worldObjectList.Get(i).AwayFromHouse)
                        {
                            worldObjectList.Get(i).Toggle(!IsPlayerOnYard);
                            continue;
                        }

                        // Fix for inspection area being unloaded after the successfull inspection,
                        // making game not save the car inspection status.
                        if (worldObjectList.Get(i).gameObject.name == "INSPECTION" && inspectionArea.InspectionPreventUnload)
                            continue;

                        // The object will be disables, if the player is in the range of that object.
                        worldObjectList.Get(i).Toggle(IsEnabled(worldObjectList.Get(i).transform, worldObjectList.Get(i).Distance));
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex);
                }

                if (MopSettings.SafeMode)
                {
                    yield return new WaitForSeconds(1);
                    continue;
                }

                // Vehicles
                if (MopSettings.ToggleVehicles)
                {
                    try
                    {
                        half = vehicles.Count / 2;
                        for (i = 0; i < half; i++)
                        {
                            if (vehicles[i] == null)
                            {
                                ModConsole.Print("[MOP] Vehicle " + i + " has been skipped, because an error occured.");
                                continue;
                            }

                            float distance = Vector3.Distance(player.transform.position, vehicles[i].transform.position);
                            float toggleDistance = MopSettings.ActiveDistance == 0 ? MopSettings.UnityCarActiveDistance : MopSettings.UnityCarActiveDistance * MopSettings.ActiveDistanceMultiplicationValue;
                            vehicles[i].ToggleUnityCar(IsEnabled(distance, toggleDistance));
                            vehicles[i].Toggle(IsEnabled(distance));
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex);
                    }

                    yield return null;

                    try
                    {
                        for (; i < vehicles.Count; i++)
                        {
                            if (vehicles[i] == null)
                            {
                                ModConsole.Print("[MOP] Vehicle " + i + " has been skipped, because an error occured.");
                                continue;
                            }

                            float distance = Vector3.Distance(player.transform.position, vehicles[i].transform.position);
                            float toggleDistance = MopSettings.ActiveDistance == 0 ? MopSettings.UnityCarActiveDistance : MopSettings.UnityCarActiveDistance * MopSettings.ActiveDistanceMultiplicationValue;
                            vehicles[i].ToggleUnityCar(IsEnabled(distance, toggleDistance));
                            vehicles[i].Toggle(IsEnabled(distance));
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex);
                    }
                }

                // Shop items
                if (MopSettings.ToggleItems)
                {
                    try
                    {
                        half = Items.instance.ItemsHooks.Count / 2;
                        for (i = 0; i < half; ++i)
                        {
                            if (Items.instance.ItemsHooks[i] == null || Items.instance.ItemsHooks[i].gameObject == null)
                            {
                                if (!CompatibilityManager.instance.BottleRecycling)
                                    ModConsole.Print("[MOP] Removed one item from ItemHooks list, because it doesn't exist anymore.\n");

                                // Remove item at the current i
                                Items.instance.ItemsHooks.RemoveAt(i);

                                // Decrease the i by 1, because the List has shifted, so the items will not be skipped
                                // Then continue;
                                i--;
                                continue;
                            }

                            Items.instance.ItemsHooks[i].ToggleActive(IsEnabled(Items.instance.ItemsHooks[i].gameObject.transform));
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex);
                    }

                    yield return null;

                    try
                    {
                        for (; i < Items.instance.ItemsHooks.Count; ++i)
                        {
                            if (Items.instance.ItemsHooks[i] == null || Items.instance.ItemsHooks[i].gameObject == null)
                            {
                                if (!CompatibilityManager.instance.BottleRecycling)
                                    ModConsole.Print("[MOP] Removed one item from ItemHooks list, because it doesn't exist anymore.\n");

                                // Remove item at the current i
                                Items.instance.ItemsHooks.RemoveAt(i);
                                
                                // Decrease the i by 1, because the List has shifted, so the items will not be skipped
                                // Then continue;
                                i--;
                                continue;
                            }

                            Items.instance.ItemsHooks[i].ToggleActive(IsEnabled(Items.instance.ItemsHooks[i].gameObject.transform));
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex);
                    }
                }

                try
                {
                    teimo.ToggleActive(IsEnabled(teimo.transform));
                    repairShop.ToggleActive(IsEnabled(repairShop.transform, 400));
                    yard.ToggleActive(IsEnabled(yard.transform, 300));
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex);
                }

                yield return new WaitForSeconds(1);
            }
        }

        /// <summary>
        /// Checks if the object is supposed to be enabled by calculating the distance between player and target.
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <param name="toggleDistance">Distance below which the object should be enabled (default 200 units).</param>
        bool IsEnabled(Transform target, float toggleDistance = 200)
        {
            return Vector3.Distance(player.transform.position, target.position) < toggleDistance * MopSettings.ActiveDistanceMultiplicationValue;
        }

        bool IsEnabled(float distance, float toggleDistance = 200)
        {
            return distance < toggleDistance * MopSettings.ActiveDistanceMultiplicationValue;
        }

        int ticks;
        int lastTick;
        bool restartTried;

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
                    if (restartTried)
                    {
                        ModConsole.Print("[MOP] Restart attempt failed. Safe Mode will be enabled this time.\n\n" +
                            "You can try and disable some objects (like Vehicles and Store Items) from being disabled.");
                        MopSettings.SafeMode = true;
                        ToggleActiveAll();
                    }

                    restartTried = true;
                    ModConsole.Error("[MOP] MOP has stopped working. Trying to restart the now...\n\n" +
                        "If the issue will still occure, please turn on output_log.txt in MSC Mod Loader settings, " +
                        "and check output_log.txt in MSC folder.\n\noutput_log.txt is located in mysummercar_Data " +
                        "in My Summer Car folder.");
                    StartCoroutine(LoopRoutine());
                }

                restartTried = false;
                lastTick = ticks;
            }
        }

        /// <summary>
        /// Toggles on all objects.
        /// </summary>
        public void ToggleActiveAll()
        {
            try
            {
                // World objects
                for (int i = 0; i < worldObjectList.Count; i++)
                {
                    worldObjectList.Get(i).Toggle(true);
                }

                // Vehicles
                for (int i = 0; i < vehicles.Count; i++)
                {
                    vehicles[i].Toggle(true);
                }

                // Items
                for (int i = 0; i < Items.instance.ItemsHooks.Count; i++)
                {
                    Items.instance.ItemsHooks[i].ToggleActive(true);
                }

                // Stores
                teimo.ToggleActive(true);
                repairShop.ToggleActive(true);
                yard.ToggleActive(true);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex);
            }
        }
    }
}
