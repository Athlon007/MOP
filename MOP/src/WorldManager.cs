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
        CharacterController playerCharacterController;

        List<Vehicle> vehicles;
        List<Place> places;

        WorldObjectList worldObjectList;

        bool isPlayerAtYard;

        public WorldManager()
        {
            // Start the delayed initialization routine
            StartCoroutine(DelayedInitializaitonRoutine());
        }

        IEnumerator DelayedInitializaitonRoutine()
        {
            yield return new WaitForSeconds(2);

            // If the GT grille is attached, perform extra delay, until the "grille gt(Clone)" parent isn't "pivot_grille",
            // or until 5 seconds have passed.
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
            playerCharacterController = player.GetComponent<CharacterController>();

            // Loading vehicles
            vehicles = new List<Vehicle>
            {
                new Satsuma("SATSUMA(557kg, 248)"),
                new Vehicle("HAYOSIKO(1500kg, 250)"),
                new Vehicle("JONNEZ ES(Clone)"),
                new Vehicle("KEKMET(350-400psi)"),
                new Vehicle("RCO_RUSCKO12(270)"),
                new Vehicle("FERNDALE(1630kg)"),
                new Vehicle("FLATBED"),
                new Vehicle("GIFU(750/450psi)"),
                new Boat("BOAT")
            };

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

            ModConsole.Print("[MOP] Main world objects loaded");

            // Initialize shops
            places = new List<Place>
            {
                new Yard(),
                new Teimo(),
                new RepairShop(),
                new Inspection()
            };

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
            Transform diskette = perajarvi.transform.Find("TerraceHouse/diskette(itemx)");
            if (diskette != null && diskette.parent != null)
                diskette.parent = null;
            
            diskette = perajarvi.transform.Find("TerraceHouse/diskette(itemx)");
            if (diskette != null && diskette.parent != null)
                diskette.parent = null;

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
            worldObjectList.Add("BirdTower", true);
            worldObjectList.Add("SwampColliders", true);

            ModConsole.Print("[MOP] Away from house world objects loaded");

            // Adding area check if Satsuma is in the inspection's area
            SatsumaInAreaCheck inspectionArea = GameObject.Find("INSPECTION").AddComponent<SatsumaInAreaCheck>();
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

            ModConsole.Print("[MOP] Loading rules...");
            foreach (ToggleRule v in RuleFiles.instance.ToggleRules)
            {
                switch (v.ToggleMode)
                {
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
                        if (MopSettings.IgnoreModVehicles) continue;

                        if (GameObject.Find(v.ObjectName) == null)
                        {
                            ModConsole.Error($"[MOP] Couldn't find vehicle {v.ObjectName}");
                            continue;
                        }

                        vehicles.Add(new Vehicle(v.ObjectName));
                        break;
                    case ToggleModes.VehiclePhysics:
                        if (MopSettings.IgnoreModVehicles) continue;

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

            ModConsole.Print("[MOP] Rules loading complete!");

            // Initialzie sector manager
            ActivateSectors();

            // Initialize the coroutines 
            currentLoop = LoopRoutine();
            StartCoroutine(currentLoop);
            StartCoroutine(ControlCoroutine());

            ModConsole.Print("<color=green>[MOP] MOD LOADED SUCCESFULLY!</color>");
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
            StopCoroutine(currentLoop);
            ToggleActiveAll();
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

                isPlayerAtYard = MopSettings.ActiveDistance == 0 ? Vector3.Distance(player.position, places[0].transform.position) < 100 
                    : Vector3.Distance(player.position, places[0].transform.position) < 100 * MopSettings.ActiveDistanceMultiplicationValue;

                // When player is in any of the sectors, MOP will act like the player is at yard.
                if (SectorManager.instance.PlayerInSector)
                {
                    // Safe check for when player uses NoClip to leave the sector.
                    // If player leaves the sector using NoClip, PlayerInSector toggle will be disabled.
                    if (!playerCharacterController.enabled && !MopFsmManager.IsPlayerInCar())
                    {
                        SectorManager.instance.PlayerInSector = false;
                        SectorManager.instance.ToggleActive(true);
                    }

                    isPlayerAtYard = true;
                }

                int half = worldObjectList.Count / 2;
                int i = 0;

                // Disable Satsuma engine renderer, if player is in Satsuma
                Satsuma.instance.ToggleEngineRenderers(!MopFsmManager.IsPlayerInSatsuma());
                yield return null;

                try
                {
                    // Go through the list worldObjectList list
                    for (i = 0; i < half; i++)
                    {
                        WorldObject worldObject = worldObjectList.Get(i);

                        // Should the object be disabled when the player leaves the house?
                        if (worldObject.AwayFromHouse)
                        {
                            if (worldObject.gameObject.name == "NPC_CARS" && SectorManager.instance.PlayerInSector)
                                continue;

                            worldObject.Toggle(!isPlayerAtYard);
                            continue;
                        }

                        // The object will be disables, if the player is in the range of that object.
                        worldObject.Toggle(IsEnabled(worldObject.transform, worldObject.Distance));
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, "CODE: 0-0");
                }

                yield return null;

                try
                {
                    for (; i < worldObjectList.Count; i++)
                    {
                        WorldObject worldObject = worldObjectList.Get(i);

                        // Should the object be disabled when the player leaves the house?
                        if (worldObject.AwayFromHouse)
                        {
                            if (worldObject.gameObject.name == "NPC_CARS" && SectorManager.instance.PlayerInSector)
                                continue;

                            worldObject.Toggle(!isPlayerAtYard);
                            continue;
                        }

                        // The object will be disables, if the player is in the range of that object.
                        worldObject.Toggle(IsEnabled(worldObject.transform, worldObject.Distance));
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, "CODE: 0-1");
                }

                // Safe mode prevents toggling elemenets that MAY case some issues (vehicles, items, etc.)
                if (MopSettings.SafeMode)
                {
                    yield return new WaitForSeconds(1);
                    continue;
                }

                // Vehicles
                try
                {
                    half = vehicles.Count / 2;
                    for (i = 0; i < half; i++)
                    {
                        if (vehicles[i] == null)
                        {
                            ModConsole.Print($"[MOP] Vehicle {i} has been skipped, because it's missing.");
                            continue;
                        }

                        float distance = Vector3.Distance(player.transform.position, vehicles[i].transform.position);
                        float toggleDistance = MopSettings.ActiveDistance == 0 
                            ? MopSettings.UnityCarActiveDistance : MopSettings.UnityCarActiveDistance * MopSettings.ActiveDistanceMultiplicationValue;
                        vehicles[i].ToggleUnityCar(IsVehicleEnabled(distance, toggleDistance));
                        vehicles[i].Toggle(IsVehicleEnabled(distance));
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, "CODE: 1-0");
                }

                yield return null;

                try
                {
                    for (; i < vehicles.Count; i++)
                    {
                        if (vehicles[i] == null)
                        {
                            ModConsole.Print($"[MOP] Vehicle {i} has been skipped, because it's missing.");
                            continue;
                        }

                        float distance = Vector3.Distance(player.transform.position, vehicles[i].transform.position);
                        float toggleDistance = MopSettings.ActiveDistance == 0 
                            ? MopSettings.UnityCarActiveDistance : MopSettings.UnityCarActiveDistance * MopSettings.ActiveDistanceMultiplicationValue;
                        vehicles[i].ToggleUnityCar(IsVehicleEnabled(distance, toggleDistance));
                        vehicles[i].Toggle(IsVehicleEnabled(distance));
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, "CODE: 1-1");
                }

                // Items (new)
                half = Items.instance.ItemsHooks.Count / 2;
                int full = Items.instance.ItemsHooks.Count;
                for (i = 0; i < full; i++)
                {
                    if (i % half == 0) yield return null;

                    // Safe check if somehow the i gets bigger than array length.
                    if (i >= Items.instance.ItemsHooks.Count) break;

                    try
                    {
                        if (Items.instance.ItemsHooks[i] == null || Items.instance.ItemsHooks[i].gameObject == null)
                        {
                            ModConsole.Print("[MOP] Removed one item from ItemHooks list, because it doesn't exist anymore " +
                                "(ignore if you have Bottle Recycling mod and you just sold the bottle/beer case).");

                            // Remove item at the current i
                            Items.instance.ItemsHooks.RemoveAt(i);

                            // Decrease the i by 1, because the List has shifted, so the items will not be skipped.
                            // Then continue.
                            i--;
                            half = Items.instance.ItemsHooks.Count / 2;
                            full = Items.instance.ItemsHooks.Count;
                            continue;
                        }

                        Items.instance.ItemsHooks[i].ToggleActive(IsEnabled(Items.instance.ItemsHooks[i].gameObject.transform));
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, "CODE: 2-0");
                    }
                }

                // Places
                try
                {
                    half = places.Count / 2;
                    for (i = 0; i < half; ++i)
                    {
                        places[i].ToggleActive(IsEnabled(places[i].transform));
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, "CODE: 3-0");
                }

                yield return null;

                try
                {
                    for (; i < places.Count; ++i)
                    {
                        places[i].ToggleActive(IsEnabled(places[i].transform));
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, "CODE: 3-1");
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
            if (SectorManager.instance != null && SectorManager.instance.PlayerInSector)
                toggleDistance = 30;

            return Vector3.Distance(player.transform.position, target.position) < toggleDistance * MopSettings.ActiveDistanceMultiplicationValue;
        }

        /// <summary>
        /// Same as IsEnabled, but used for vehicles only
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        bool IsVehicleEnabled(float distance, float toggleDistance = 200)
        {
            if (SectorManager.instance != null && SectorManager.instance.PlayerInSector)
                toggleDistance = 30;

            return distance < toggleDistance;
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
                        ModConsole.Error("[MOP] Restart attempt failed. Enabling Safe Mode.");
                        ModConsole.Error("[MOP] Please contact mod developer. Make sure you send output_log and last MOP crash log!");
                        MopSettings.SafeMode = true;
                        ToggleActiveAll();
                    }

                    restartTried = true;
                    ModConsole.Warning("[MOP] MOP has stopped working! Trying to restart it now...");
                    currentLoop = LoopRoutine();
                    StartCoroutine(currentLoop);
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

                // Places
                for (int i = 0; i < places.Count; i++)
                {
                    places[i].ToggleActive(true);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, "CODE: 90");
            }
        }

        /// <summary>
        /// Activate sectors.
        /// </summary>
        void ActivateSectors()
        {
            if (gameObject.GetComponent<SectorManager>() == null)
            {
                this.gameObject.AddComponent<SectorManager>();

                GameObject colliderCheck = new GameObject("MOP_PlayerCheck");
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
    }
}
