// Modern Optimization Plugin
// Copyright(C) 2019-2022 Athlon

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOP.Managers;
using MOP.Common;
using MOP.Common.Enumerations;
using MOP.FSM;
using MOP.FSM.Actions;
using MOP.Items;
using MOP.Vehicles;
using MOP.Vehicles.Cases;
using MOP.Helpers;
using MOP.Rules;
using MOP.Rules.Types;
using MOP.WorldObjects;

namespace MOP
{
    /// <summary>
    /// Controls all objects in the game world.
    /// </summary>
    internal class Hypervisor : MonoBehaviour
    {
        static Hypervisor instance;
        public static Hypervisor Instance => instance;

        Transform player;

        // Managers
        VehicleManager vehicleManager;
        PlaceManager placeManager;
        WorldObjectManager worldObjectManager;
        private bool isPlayerAtYard;
        private bool inSectorMode;

        #region Loading Variables
        readonly CharacterController playerController;
        bool itemInitializationDelayDone;
        bool isFinishedCheckingSatsuma;

        const int WaitForPhysicsToSettleTime = 2; // How many seconds to wait for the physics of MSC to "settle".
        const int WaitForSatsumaCheckTime = 10; // How many seconds will the MOP wait for SatsumaIsLoaded check to finish.
        const int LoadScreenWorkaroundTime = 20; // How long the game will wait for the loading to finish (if it fails).

        readonly LoadScreen loadScreen;
        #endregion

        readonly Queue<ItemBehaviour> itemsToRemove = new Queue<ItemBehaviour>();
        readonly Queue<ItemBehaviour> itemsToEnable = new Queue<ItemBehaviour>();
        
        private GameObject computerSystem;
        private float distance;
        private float toggleDistance;

        readonly string[] trafficVehicleRoots = { "NPC_CARS", "TRAFFIC", "RALLY" };
        public string[] TrafficVehicleRoots => trafficVehicleRoots;
        GameObject traffic, trafficHighway, trafficDirt;

        private Hypervisor()
        {
            instance = this;

            if (RulesManager.Instance == null)
            {
                ModConsole.LogError("[MOP] Rule Files haven't been loaded! Please exit to the main menu and start the game again.");
                return;
            }

            MopSettings.LoadedOnce = true;

            // Load loadscreen.
            try
            {
                GameObject loadscreenObj = GameObject.Instantiate(MOP.LoadAssetBundle());
                loadScreen = loadscreenObj.AddComponent<LoadScreen>();
                loadScreen.Activate();
                loadScreenWorkaround = InfiniteLoadscreenWorkaround();
                StartCoroutine(loadScreenWorkaround);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "LOAD_SCREEN_ERROR");
            }

            // Disable player controller and pretend like he's in the main menu.
            playerController = GameObject.Find("PLAYER").GetComponent<CharacterController>();
            playerController.enabled = false;
            FsmManager.PlayerInMenu = true;

            // Disable rule files if user wants it.
            if (!RulesManager.Instance.LoadRules)
            {
                RulesManager.Instance.ResetLists();
            }

            ExceptionManager.SessionTimeStart = DateTime.Now;

#if DEBUG
            ToggleDebugMode();
#endif

            // Start the delayed initialization routine
            StartCoroutine(DelayedInitializaitonRoutine());
        }

        #region MOP Initialization
        IEnumerator DelayedInitializaitonRoutine()
        {
            for (int i = 0; i < 200; i++)
            {
                yield return null;
            }

            // Check if the Satsuma has been loaded completely by the game.
            // If not, restart the scene at least once.
            CheckIfSatsumaIsLoaded();

            // Wait either for the isFinishedCheckingSatsuma to be set to true,
            // or wait WaitForSastumaCheckTime seconds to break.
            for (int i = 0; i < WaitForSatsumaCheckTime; ++i)
            {
                if (isFinishedCheckingSatsuma)
                    break;

                yield return new WaitForSeconds(1);
            }

            Initialize();
        }

        /// <summary>
        /// Performs a check that makes sure that Satsuma is loaded by the game.
        /// </summary>
        void CheckIfSatsumaIsLoaded()
        {
            bool satsumaIsLoaded = false;

            if (SaveManager.IsCarAssembledWithMSCEditor())
            {
                MSCLoader.ModUI.ShowMessage("MSCEditor (or another third-party software) has been used to assemble the car. " +
                    "This might cause MOP to not work as intended.", "MOP - Warning");
            }
            else
            {
                try
                {
                    satsumaIsLoaded = SaveManager.IsSatsumaLoadedCompletely();
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, true, "SATSUMA_IS_LOADED_ERROR");
                }

                // For testing purposes.
                if (MopSettings.ForceLoadRestart)
                {
                    satsumaIsLoaded = false;
                }

                if (!satsumaIsLoaded)
                {
                    if (MopSettings.GameFixStatus == GameFixStatus.None)
                    {
                        StartCoroutine(GameRestartCoroutine());
                        return;
                    }

                    // Fix already has been attempted? Show error!
                    MSCLoader.ModUI.ShowMessage("Satsuma has not been fully loaded by the game!\n\n" +
                                                "Consider restarting the game in order to avoid any issues.",
                                                "MOP");
                }
            }

