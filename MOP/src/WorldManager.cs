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

        public Transform player;

        // Vehicles
        List<Vehicle> vehicles;

        // Places
        Teimo teimo;
        RepairShop repairShop;
        Yard yard;

        // World Objects
        List<WorldObject> worldObjects;

        public WorldManager()
        {
            instance = this;

            // Initialize the worldObjects list
            worldObjects = new List<WorldObject>();

            // Looking for player and yard
            player = GameObject.Find("PLAYER").transform;

            // Loading vehicles
            vehicles = new List<Vehicle>();
            vehicles.Add(new Satsuma("SATSUMA(557kg, 248)"));
            vehicles.Add(new Vehicle("HAYOSIKO(1500kg, 250)"));
            vehicles.Add(new Vehicle("JONNEZ ES(Clone)"));
            vehicles.Add(new Vehicle("HAYOSIKO(1500kg, 250)"));
            vehicles.Add(new Vehicle("KEKMET(350-400psi)"));
            vehicles.Add(new Vehicle("RCO_RUSCKO12(270)"));
            vehicles.Add(new Vehicle("FERNDALE(1630kg)"));
            vehicles.Add(new Vehicle("FLATBED"));
            vehicles.Add(new Gifu("GIFU(750/450psi)"));

            // World Objects
            worldObjects.Add(new WorldObject("CABIN", 200));
            worldObjects.Add(new WorldObject("COTTAGE", 200));
            worldObjects.Add(new WorldObject("DANCEHALL", 200));
            worldObjects.Add(new WorldObject("INSPECTION", 200));
            worldObjects.Add(new WorldObject("LANDFILL", 200));
            worldObjects.Add(new WorldObject("PERAJARVI", 200));
            worldObjects.Add(new WorldObject("RYKIPOHJA", 200));
            worldObjects.Add(new WorldObject("SOCCER", 200));
            worldObjects.Add(new WorldObject("WATERFACILITY", 200));
            worldObjects.Add(new WorldObject("TREES1_COLL", 200));
            worldObjects.Add(new WorldObject("TREES2_COLL", 200));
            worldObjects.Add(new WorldObject("TREES3_COLL", 200));
            worldObjects.Add(new WorldObject("DRAGRACE", 1100));

            // Initialize shops
            teimo = new Teimo();
            repairShop = new RepairShop();
            yard = new Yard();

            Transform buildings = GameObject.Find("Buildings").transform;

            // Find house of Teimo and detach it from Perajarvi, so it can be loaded and unloaded separately
            // It shouldn't cause any issues, but that needs testing.
            GameObject perajarvi = GameObject.Find("PERAJARVI");
            perajarvi.transform.Find("HouseRintama4").parent = buildings;
            // Same for chicken house
            perajarvi.transform.Find("ChickenHouse").parent = buildings;

            // Chicken house (barn) close to player's house
            Transform playerChickenHouse = GameObject.Find("Buildings").transform.Find("ChickenHouse");
            playerChickenHouse.parent = null;

            // Fix for church wall. Changing it's parent ot NULL, so it will not be loaded or unloaded.
            // It used to be changed to CHURCH gameobject, 
            // but the Amis cars (yellow and grey cars) used to end up in the graveyard area.
            GameObject.Find("CHURCHWALL").transform.parent = null;

            // Fix for old house on the way from Perajarvi to Ventti's house (HouseOld5)
            perajarvi.transform.Find("HouseOld5").parent = buildings;

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

            // Possible fix for Jokke.
            // Needs testing
            Transform[] kiljuGuyChilds = GameObject.Find("KILJUGUY").transform.GetComponentsInChildren<Transform>();
            for (int i = 0; i < kiljuGuyChilds.Length; i++)
            {
                worldObjects.Add(new WorldObject(kiljuGuyChilds[i].gameObject));
            }

            // Removes the mansion from the Buildings, so the tires will not land under the mansion.
            GameObject.Find("autiotalo").transform.parent = null;

            //Things that should be enabled when out of proximity of the house
            worldObjects.Add(new WorldObject("NPC_CARS", awayFromHouse: true));
            worldObjects.Add(new WorldObject("RALLY", awayFromHouse: true));
            worldObjects.Add(new WorldObject("TRAFFIC", awayFromHouse: true));
            worldObjects.Add(new WorldObject("TRAIN", awayFromHouse: true));
            worldObjects.Add(new WorldObject("Buildings", awayFromHouse: true));
            worldObjects.Add(new WorldObject("TrafficSigns", awayFromHouse: true));
            worldObjects.Add(new WorldObject("ELEC_POLES", awayFromHouse: true));
            worldObjects.Add(new WorldObject("StreetLights", awayFromHouse: true));
            worldObjects.Add(new WorldObject("HUMANS", awayFromHouse: true));
            worldObjects.Add(new WorldObject("HayBales", true));
            worldObjects.Add(new WorldObject("TRACKFIELD", true));
            worldObjects.Add(new WorldObject("SkijumpHill", true));
            worldObjects.Add(new WorldObject("Factory", true));
            worldObjects.Add(new WorldObject("SWAMP", true));
            worldObjects.Add(new WorldObject("WHEAT", true));
            worldObjects.Add(new WorldObject("ROCKS", true));
            worldObjects.Add(new WorldObject("RAILROAD", true));
            worldObjects.Add(new WorldObject("AIRPORT", true));

            new Items();

            HookPreSaveGame();

            // Initialize the coroutines
            StartCoroutine(LoopRoutine());
            StartCoroutine(ControlCoroutine());
        }

        /// <summary>
        /// Looks for gamobject named SAVEGAME, and hooks PreSaveGame into them.
        /// </summary>
        void HookPreSaveGame()
        {
            GameObject[] saveGames = FindObjectsOfType<GameObject>().Where(obj => obj.name.Contains("SAVEGAME")).ToArray();

            //foreach (var obj in shitHouses)
            for (int i = 0; i < saveGames.Length; i++)
            {
                FsmHook.FsmInject(saveGames[i], "Mute audio", PreSaveGame);
            }
        }

        /// <summary>
        /// This void is initialized before the player decides to save the game.
        /// </summary>
        void PreSaveGame()
        {
            MopSettings.IsModActive = false;

            // World objects
            for (int i = 0; i < worldObjects.Count; i++)
            {
                worldObjects[i].ToggleActive(true);
            }

            // Vehicles
            for (int i = 0; i < vehicles.Count; i++)
            {
                vehicles[i].ToggleActive(true);
            }

            // Items
            for (int i = 0; i < Items.instance.ObjectHooks.Count; i++)
            {
                Items.instance.ObjectHooks[i].ToggleActive(true);
            }

            // Stores
            teimo.ToggleActive(true);
            repairShop.ToggleActive(true);
            yard.ToggleActive(true);
        }

        /// <summary>
        /// This coroutine runs
        /// </summary>
        IEnumerator LoopRoutine()
        {
            MopSettings.IsModActive = true;
            while (MopSettings.IsModActive)
            {
                ticks += 1;
                if (ticks > 100)
                    ticks = 0;

                int half = worldObjects.Count / 2;
                int i = 0;


                try
                {
                    // Go through the list worldObjects list
                    for (i = 0; i < half; i++)
                    {
                        // Should the object be disabled when the player leaves the house?
                        if (worldObjects[i].AwayFromHouse)
                        {
                            worldObjects[i].ToggleActive(player.Distance(yard.transform) > 100);
                            continue;
                        }

                        // The object will be disables, if the player is in the range of that object.
                        worldObjects[i].ToggleActive(ToEnable(worldObjects[i].transform, worldObjects[i].Distance));
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.New(ex);
                }

                yield return null;

                try
                {
                    for (; i < worldObjects.Count; i++)
                    {
                        // Should the object be disabled when the player leaves the house?
                        if (worldObjects[i].AwayFromHouse)
                        {
                            worldObjects[i].ToggleActive(player.Distance(yard.transform) > 100);
                            continue;
                        }

                        // The object will be disables, if the player is in the range of that object.
                        worldObjects[i].ToggleActive(ToEnable(worldObjects[i].transform, worldObjects[i].Distance));
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.New(ex);
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
                            if (vehicles[i] == null || vehicles[i].gm == null)
                            {
                                ModConsole.Print("Vehicle " + i + " has been skipped, because an error occured.");
                                continue;
                            }

                            float distance = player.Distance(vehicles[i].transform);
                            vehicles[i].ToggleUnityCar(ToEnable(distance, 5));

                            // If the vehicle is Gifu, execute the ToggleActive from gifuScript
                            if (vehicles[i].gifuScript != null)
                            {
                                vehicles[i].gifuScript.ToggleActive(ToEnable(distance));
                                continue;
                            }

                            // If the vehicle is Satsuma, execute the ToggleActive from satsumaScript
                            if (vehicles[i].satsumaScript != null)
                            {
                                vehicles[i].satsumaScript.ToggleActive(ToEnable(distance));
                                continue;
                            }

                            vehicles[i].ToggleActive(ToEnable(distance));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.New(ex);
                    }

                    yield return null;

                    try
                    {
                        for (; i < vehicles.Count; i++)
                        {
                            if (vehicles[i] == null || vehicles[i].gm == null)
                            {
                                ModConsole.Print("Vehicle " + i + " has been skipped, because an error occured.");
                                continue;
                            }

                            float distance = player.Distance(vehicles[i].transform);
                            vehicles[i].ToggleUnityCar(ToEnable(distance, 5));

                            // If the vehicle is Gifu, execute the ToggleActive from gifuScript
                            if (vehicles[i].gifuScript != null)
                            {
                                vehicles[i].gifuScript.ToggleActive(ToEnable(distance));
                                continue;
                            }

                            // If the vehicle is Satsuma, execute the ToggleActive from satsumaScript
                            if (vehicles[i].satsumaScript != null)
                            {
                                vehicles[i].satsumaScript.ToggleActive(ToEnable(distance));
                                continue;
                            }

                            vehicles[i].ToggleActive(ToEnable(distance));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.New(ex);
                    }
                }

                // Shop items
                if (MopSettings.ToggleItems)
                {
                    try
                    {
                        half = Items.instance.ObjectHooks.Count / 2;
                        for (i = 0; i < half; ++i)
                        {
                            if (Items.instance.ObjectHooks[i] == null || Items.instance.ObjectHooks[i].gm == null)
                            {
                                ModConsole.Print("One minor object has been skipped");
                                continue;
                            }

                            Items.instance.ObjectHooks[i].ToggleActive(ToEnable(Items.instance.ObjectHooks[i].gm.transform));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.New(ex);
                    }

                    yield return null;

                    try
                    {
                        for (; i < Items.instance.ObjectHooks.Count; ++i)
                        {
                            if (Items.instance.ObjectHooks[i] == null || Items.instance.ObjectHooks[i].gm == null)
                            {
                                ModConsole.Print("One minor object has been skipped");
                                continue;
                            }

                            Items.instance.ObjectHooks[i].ToggleActive(ToEnable(Items.instance.ObjectHooks[i].gm.transform));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.New(ex);
                    }
                }

                try
                {
                    teimo.ToggleActive(ToEnable(teimo.transform));
                    repairShop.ToggleActive(ToEnable(repairShop.transform));
                    yard.ToggleActive(ToEnable(yard.transform, 300));
                }
                catch (Exception ex)
                {
                    ErrorHandler.New(ex);
                }

                yield return new WaitForSeconds(1);
            }
        }

        /// <summary>
        /// Checks if the object is supposed to be enabled by calculating the distance between player and target.
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <param name="toggleDistance">Distance below which the object should be enabled (default 200 units).</param>
        bool ToEnable(Transform target, float toggleDistance = 200)
        {
            return player.Distance(target) < toggleDistance * MopSettings.ActiveDistanceMultiplicationValue;
        }

        bool ToEnable(float distance, float toggleDistance = 200)
        {
            return distance < toggleDistance * MopSettings.ActiveDistanceMultiplicationValue;
        }

        int ticks;
        int lastTick;
        bool restartRetried;

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
                    if (restartRetried)
                    {
                        ModConsole.Print("Restart attempt failed. Safe Mode will be enabled this time.\n\n" +
                            "You can try and disable some objects (like Vehicles and Store Items) from being disabled.");
                        MopSettings.SafeMode = true;
                    }

                    restartRetried = true;
                    ModConsole.Error("MOP has stopped working. Trying to restart the now...\n\n" +
                        "If the issue will still occure, please turn on output_log.txt in MSC Mod Loader settings, " +
                        "and check output_log.txt in MSC folder.\n\noutput_log.txt is located in mysummercar_Data " +
                        "in My Summer Car folder.");
                    StartCoroutine(LoopRoutine());
                }

                restartRetried = false;
                lastTick = ticks;
            }
        }
    }
}
