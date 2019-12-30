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

        public Transform Player { get; private set; }

        // Vehicles
        List<Vehicle> vehicles;

        // Places
        Teimo teimo;
        RepairShop repairShop;
        Yard yard;

        // World Objects
        WorldObjects worldObjects;

        // Area Checks
        SatsumaInAreaCheck inspectionArea;

        public WorldManager()
        {
            instance = this;

            // Initialize the worldObjects list
            worldObjects = new WorldObjects();

            // Looking for player and yard
            Player = GameObject.Find("PLAYER").transform;

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

            // Drivable Fury mod
            if (CompatibilityManager.instance.DrivableFury)
            {
                vehicles.Add(new Fury("FURY(1630kg)"));
            }

            // Second Ferndale
            if (CompatibilityManager.instance.SecondFerndale)
            {
                vehicles.Add(new Vehicle("SECONDFERNDALE(1630kg)"));
            }

            // World Objects
            worldObjects.Add("CABIN", 200);
            worldObjects.Add("COTTAGE", 400);
            worldObjects.Add("DANCEHALL", 200);
            worldObjects.Add("INSPECTION", 200);
            //worldObjects.Add(new WorldObject("LANDFILL", 200));
            worldObjects.Add("PERAJARVI", 200);
            worldObjects.Add("RYKIPOHJA", 200);
            worldObjects.Add("SOCCER", 200);
            worldObjects.Add("WATERFACILITY", 200);
            worldObjects.Add("DRAGRACE", 1100);
            worldObjects.Add("BOAT", 200);

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
            Transform playerChickenHouse = GameObject.Find("Buildings").transform.Find("ChickenHouse");
            playerChickenHouse.parent = null;

            // Fix for church wall. Changing it's parent to NULL, so it will not be loaded or unloaded.
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

            // Removes the mansion from the Buildings, so the tires will not land under the mansion.
            GameObject.Find("autiotalo").transform.parent = null;

            // Fix for cottage items disappearing when moved
            GameObject.Find("coffee pan(itemx)").transform.parent = null;
            GameObject.Find("lantern(itemx)").transform.parent = null;
            GameObject.Find("coffee cup(itemx)").transform.parent = null;
            GameObject.Find("camera(itemx)").transform.parent = null;
            GameObject.Find("fireworks bag(itemx)").transform.parent = null;

            ModConsole.Print("[MOP] Finished applying fixes");

            //Things that should be enabled when out of proximity of the house
            worldObjects.Add("NPC_CARS", awayFromHouse: true);
            worldObjects.Add("TRAFFIC", awayFromHouse: true);
            worldObjects.Add("TRAIN", awayFromHouse: true);
            worldObjects.Add("Buildings", awayFromHouse: true);
            worldObjects.Add("TrafficSigns", awayFromHouse: true);
            worldObjects.Add("StreetLights", awayFromHouse: true);
            worldObjects.Add("HUMANS", awayFromHouse: true);
            worldObjects.Add("HayBales", true);
            worldObjects.Add("TRACKFIELD", true);
            worldObjects.Add("SkijumpHill", true);
            worldObjects.Add("Factory", true);
            worldObjects.Add("SWAMP", true);
            worldObjects.Add("WHEAT", true);
            worldObjects.Add("ROCKS", true);
            worldObjects.Add("RAILROAD", true);
            worldObjects.Add("AIRPORT", true);

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
                worldObjects.Add("tv(Clo01)", 200, true);
                worldObjects.Add("chair(Clo02)", 200, true);
                worldObjects.Add("chair(Clo05)", 200, true);
                worldObjects.Add("bench(Clo01)", 200, true);
                worldObjects.Add("bench(Clo02)", 200, true);
                worldObjects.Add("table(Clo02)", 200, true);
                worldObjects.Add("table(Clo03)", 200, true);
                worldObjects.Add("table(Clo04)", 200, true);
                worldObjects.Add("table(Clo05)", 200, true);
                worldObjects.Add("desk(Clo01)", 200, true);
                worldObjects.Add("arm chair(Clo01)", 200, true);

                ModConsole.Print("[MOP] Jokke's furnitures found and loaded");
            }

            // Initialize Items class
            new Items();

            ModConsole.Print("[MOP] Items class loaded");

            HookPreSaveGame();

            ModConsole.Print("[MOP] Pre Save Game Hook done");

            // Initialize the coroutines
            StartCoroutine(LoopRoutine());
            StartCoroutine(ControlCoroutine());

            ModConsole.Print("[MOP] Mod loaded succesfully!");
        }

        /// <summary>
        /// Looks for gamobject named SAVEGAME, and hooks PreSaveGame into them.
        /// </summary>
        void HookPreSaveGame()
        {
            GameObject[] saveGames = (Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                .Where(obj => obj.name.Contains("SAVEGAME") && obj.transform.parent.gameObject.name != "JAIL").ToArray();

            try
            {
                for (int i = 0; i < saveGames.Length; i++)
                {
                    bool useInnactiveFix = false;

                    if (!saveGames[i].activeSelf)
                    {
                        useInnactiveFix = true;
                        saveGames[i].SetActive(true);
                    }

                    FsmHook.FsmInject(saveGames[i], "Mute audio", PreSaveGame);
                    GameObject newSavePivot = new GameObject("NewSavePivot");
                    newSavePivot.transform.position = saveGames[i].transform.position;
                    newSavePivot.transform.parent = saveGames[i].transform;

                    if (useInnactiveFix)
                    {
                        saveGames[i].SetActive(false);
                    }
                }

                // fix for jail savegame
                if (GameObject.Find("JAIL") != null)
                    FsmHook.FsmInject(GameObject.Find("JAIL/SAVEGAME"), "Mute audio", PreSaveGame);
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
            try
            {
                // World objects
                for (int i = 0; i < worldObjects.Count; i++)
                {
                    worldObjects.Get(i).Toggle(true);
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
                ErrorHandler.New(ex);
            }
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
                        if (worldObjects.Get(i).AwayFromHouse)
                        {
                            worldObjects.Get(i).Toggle(Player.GetDistance(yard.transform) > 100);
                            continue;
                        }

                        // Fix for inspection area being unloaded after the successfull inspection,
                        // making game not save the car inspection status.
                        if (worldObjects.Get(i).gameObject.name == "INSPECTION" && inspectionArea.InspectionPreventUnload)
                            continue;

                        // The object will be disables, if the player is in the range of that object.
                        worldObjects.Get(i).Toggle(IsEnabled(worldObjects.Get(i).transform, worldObjects.Get(i).Distance));
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
                        if (worldObjects.Get(i).AwayFromHouse)
                        {
                            worldObjects.Get(i).Toggle(Player.GetDistance(yard.transform) > 100);
                            continue;
                        }

                        // Fix for inspection area being unloaded after the successfull inspection,
                        // making game not save the car inspection status.
                        if (worldObjects.Get(i).gameObject.name == "INSPECTION" && inspectionArea.InspectionPreventUnload)
                            continue;

                        // The object will be disables, if the player is in the range of that object.
                        worldObjects.Get(i).Toggle(IsEnabled(worldObjects.Get(i).transform, worldObjects.Get(i).Distance));
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
                            if (vehicles[i] == null || vehicles[i].gameObject == null)
                            {
                                ModConsole.Print("Vehicle " + i + " has been skipped, because an error occured.");
                                continue;
                            }

                            float distance = Player.GetDistance(vehicles[i].transform);
                            vehicles[i].ToggleUnityCar(IsEnabled(distance, MopSettings.UnityCarActiveDistance));

                            vehicles[i].Toggle(IsEnabled(distance));
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
                            if (vehicles[i] == null || vehicles[i].gameObject == null)
                            {
                                ModConsole.Print("Vehicle " + i + " has been skipped, because an error occured.");
                                continue;
                            }

                            float distance = Player.GetDistance(vehicles[i].transform);
                            vehicles[i].ToggleUnityCar(IsEnabled(distance, MopSettings.UnityCarActiveDistance));

                            vehicles[i].Toggle(IsEnabled(distance));
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
                        half = Items.instance.ItemsHooks.Count / 2;
                        for (i = 0; i < half; ++i)
                        {
                            if (Items.instance.ItemsHooks[i] == null || Items.instance.ItemsHooks[i].gameObject == null)
                            {
                                ModConsole.Print("One minor object has been skipped");
                                continue;
                            }

                            Items.instance.ItemsHooks[i].ToggleActive(IsEnabled(Items.instance.ItemsHooks[i].gameObject.transform));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.New(ex);
                    }

                    yield return null;

                    try
                    {
                        for (; i < Items.instance.ItemsHooks.Count; ++i)
                        {
                            if (Items.instance.ItemsHooks[i] == null || Items.instance.ItemsHooks[i].gameObject == null)
                            {
                                ModConsole.Print("One minor object has been skipped");
                                continue;
                            }

                            Items.instance.ItemsHooks[i].ToggleActive(IsEnabled(Items.instance.ItemsHooks[i].gameObject.transform));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.New(ex);
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
        private bool IsEnabled(Transform target, float toggleDistance = 200)
        {
            return Player.GetDistance(target) < toggleDistance * MopSettings.ActiveDistanceMultiplicationValue;
        }

        private bool IsEnabled(float distance, float toggleDistance = 200)
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
    }
}