            isFinishedCheckingSatsuma = true;
        }

        /// <summary>
        /// This routine restarts the game. Called by CheckIfSatsumaIsLoaded, after it detects that the Satsuma has not been loaded fully.
        /// </summary>
        /// <returns></returns>
        IEnumerator GameRestartCoroutine()
        {
#if !PRO
            // On MSCLoader, we must wait for the mod loader to finish, otherwise it will break.
            GameObject mscloaderLoadscreen = GameObject.Find("MSCLoader Canvas loading").transform.Find("MSCLoader loading dialog").gameObject;
            ModConsole.Log("[MOP] Waiting for the MSCLoader to finish to load...");

            while (mscloaderLoadscreen.activeSelf)
                yield return null;
#endif
            yield return null;

            MopSettings.GameFixStatus = GameFixStatus.DoFix;
            ModConsole.Log("[MOP] Attempting to restart the scene...");
            Application.LoadLevel(1);
        }

        void Initialize()
        {
            ModConsole.Log("[MOP] Loading MOP...");

            // Initialize the worldObjectManager list
            worldObjectManager = new WorldObjectManager();

            // Looking for player and yard
            player = GameObject.Find("PLAYER").transform;

            // Add GameFixes MonoBehaviour.
            try
            {
                gameObject.AddComponent<GameFixes>();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, $"GAME_FIXES_INITIALIZAITON | {ex}");
            }

            // Loading vehicles
            vehicleManager = new VehicleManager();

            // World Objects
            try
            {
                var cabin = worldObjectManager.Add("CABIN", DisableOn.Distance | DisableOn.IgnoreInQualityMode);
                cabin.MinimumToggleDistance = 300;
                var cottage = worldObjectManager.Add("COTTAGE", DisableOn.Distance, 400);
                cottage.MinimumToggleDistance = 400;
                worldObjectManager.Add("DANCEHALL", DisableOn.Distance | DisableOn.IgnoreInQualityMode, 500);
                var perajarvi = worldObjectManager.Add("PERAJARVI", DisableOn.Distance | DisableOn.IgnoreInQualityMode, 600);
                perajarvi.MinimumToggleDistance = 600;
                worldObjectManager.Add("SOCCER", DisableOn.Distance);
                var waterfacility = worldObjectManager.Add("WATERFACILITY", DisableOn.Distance, 300);
                waterfacility.MinimumToggleDistance = 300;
                worldObjectManager.Add("StrawberryField", DisableOn.Distance, 400);
                worldObjectManager.Add("MAP/Buildings/DINGONBIISI", DisableOn.Distance | DisableOn.IgnoreInBalancedAndAbove, 400);
                worldObjectManager.Add("RALLY/PartsSalesman", DisableOn.Distance, 400);
                worldObjectManager.Add("LakeSmallBottom1", DisableOn.Distance, 500);
                worldObjectManager.Add("machine", DisableOn.Distance, 200, silent: true);

                SkidmarkObject skidmark = new SkidmarkObject(GameObject.Find("Skidmarks"), 0);
                worldObjectManager.Add(skidmark);

                ModConsole.Log("[MOP] World objects (1) loaded");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "WORLD_OBJECTS_1_INITIALIZAITON_FAIL");
            }

            // Initialize places.
            placeManager = new PlaceManager();

            // Fixes
            GameFixes.Instance.MainFixes();

            //Things that should be enabled when out of proximity of the house
            try
            {
                worldObjectManager.Add("NPC_CARS", DisableOn.PlayerInHome);
                worldObjectManager.Add("TRAFFIC", DisableOn.PlayerInHome);
                worldObjectManager.Add("VehiclesHighway", DisableOn.PlayerInHome | DisableOn.DoNotEnableWhenLeavingHome);
                worldObjectManager.Add("VehiclesDirtRoad", DisableOn.PlayerInHome);
                worldObjectManager.Add("TRAIN", DisableOn.PlayerInHome | DisableOn.IgnoreInQualityMode);
                worldObjectManager.Add("Buildings", DisableOn.PlayerInHome);
                worldObjectManager.Add("TrafficSigns", DisableOn.PlayerInHome);
                worldObjectManager.Add("StreetLights", DisableOn.PlayerInHome);
                worldObjectManager.Add("HUMANS", DisableOn.PlayerInHome);
                worldObjectManager.Add("TRACKFIELD", DisableOn.PlayerInHome);
                worldObjectManager.Add("SkijumpHill", DisableOn.PlayerInHome | DisableOn.IgnoreInQualityMode);
                worldObjectManager.Add("Factory", DisableOn.PlayerInHome);
                worldObjectManager.Add("WHEAT", DisableOn.PlayerInHome);
                worldObjectManager.Add("RAILROAD", DisableOn.PlayerInHome);
                worldObjectManager.Add("AIRPORT", DisableOn.PlayerInHome);
                worldObjectManager.Add("RAILROAD_TUNNEL", DisableOn.PlayerInHome);
                worldObjectManager.Add("PierDancehall", DisableOn.PlayerInHome);
                worldObjectManager.Add("PierRiver", DisableOn.PlayerInHome);
                worldObjectManager.Add("PierStore", DisableOn.PlayerInHome);
                worldObjectManager.Add("BRIDGE_dirt", DisableOn.PlayerInHome);
                worldObjectManager.Add("BRIDGE_highway", DisableOn.PlayerInHome);
                worldObjectManager.Add("BirdTower", DisableOn.Distance, 400);
                worldObjectManager.Add("RYKIPOHJA", DisableOn.PlayerInHome);
                worldObjectManager.Add("COMPUTER", DisableOn.PlayerAwayFromHome, silent: true);

                ModConsole.Log("[MOP] World objects (2) loaded");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "WORLD_OBJECTS_2_INITIALIZAITON_FAIL");
            }

            // Adding area check if Satsuma is in the inspection's area
            try
            {
                SatsumaInArea inspectionArea = GameObject.Find("INSPECTION").AddComponent<SatsumaInArea>();
                inspectionArea.Initialize(new Vector3(20, 20, 20));
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_AREA_CHECK_INSPECTION_FAIL");
            }

            // Check for when Satsuma is on the lifter
            try
            {
                SatsumaInArea lifterArea = GameObject.Find("REPAIRSHOP/Lifter/Platform").AddComponent<SatsumaInArea>();
                lifterArea.Initialize(new Vector3(5, 5, 5));
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_AREA_CHECK_REPAIRSHOP_FAIL");
            }

            // Area for the parc ferme.
            try
            {
                GameObject parcFermeTrigger = new GameObject("MOP_ParcFermeTrigger");
                parcFermeTrigger.transform.parent = GameObject.Find("RALLY").transform.Find("Scenery");
                parcFermeTrigger.transform.position = new Vector3(-1383f, 3f, 1260f);
                SatsumaInArea parcFerme = parcFermeTrigger.AddComponent<SatsumaInArea>();
                parcFerme.Initialize(new Vector3(41, 12, 35));
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "PARC_FERME_TRIGGER_FAIL");
            }

            ModConsole.Log("[MOP] Satsuma triggers loaded");

            // Jokke's furnitures.
            // Only renderers are going to be toggled.
            try
            {
                if (GameObject.Find("tv(Clo01)"))
                {
                    string[] furnitures = { "tv(Clo01)", "chair(Clo02)", "chair(Clo05)", "bench(Clo01)",
                                            "bench(Clo02)", "table(Clo02)", "table(Clo03)", "table(Clo04)",
                                            "table(Clo05)", "desk(Clo01)", "arm chair(Clo01)" };

                    foreach (string furniture in furnitures)
                    {
                        GameObject g = GameObject.Find(furniture);
                        if (g)
                        {
                            g.transform.parent = null;
                            worldObjectManager.Add(g, DisableOn.Distance, 100, ToggleModes.Renderer);
                        }
                    }

                    ModConsole.Log("[MOP] Jokke's furnitures found and loaded");
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "JOKKE_FURNITURE_ERROR");
            }

            // Haybales.
            // First we null out the prevent it from reloading the position of haybales.
            try
            {
                GameObject haybalesParent = GameObject.Find("JOBS/HayBales");
                if (haybalesParent != null)
                {
                    haybalesParent.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    // And now we add all child haybale to world objects.
                    foreach (Transform haybale in haybalesParent.transform.GetComponentInChildren<Transform>())
                    {
                        worldObjectManager.Add(haybale.gameObject.name, DisableOn.Distance | DisableOn.IgnoreInQualityMode, 120);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "HAYBALES_FIX_ERROR");
            }

            // Logwalls
            try
            {
                foreach (GameObject wall in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "LogwallLarge"))
                {
                    var logWall = worldObjectManager.Add(wall, DisableOn.Distance, 300);
                    logWall.MinimumToggleDistance = 200;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "LOGWALL_LOAD_ERROR");
            }

            // Perajarvi Church.
            try
            {
                if (MopSettings.Mode != PerformanceMode.Performance)
                {
                    GameObject church = GameObject.Find("PERAJARVI").transform.Find("CHURCH").gameObject;
                    church.transform.parent = null;
                    GameObject churchLOD = church.transform.Find("LOD").gameObject;
                    church.GetComponent<PlayMakerFSM>().enabled = false;
                    var churchObject = worldObjectManager.Add(churchLOD, DisableOn.Distance, 300);
                    churchObject.MinimumToggleDistance = 200;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "CHURCH_LOD_ERROR");
            }

            // Lake houses.
            try
            {
                if (MopSettings.Mode == PerformanceMode.Quality)
                {
                    GameObject.Find("PERAJARVI").transform.Find("TerraceHouse").transform.parent = null;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "LAKE_HOUSE_ERROR");
            }

            // VehiclesHighway renderers.
            try
            {
                Transform vehiclesHighway = GameObject.Find("TRAFFIC").transform.Find("VehiclesHighway");
                foreach (var f in vehiclesHighway.GetComponentsInChildren<Transform>(true).Where(f => f.parent == vehiclesHighway))
                {
                    var traffic = worldObjectManager.Add(f.gameObject, DisableOn.Distance, 600, ToggleModes.MultipleRenderers);
                    traffic.MinimumToggleDistance = 400;
                }

                // Also we gonna fix the lag on initial traffic load.
                vehiclesHighway.gameObject.SetActive(true);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TRAFFIC_VEHICLES_ERROR");
            }

            // FITTAN renderers.
            try
            {
                var fittan = worldObjectManager.Add(GameObject.Find("TRAFFIC").transform.Find("VehiclesDirtRoad/Rally/FITTAN").gameObject, DisableOn.Distance, 600, ToggleModes.MultipleRenderers);
                fittan.MinimumToggleDistance = 400;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "FITTAN_RENDERERS_ERROR");
            }

            // Cabin: do not disable Cabin's house and burned house on Quality mode.
            try
            {
                if (MopSettings.Mode >= PerformanceMode.Quality)
                {
                    Transform cabin = GameObject.Find("CABIN").transform;
                    cabin.Find("Cabin").parent = null;
                    cabin.Find("BurntHouse").parent = null;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "CABIN_DETAILS_QUALITY");
            }

            // Cottage: Do not disable main renderer in Quality mode.
            try
            {
                if (MopSettings.Mode >= PerformanceMode.Quality)
                {
                    Transform cottage = GameObject.Find("COTTAGE").transform;
                    cottage.Find("MESH").parent = null;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "CABIN_DETAILS_QUALITY");
            }

            // Initialize Items class
            try
            {
                new ItemsManager();
                ItemsManager.Instance.Initialize();
                ModConsole.Log("[MOP] Items class initialized");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "ITEMS_CLASS_ERROR");
            }

            try
            {
                DateTime now = DateTime.Now;
                if (now.Day == 1 && now.Month == 4)
                {
                    GameObject fpsObject = GameObject.Find("GUI").transform.Find("HUD/FPS/HUDValue").gameObject;
                    PlayMakerFSM[] fsms = fpsObject.GetComponents<PlayMakerFSM>();
                    foreach (var fsm in fsms)
                        fsm.enabled = false;
                    fpsObject.GetComponent<TextMesh>().text = "99999999 :)";
                    fpsObject.transform.Find("HUDValueShadow").GetComponent<TextMesh>().text = "99999999 :)";
                }
            }
            catch { }

            HookPreSaveGame();

            ModConsole.Log("[MOP] Loading rules...");
            foreach (ToggleRule v in RulesManager.Instance.GetList<ToggleRule>())
            {
                try
                {
                    GameObject g = GameObject.Find(v.ObjectName);
                    if (g == null)
                    {
                        ModConsole.LogError($"[MOP] Couldn't find {v.ToggleMode} {v.ObjectName}");
                        continue;
                    }

                    if (v.ToggleMode == ToggleModes.Simple || v.ToggleMode == ToggleModes.Renderer)
                    {
                        worldObjectManager.Add(g, DisableOn.Distance, 200, v.ToggleMode);
                    }
                    else if (v.ToggleMode == ToggleModes.Item)
                    {
                        if (g.GetComponent<ItemBehaviour>() == null)
                        {
                            g.AddComponent<ItemBehaviour>();
                        }
                    }
                    else if (v.ToggleMode == ToggleModes.Vehicle || v.ToggleMode == ToggleModes.VehiclePhysics)
                    {
                        Vehicle veh = new Vehicle(v.ObjectName);
                        vehicleManager.Add(veh);
                        if (v.ToggleMode == ToggleModes.VehiclePhysics)
                        {
                            veh.Toggle = veh.ToggleUnityCar;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "TOGGLE_RULES_LOAD_ERROR");
                }
            }

            foreach (ChangeParentRule rule in RulesManager.Instance.GetList<ChangeParentRule>())
            {
                try
                {
                    GameObject obj = GameObject.Find(rule.ObjectName);
                    if (obj == null)
                    {
                        throw new Exception($"Object {rule.ObjectName} doesn't exist.");
                    }

                    if (rule.NewParentName.ToLower() == "null")
                    {
                        obj.transform.parent = null;
                    }
                    else
                    {
                        GameObject parent = GameObject.Find(rule.NewParentName);
                        if (parent == null)
                        {
                            throw new Exception($"Parent {rule.NewParentName} doesn't exist.");
                        }

                        obj.transform.parent = parent.transform;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "CHANGE_PARENT_RULE_ERROR");
                }
            }

            ModConsole.Log("[MOP] Rules loading complete!");

            // Initialzie sector manager
            try
            {
                gameObject.AddComponent<SectorManager>();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "SECTOR_MANAGER_ERROR");
            }

            // Add DynamicDrawDistance component.
            try
            {
                gameObject.AddComponent<DynamicDrawDistance>();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "DYNAMIC_DRAW_DISTANCE_LOAD_ERROR");
            }

            if (MopSettings.Mode != PerformanceMode.Safe)
            {
                try
                {
                    ToggleAll(false, ToggleAllMode.OnLoad);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "TOGGLING_ALL_ERROR");
                }
            }

            // Locate computer system.
            try
            {
                GameObject computer = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "COMPUTER");
                if (computer != null)
                {
                    computerSystem = computer.transform.Find("SYSTEM").gameObject;
                    GameObject.Find("Systems").transform.Find("OptionsMenu").gameObject.AddComponent<MopPauseMenuHandler>();
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "COMPUTER_SYSTEM_ERROR");
            }

            try
            {
                traffic = worldObjectManager.Get("TRAFFIC");
                trafficDirt = worldObjectManager.Get("VehiclesDirtRoad");
                trafficHighway = worldObjectManager.Get("VehiclesHighway");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TRAFFIC_LOOKUP_ERROR");
            }

            // Initialize the coroutines.
            Startup();

            // If generate-list command is set to true, generate the list of items that are disabled by MOP.
            if (MopSettings.GenerateToggledItemsListDebug)
            {
                ToggledItemsListGenerator.CreateWorldList(WorldObjectManager.Instance.GetAll);
                ToggledItemsListGenerator.CreateVehicleList(VehicleManager.Instance.GetAll);
                ToggledItemsListGenerator.CreateItemsList(ItemsManager.Instance.GetAll);
                ToggledItemsListGenerator.CreatePlacesList(PlaceManager.Instance.GetAll);
            }
        }
        
        public void Startup()
        {
            currentLoop = LoopRoutine();
            StartCoroutine(currentLoop);
            currentControlCoroutine = ControlCoroutine();
            StartCoroutine(currentControlCoroutine);

            ModConsole.Log("<color=green>[MOP] MOD LOADED SUCCESFULLY!</color>");
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
#endregion
#region Save Game Actions
        /// <summary>
        /// Looks for gamobject named SAVEGAME, and hooks PreSaveGame into them.
        /// </summary>
        void HookPreSaveGame()
        {
            try
            {
                IEnumerable<GameObject> saveGames = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(obj => obj.name.Contains("SAVEGAME"));

                int i = 0;
                for (; i < saveGames.Count(); i++)
                {
                    bool useInnactiveFix = false;
                    bool isJail = false;

                    GameObject savegame = saveGames.ElementAt(i);

                    if (!savegame.activeSelf)
                    {
                        useInnactiveFix = true;
                        savegame.SetActive(true);
                    }

                    if (savegame.transform.parent != null && savegame.transform.parent.name == "JAIL" && !savegame.transform.parent.gameObject.activeSelf)
                    {
                        useInnactiveFix = true;
                        isJail = true;
                        savegame.transform.parent.gameObject.SetActive(true);
                    }

                    savegame.FsmInject("Mute audio", PreSaveGame);
                    savegame.FsmInject("Wait for click", SaveManager.RemoveReadOnlyAttribute);

                    if (useInnactiveFix)
                    {
                        if (isJail)
                        {
                            savegame.transform.parent.gameObject.SetActive(false);
                            continue;
                        }

                        savegame.SetActive(false);
                    }
                }

                // Hooking up on death save.
                GameObject onDeathSaveObject = new GameObject("MOP_OnDeathSave");
                onDeathSaveObject.transform.parent = GameObject.Find("Systems").transform.Find("Death/GameOverScreen");
                OnDeathBehaviour behaviour = onDeathSaveObject.AddComponent<OnDeathBehaviour>();
                behaviour.Initialize(PreSaveGame);
                i++;

                // Adding custom action to state that will trigger PreSaveGame, if the player picks up the phone with large Suski.
                GameObject telephone = GameObject.Find("Telephone");
                if (telephone != null)
                {
                    telephone.transform.Find("Logic/UseHandle").GetComponent<PlayMakerFSM>().GetState("Pick phone").InsertAction(0, new CustomSuskiLargeFlip());
                    i++;
                }

                ModConsole.Log($"[MOP] Hooked {i} save points!");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "SAVE_HOOK_ERROR");
            }
        }

        /// <summary>
        /// This void is initialized before the player decides to save the game.
        /// </summary>
        private void PreSaveGame()
        {
            ModConsole.Log("[MOP] Initializing Pre-Save Actions...");
            SaveManager.ReleaseSave();
            MopSettings.IsModActive = false;
            StopCoroutine(currentLoop);
            StopCoroutine(currentControlCoroutine);
            
            SaveManager.RemoveReadOnlyAttribute();
            ItemsManager.Instance.OnSave();

            ToggleAll(true, ToggleAllMode.OnSave);
            ModConsole.Log("[MOP] Pre-Save Actions Completed!");
        }

        public void DelayedPreSave()
        {
            if (currentDelayedSaveRoutine != null)
            {
                StopCoroutine(currentDelayedSaveRoutine);
            }

            currentDelayedSaveRoutine = DelayedSaveRoutine();
            StartCoroutine(currentDelayedSaveRoutine);
        }

        private IEnumerator currentDelayedSaveRoutine;
        IEnumerator DelayedSaveRoutine()
        {
            yield return new WaitForSeconds(1);
            if (FsmManager.IsSuskiLargeCall())
                PreSaveGame();
        }
#endregion
#region Updating
        /// <summary>
        /// This coroutine runs
        /// </summary>
        private IEnumerator currentLoop;
        IEnumerator LoopRoutine()
        {
            MopSettings.IsModActive = true;

            int waitTimer = 0;

            while (MopSettings.IsModActive)
            {
                // Ticks make sure that MOP is still up and running.
                // If the ticks didn't update, that means this routine stopped.
                ++ticks;

                if (!IsItemInitializationDone())
                {
                    // We are slightly delaying the initialization, so all items have chance to set in place, because fuck MSC and its physics.
                    waitTimer++;
                    if (waitTimer >= WaitForPhysicsToSettleTime)
                    {
                        FinishLoading();
                    }
                }

                isPlayerAtYard = MOP.ActiveDistance.GetValue() <= 1 ? Vector3.Distance(player.position, placeManager[0].transform.position) < 100
                    : Vector3.Distance(player.position, placeManager[0].transform.position) < 100 * MopSettings.ActiveDistanceMultiplicationValue;

                // When player is in any of the sectors, MOP will act like the player is at yard.
                if (SectorManager.Instance.IsPlayerInSector())
                {
                    inSectorMode = true;
                    isPlayerAtYard = true;
                }
                else
                {
                    inSectorMode = false;
                }

                yield return null;

                int i;
                long half = worldObjectManager.Count >> 1;
                // World Objects.
                for (i = 0; i < worldObjectManager.Count; ++i)
                {
                    if (i == half)
                        yield return null;

                    try
                    {
                        GenericObject worldObject = worldObjectManager[i];
                        GameObject gm = worldObject.GameObject;

                        // Check if object was destroyed (mostly intended for AI pedastrians).
                        if (gm == null)
                        {
                            worldObjectManager.Remove(worldObject);
                            continue;
                        }

                        string name = gm.name;

                        if (SectorManager.Instance.IsPlayerInSector() && SectorManager.Instance.SectorRulesContains(name))
                        {
                            gm.SetActive(true);
                            continue;
                        }

                        // Should the object be disabled when the player leaves the house?
                        if (worldObject.DisableOn.HasFlag(DisableOn.PlayerAwayFromHome) || worldObject.DisableOn.HasFlag(DisableOn.PlayerInHome))
                        {
                            if (name == "NPC_CARS" && inSectorMode)
                                continue;

                            if (name == "COMPUTER" && computerSystem.activeSelf)
                                continue;

                            bool enableObject = worldObject.DisableOn.HasFlag(DisableOn.PlayerAwayFromHome) ? isPlayerAtYard : !isPlayerAtYard;

                            if (worldObject.DisableOn.HasFlag(DisableOn.DoNotEnableWhenLeavingHome) && enableObject && !isPlayerAtYard)
                            {
                                continue;
                            }

                            worldObject.Toggle(worldObject.DisableOn.HasFlag(DisableOn.PlayerAwayFromHome) ? isPlayerAtYard : !isPlayerAtYard);
                        }
                        else if (worldObject.DisableOn.HasFlag(DisableOn.Distance))
                        {                      
                            // The object will be disabled, if the player is in the range of that object.
                            worldObject.Toggle(IsGenericObjectEnabled(worldObject));
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, false, "WORLD_OBJECT_TOGGLE_ERROR");
                    }
                }

                // Safe mode prevents toggling elemenets that MAY case some issues (vehicles, items, etc.)
                if (MopSettings.Mode == PerformanceMode.Safe)
                {
                    yield return new WaitForSeconds(.7f);
                    continue;
                }

                // So we create two separate lists - one is meant to enable, and second is ment to disable them,
                // Why?
                // If we enable items before enabling vehicle inside of which these items are supposed to be, they'll fall through to ground.
                // And the opposite happens if we disable vehicles before disabling items.
                // So if we are disabling items, we need to do that BEFORE we disable vehicles.
                // And we need to enable items AFTER we enable vehicles.
                half = ItemsManager.Instance.Count >> 1;
                for (i = 0; i < ItemsManager.Instance.Count; ++i)
                {
                    if (i == half)
                        yield return null;

                    // Safe check if somehow the i gets bigger than array length.
                    if (i >= ItemsManager.Instance.Count) break;

                    try
                    {

                        ItemBehaviour item = ItemsManager.Instance[i];

                        if (item == null || item.gameObject == null)
                        {
                            itemsToRemove.Enqueue(item);
                            continue;
                        }

                        // Check the mode in what MOP is supposed to run and adjust to it.
                        bool toEnable;
                        if (MopSettings.Mode == PerformanceMode.Performance)
                        {
                            toEnable = IsEnabled(item.transform, FsmManager.IsPlayerInCar() && !isPlayerAtYard ? 20 : 50);
                        }
                        else
                        {
                            toEnable = IsEnabled(item.transform);
                        }


                        if (toEnable)
                        {
                            item.ToggleChangeFix();

                            // Load object's position AFTER MOP starts to actually function,
                            // AND item is meant to be enabled.
                            if (ticks > 1 && !item.WasTransformLoaded)
                            {
                                item.LoadTransform();
                            }

                            if (item.ActiveSelf) continue;
                            itemsToEnable.Enqueue(item);
                        }
                        else
                        {
                            if (!item.ActiveSelf) continue;
                            item.Toggle(false);
                        }

                        if (item.rb != null && item.rb.IsSleeping())
                        {
                            if (CompatibilityManager.IsInBackpack(item)) continue;
                            item.rb.isKinematic = true;
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, false, "ITEM_TOGGLE_GATHER_ERROR");
                    }
                }

                // Vehicles
                half = vehicleManager.Count >> 1;
                for (i = 0; i < vehicleManager.Count; ++i)
                {
                    if (i == half)
                        yield return null;

                    try
                    {
                        Vehicle vehicle = vehicleManager[i];
                        if (vehicle == null)
                        {
                            vehicleManager.RemoveAt(i);
                            continue;
                        }

                        distance = Vector3.Distance(player.transform.position, vehicle.transform.position);
                        toggleDistance = MOP.ActiveDistance.GetValue() <= 1
                            ? MopSettings.UnityCarActiveDistance : MopSettings.UnityCarActiveDistance * MopSettings.ActiveDistanceMultiplicationValue;

                        switch (vehicle.VehicleType)
                        {
                            case VehiclesTypes.Satsuma:
                                Satsuma.Instance.ToggleElements(distance);
                                vehicle.ToggleEventSounds(distance < 3);
                                break;
                            case VehiclesTypes.Jonnez:
                                vehicle.ToggleEventSounds(distance < 2);
                                break;
                        }


                        bool isVehicleEnabled = IsVehicleEnabled(distance);
                        vehicle.ToggleUnityCar(IsVehiclePhysicsEnabled(distance, toggleDistance));
                        vehicle.Toggle(isVehicleEnabled);

                        if (!isVehicleEnabled)
                        {
                            vehicle.ToggleDummyCar(distance < 300);
                        }
                        else
                        {
                            vehicle.ToggleDummyCar(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, false, $"VEHICLE_TOGGLE_ERROR_{i}");
                    }
                }

                // Items To Enable
                while (itemsToEnable.Count > 0)
                {
                    try
                    {
                        itemsToEnable.Dequeue().Toggle(true);
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, false, "ITEM_TOGGLE_ENABLE_ERROR");
                    }
                }

                // Places (New)
                int full = placeManager.Count;
                half = full >> 1;
                for (i = 0; i < full; ++i)
                {
                    if (i == half)
                        yield return null;

                    try
                    {
                        if (SectorManager.Instance.IsPlayerInSector() && SectorManager.Instance.SectorRulesContains(placeManager[i].GetName()))
                        {
                            continue;
                        }

                        placeManager[i].ToggleActive(IsPlaceEnabled(placeManager[i].transform, placeManager[i].GetToggleDistance()));
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, false, $"PLACE_TOGGLE_ERROR_{i}");
                    }
                }

                // Remove items that don't exist anymore.
                while (itemsToRemove.Count > 0)
                {
                    ItemsManager.Instance.Remove(itemsToRemove.Dequeue());
                }

                yield return new WaitForSeconds(.7f);

                if (retries > 0 && !restartSucceedMessaged)
                {
                    restartSucceedMessaged = true;
                    ModConsole.Log("<color=green>[MOP] Restart succeeded!</color>");
                }
            }
        }

        void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.F5))
            {
                PreSaveGame();
                Application.LoadLevel(1);
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                PreSaveGame();
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                SaveManager.RemoveReadOnlyAttribute();
            }
            
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                FinishLoading();
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                List<ItemBehaviour> b = ItemsManager.Instance.GetAll;
                foreach (ItemBehaviour i in b)
                {
                    if (i.name.Contains("r20 battery box"))
                    {
                        i.LoadTransform();
                    }
                }
            }

#endif
            if (!MopSettings.IsModActive) return;

            Satsuma.Instance?.ForceRotation();

            if (MopSettings.Mode > PerformanceMode.Performance)
            {
                // Pointless to checck if traffic is about to collide with other vehicles, if traffic is disabled.
                if (!traffic.activeSelf)
                {
                    return;
                }

                // Same for traffic highway and dirt road traffic.
                if (!trafficHighway.activeSelf && !trafficDirt.activeSelf)
                {
                    return;
                }

                for (int i = 0; i < vehicleManager.Count; ++i)
                {
                    if (vehicleManager[i] == null) continue;
                    if (!vehicleManager[i].IsActive) continue;
                    // Vehicle is not on the dirt road, nor on highway.
                    // It's very unlikely that it will be hit by anything.
                    if (Vector3.Distance(Vector3.zero, vehicleManager[i].transform.position) < 1000) continue; 

                    if (vehicleManager[i].IsTrafficCarInArea())
                        vehicleManager[i].ToggleUnityCar(true);
                }
            }
        }

        #endregion
        #region Item enabling checks
        const int MinimumItemDistance = 10;
        const int MinimumItemOutsideDistance = 30;
        const int DefaultItemToggleDistance = 75;
        // 0.25 = 18 | 0.5 = 37 | 0.75 = 56 | 1 = 75 | 2 = 150 | 4 = 300

        const int MinimumVehicleDistance = 20;
        const int MinimumVehicleOutsideDistance = 50;
        const int DefaultVehicleDistance = 125;
        // 0.25 = 31 | 0.5 = 62 | 0.75 = 93 | 1 = 125 | 2 = 250 | 4 = 500

        const int MinimumPlaceDistance = 175;

        /// <summary>
        /// Checks if the object is supposed to be enabled by calculating the distance between player and target.
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <param name="toggleDistance">Distance below which the object should be enabled (default 200 units).</param>
        bool IsEnabled(Transform target, float toggleDistance = DefaultItemToggleDistance)
        {
            if (toggleDistance == 0)
            {
                return false;
            }

            if (inSectorMode)
                toggleDistance = 20;

            toggleDistance *= MopSettings.ActiveDistanceMultiplicationValue;

            if (inSectorMode)
            {
                if (toggleDistance < MinimumItemDistance)
                {
                    toggleDistance = MinimumItemDistance;
                }
            }
            else
            {
                if (toggleDistance < MinimumItemOutsideDistance)
                {
                    toggleDistance = MinimumItemOutsideDistance;
                }
            }
            

            return Vector3.Distance(player.transform.position, target.position) < toggleDistance;
        }

        bool IsGenericObjectEnabled(GenericObject obj)
        {
            if (obj.Distance == -1)
            {
                return false;
            }

            float toggleDistance = obj.Distance;
            if (obj.DisableOn.HasFlag(DisableOn.AlwaysUse1xDistance))
            {
                toggleDistance *= MOP.ActiveDistance.GetValue() <= 0 ? 0.5f : 0.1f;
                return Vector3.Distance(player.transform.position, obj.transform.position) < toggleDistance;
            }

            if (inSectorMode)
            {
                toggleDistance *= MOP.ActiveDistance.GetValue() <= 0 ? 0.5f : 0.1f;
            }

            toggleDistance *= MopSettings.ActiveDistanceMultiplicationValue;
            if (toggleDistance < obj.MinimumToggleDistance && !inSectorMode)
            {
                toggleDistance = obj.MinimumToggleDistance;
            }

            return Vector3.Distance(player.transform.position, obj.transform.position) < toggleDistance;
        }

        bool IsVehicleEnabled(float distance, float toggleDistance = DefaultVehicleDistance)
        {
            if (inSectorMode)
            {
                toggleDistance = MinimumVehicleDistance;
            }

            toggleDistance *= MopSettings.ActiveDistanceMultiplicationValue;

            if (inSectorMode)
            {
                if (toggleDistance < MinimumVehicleDistance)
                {
                    toggleDistance = MinimumVehicleDistance;
                }
            }
            else
            {
                if (toggleDistance < MinimumVehicleOutsideDistance)
                {
                    toggleDistance = MinimumVehicleOutsideDistance;
                }
            }

            return distance < toggleDistance;
        }

        bool IsVehiclePhysicsEnabled(float distance, float toggleDistance = 200)
        {
            return distance < toggleDistance;
        }

        bool IsPlaceEnabled(Transform target, float toggleDistance = 200)
        {
            toggleDistance *= MopSettings.ActiveDistanceMultiplicationValue;
            if (toggleDistance < MinimumPlaceDistance)
            {
                toggleDistance = MinimumPlaceDistance;
            }
            return Vector3.Distance(player.transform.position, target.position) < toggleDistance;
        }
#endregion
#region System Control & Crash Protection
        private int ticks;
        public int Tick { get => ticks; }
        private int lastTick;
        private int retries;
        private const int MaxRetries = 3;
        private bool restartSucceedMessaged;
        private IEnumerator currentControlCoroutine;

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
                    if (retries >= MaxRetries)
                    {
                        ModConsole.LogError("[MOP] Restart attempt failed. Enabling Safe Mode.");
                        ModConsole.LogError("[MOP] Please contact mod developer. Please use \"I Found a Bug\" button in MOP settings!");
                        try { ToggleAll(true); } catch { }
                        MopSettings.EnableSafeMode();
                        yield break;
                    }

                    retries++;
                    restartSucceedMessaged = false;
                    ModConsole.LogWarning($"[MOP] MOP has stopped working! Restart attempt {retries}/{MaxRetries}...");
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
#endregion

        /// <summary>
        /// Toggles on all objects.
        /// </summary>
        public void ToggleAll(bool enabled, ToggleAllMode mode = ToggleAllMode.Default)
        {
            ModConsole.Log($"[MOP] Toggling all to {enabled.ToString().ToUpper()} in mode {mode.ToString().ToUpper()}");

            // World objects
            for (int i = 0; i < worldObjectManager.Count; i++)
            {
                try
                {
                    worldObjectManager[i]?.Toggle(enabled);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "TOGGLE_ALL_WORLD_OBJECTS_ERROR");
                }
            }

            ModConsole.Log("[MOP] Toggled WORLD OBJECTS");

            if (MopSettings.Mode == PerformanceMode.Safe) return;

            // Items
            for (int i = 0; i < ItemsManager.Instance.Count; i++)
            {
                if (i >= ItemsManager.Instance.Count) break;

                try
                {
                    ItemBehaviour item = ItemsManager.Instance[i];
                    item.Toggle(enabled);

                    // We're freezing the object on save, so it won't move at all.
                    if (mode == ToggleAllMode.OnSave)
                    {
                        item.gameObject.SetActive(true);
                        item.Freeze();
                        item.SaveGame();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "TOGGLE_ALL_ITEMS_ERROR");
                }
            }

            ModConsole.Log("[MOP] Toggled ITEMS");

            // Find all kilju, emptyca, empty juice container, and force empty them if applicable
            try
            {
                foreach (GameObject bottle in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name.ContainsAny("kilju", "emptyca", "empty plastic can")))
                {
                    bottle.GetComponent<ItemBehaviour>()?.ResetKiljuContainer();
                    if (mode == ToggleAllMode.OnSave)
                    {
                        bottle.SetActive(true);
                        bottle.GetComponent<ItemBehaviour>()?.SaveGame();
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "KILJU_RESET_FORCE_ERROR");
            }

            ModConsole.Log("[MOP] Toggled KILJU");

            // Vehicles
            for (int i = 0; i < vehicleManager.Count; i++)
            {
                try
                {
                    vehicleManager[i].Toggle(enabled);

                    if (mode == ToggleAllMode.OnLoad)
                    {
                        vehicleManager[i].ForceToggleUnityCar(false);
                    }
                    else if (mode == ToggleAllMode.OnSave)
                    {
                        vehicleManager[i].ToggleUnityCar(enabled);
                        vehicleManager[i].Freeze();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, $"TOGGLE_ALL_VEHICLE_ERROR_{i}");
                }
            }

            try
            {
                if (mode == ToggleAllMode.OnSave)
                {
                    Satsuma.Instance.OnSaveGlueAll();
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TOGGLE_SATSUMA_GLUE_ALL_ERROR");
            }

            ModConsole.Log("[MOP] Toggled VEHICLES");

            // Places
            for (int i = 0; i < placeManager.Count; i++)
            {
                try
                {
                    placeManager[i].ToggleActive(enabled);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, $"TOGGLE_ALL_PLACES_{i}");
                }
            }

            ModConsole.Log("[MOP] Toggled PLACES");

            // Force teleport kilju bottles.
            try
            {
                if (mode == ToggleAllMode.OnSave)
                {
                    GameObject canTrigger = ItemsManager.Instance.GetCanTrigger();
                    if (canTrigger)
                    {
                        if (!canTrigger.transform.parent.gameObject.activeSelf)
                        {
                            canTrigger.transform.parent.gameObject.SetActive(true);
                        }
                        canTrigger.GetComponent<PlayMakerFSM>().SendEvent("STOP");
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TOGGLE_ALL_JOBS_DRUNK");
            }

            ModConsole.Log("[MOP] Toggled KILJU TELEPORT");

            // ToggleElements class of Satsuma.
            try
            {
                if (mode == ToggleAllMode.OnSave)
                {
                    Satsuma.Instance.ToggleElements(0);
                }
                else
                {
                    Satsuma.Instance.ToggleElements(enabled ? 0 : 10000);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TOGGLE_ALL_SATSUMA_TOGGLE_ELEMENTS");
            }

            ModConsole.Log("[MOP] Toggled SATSUMA ELEMENTS");
            ModConsole.Log("[MOP] Toggle done!");
        }

        public Transform GetPlayer()
        {
            return player;
        }

        public bool IsInSector()
        {
            return inSectorMode;
        }

        public bool IsItemInitializationDone()
        {
            return itemInitializationDelayDone;
        }

        void FinishLoading()
        {
            if (loadScreenWorkaround != null)
            {
                StopCoroutine(loadScreenWorkaround);
            }

            itemInitializationDelayDone = true;
            loadScreen?.Deactivate();
            playerController.enabled = true;
            FsmManager.PlayerInMenu = false;

            GameObject vh = GameObject.Find("VehiclesHighway");
            if (vh)
            {
                vh.SetActive(false);
            }
        }

        private readonly IEnumerator loadScreenWorkaround;
        IEnumerator InfiniteLoadscreenWorkaround()
        {
            yield return new WaitForSeconds(LoadScreenWorkaroundTime);
            if (FsmManager.PlayerInMenu)
            {
                ModConsole.LogError("[MOP] MOP failed to load in time. Please go into MOP settings and use \"I found a bug\" button.");
                FinishLoading();
                Satsuma.Instance?.ToggleActive(true);
                Satsuma.Instance?.ForceToggleUnityCar(true);
                FsmManager.PlayerInMenu = false;
            }
        }

        internal void ToggleDebugMode()
        {
            if (GetComponent<DebugTools.DebugMonitor>() == null)
            {
                gameObject.AddComponent<DebugTools.DebugMonitor>();
            }
            else
            {
                Destroy(GetComponent<DebugTools.DebugMonitor>());
            }
        }
    }
}
