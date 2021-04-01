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

using HutongGames.PlayMaker;
using MSCLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOP.FSM.Actions;
using MOP.Common;
using MOP.Common.Enumerations;
using MOP.Vehicles.Managers.SatsumaManagers;
using MOP.Rules;
using MOP.Rules.Types;
using MOP.Items;

namespace MOP.Vehicles
{
    class Satsuma : Vehicle
    {
        const int MinimumBolts = 163;

        static Satsuma instance;
        public static Satsuma Instance { get => instance; }

        // If true, prevents Satsuma physics from disabling.
        public bool IsSatsumaInInspectionArea;
        public bool IsSatsumaInParcFerme;

        // If is false, the first enabling script will be toggled.
        public bool AfterFirstEnable;

        // Objects that can be toggled.
        readonly Transform[] disableableObjects;

        // Whitelist of name of objects that should be disabled.
        string[] whiteList = 
        { 
            "DriveTrigger", "SpawnPlayer", "CarShadowProjector", "Hooks", "CameraPivot", "PivotSeatR", "HookRear", "BeerCaseTarget",
            "HookFront", "GetInPivot", "TrafficTrigger", "Wipers", "WiperLeftPivot", "WiperRightPivot", "wipers_tap", "wipers_rod",
            "generalPivot", "CarRearMirrorPivot", "StagingWheel", "shadow_body", "NormalFront", "NormalSide", "NormalRear", "WindSide",
            "WindFront", "WindRear", "CoG", "Interior", "Body", "MiscParts", "Dashboard" 
        };

        // Renderers.
        // Renderes of the entire car.
        readonly List<Renderer> renderers;
        readonly List<Renderer> engineBayRenderers;

        // Pivot transform of hood.
        readonly Transform pivotHood;

        // Key object.
        readonly GameObject key;

        // Rear bumper transform.
        readonly GameObject rearBumper;
        readonly GameObject rearBumperTrigger;
        readonly GameObject databaseBumper;
        readonly FsmBool isBumperBolted;
        int bumperFixRetries;
        bool doBumperFixes;
        const int MaxBumperFixRetries = 10;

        // Elements that are toggled uppon some situation.
        List<SatsumaOnActionObjects> satsumaOnActionObjects;

        // Bolts that resetting will be disabled.
        List<SatsumaBoltsAntiReload> satsumaBoltsAntiReloads;
        // Fix for elements for which the bolts are masked.
        Dictionary<GameObject, bool> maskedElements;
        int maskedFixStages;

        // Dashboard gauges.
        List<Material> dashboardMaterials;
        FsmInt lightSelection;

        // Cooldown ticking object.
        GameObject cooldownTick;

        // AI object.
        PlayMakerFSM drivingAI;

        // Used for when the Fleetari has nicked the car.
        // It prevents the car teleporting back to the last position known to the player.
        bool hasBeenMovedByFleetari;

        /// <summary>
        /// Initialize class
        /// </summary>
        public Satsuma(string gameObject) : base(gameObject)
        {
            instance = this;

            disableableObjects = GetDisableableChilds();

            Toggle = ToggleActive;

            // Get engine bay renderers
            engineBayRenderers = new List<Renderer>();
            Transform block = this.gameObject.transform.Find("Chassis/sub frame(xxxxx)/CarMotorPivot");
            engineBayRenderers = block.GetComponentsInChildren<Renderer>(true).ToList();
            pivotHood = this.gameObject.transform.Find("Body/pivot_hood");

            // Get all the other renderers
            renderers = new List<Renderer>();
            if (RulesManager.Instance.IgnoreRules.Count > 0)
            {
                renderers = this.gameObject.transform.GetComponentsInChildren<Renderer>(true)
                    .Where(t => !t.gameObject.name.ContainsAny("Sphere", "Capsule", "Cube", "Mokia") && RulesManager.Instance.IgnoreRules.Find(g => g.ObjectName == t.gameObject.name) == null).ToList();
            }
            else
            {
                renderers = this.gameObject.transform.GetComponentsInChildren<Renderer>(true)
                    .Where(t => !t.gameObject.name.ContainsAny("Sphere", "Capsule", "Cube", "Mokia")).ToList();
            }

            // Ignore Rule
            IgnoreRule vehicleRule = RulesManager.Instance.IgnoreRules.Find(v => v.ObjectName == this.gameObject.name);
            if (vehicleRule != null)
            {
                Toggle = IgnoreToggle;

                if (vehicleRule.TotalIgnore)
                    IsActive = false;
            }

            lastGoodRotation = transform.localRotation;
            lastGoodPosition = transform.localPosition;

            // Adding components to normal and bucket seats.
            GameObject.Find("seat driver(Clone)").AddComponent<SatsumaSeatsManager>();
            GameObject.Find("seat passenger(Clone)").AddComponent<SatsumaSeatsManager>();

            GameObject bucketDriver = GameObject.Find("bucket seat driver(Clone)");
            GameObject bucketPassanger = GameObject.Find("bucket seat passenger(Clone)");
            if (bucketDriver == null)
            {
                bucketDriver = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "bucket seat driver(Clone)"
                && g.transform.parent.gameObject.name == "Parts");
                bucketPassanger = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "bucket seat passenger(Clone)"
                && g.transform.parent.gameObject.name == "Parts");
            }
            if (bucketDriver)
            {
                bucketDriver.AddComponent<SatsumaSeatsManager>();
            }
            if (bucketPassanger)
            {
                bucketPassanger.AddComponent<SatsumaSeatsManager>();
            }

            // Fix for mechanical wear of the car.
            transform.Find("CarSimulation/MechanicalWear").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            // Fix for not working handbrake after respawn.
            GameObject.Find("HandBrake").GetComponent<PlayMakerFSM>().enabled = true;

            // Fixes handbrake lever position.
            transform.Find("MiscParts/HandBrake/handbrake(xxxxx)/handbrake lever").GetPlayMakerByName("Use").Fsm.RestartOnEnable = false;

            // Battery terminal.
            // For some reason in the vanilla game, the negative battery terminal sometimes gets disabled, and the game doesn't allow to reenable it.
            FsmState wiringBatteryDisable = transform.Find("Wiring").GetPlayMakerByName("Status").FindFsmState("Disable battery wires");
            List<FsmStateAction> disableBatteryActions = new List<FsmStateAction>();
            disableBatteryActions.Add(new CustomBatteryDisable());
            wiringBatteryDisable.Actions = disableBatteryActions.ToArray();

            // Get all bolts.
            satsumaBoltsAntiReloads = new List<SatsumaBoltsAntiReload>();
            GameObject[] bolts = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "BoltPM").ToArray();
            foreach (GameObject bolt in bolts)
            {
                Transform parent = bolt.transform.parent;

                // Elements with that name have their BoltCheck parents as a grandparent (water_pump and Hooks are exception)
                if (parent.name.EqualsAny("Bolts", "_Motor", "water_pump_pulley_mesh", "Hooks", "hooks"))
                {
                    // Great-great grandparents for these ones.
                    if (parent.parent.gameObject.name.EqualsAny("water_pump_pulley_mesh", "Hooks", "hooks"))
                    {
                        parent = parent.parent.parent;
                    }
                    else
                    {
                        parent = parent.parent;
                    }
                }
                // Great-great grandparents for these too...
                else if (parent.name.ContainsAny("Masked", "ValveAdjust"))
                {
                    parent = parent.parent.parent;
                }
                // Sibling of bolt if the parent is IK_wishbone.
                else if (parent.name.StartsWith("IK_wishbone_"))
                {
                    string suffix = parent.gameObject.name.Replace("IK_wishbone_", "");
                    parent = parent.Find($"wishbone {suffix}(xxxxx)");
                }

                // Skip those if the assigned parent is one of these.
                if (parent.name.ContainsAny("bolts_shock", "shock_bottom", "halfshaft_", "pivot_steering_arm_", "OFFSET", "pivot_shock_"))
                {
                    continue;
                }

                // Skip if the parent is Pivot and the grandparent name starts with "alternator".
                if (parent.name == "Pivot" && parent.parent.gameObject.name.StartsWith("alternator"))
                {
                    continue;
                }

                if (parent.gameObject.name == "hood(Clone)")
                    continue;

                if (parent.gameObject.GetComponent<SatsumaBoltsAntiReload>() == null)
                {
                    SatsumaBoltsAntiReload s = parent.gameObject.AddComponent<SatsumaBoltsAntiReload>();
                    satsumaBoltsAntiReloads.Add(s);
                }
            }

            // Halfshafts hook.
            GameObject[] halfshafts = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "halfshaft(xxxxx)").ToArray();
            foreach (GameObject halfshaft in halfshafts)
            {
                if (halfshaft.GetComponent<SatsumaBoltsAntiReload>() == null)
                    satsumaBoltsAntiReloads.Add(halfshaft.AddComponent<SatsumaBoltsAntiReload>());
            }

            // Engine Block.
            satsumaBoltsAntiReloads.Add(GameObject.Find("block(Clone)").AddComponent<SatsumaBoltsAntiReload>());

            // Steering rods.
            satsumaBoltsAntiReloads.Add(transform.Find("Chassis/steering rod fr(xxxxx)").gameObject.AddComponent<SatsumaBoltsAntiReload>());
            satsumaBoltsAntiReloads.Add(transform.Find("Chassis/steering rod fl(xxxxx)").gameObject.AddComponent<SatsumaBoltsAntiReload>());

            // Destroy all bolt anti reloads.
            ModConsole.Print($"[MOP] Found {satsumaBoltsAntiReloads.Count} bolts.");
            // If there's less bolts found than the value, warn user.
            if (satsumaBoltsAntiReloads.Count < MinimumBolts)
            {
                ModConsole.Error($"[MOP] Only {satsumaBoltsAntiReloads.Count} out of expected {MinimumBolts} have been reset!");
            }

            // Fixes car body color resetting to default.
            transform.Find("Body/car body(xxxxx)").GetPlayMakerByName("Paint").Fsm.RestartOnEnable = false;
            
            // Wiping Load for alternator belts, oil filters, spark plugs and batteries.
            GameObject[] parts = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(obj => obj.name.ContainsAny("alternator belt(Clone)", "oil filter(Clone)", "spark plug(Clone)", "battery(Clone)"))
                .ToArray();
            foreach (GameObject part in parts)
            {
                if (part.transform.root.gameObject.name == "GAZ24(1420kg)") continue;

                PlayMakerFSM useFsm = part.GetPlayMakerByName("Use");
                FsmState state1 = useFsm.FindFsmState("State 1");
                List<FsmStateAction> emptyState1 = state1.Actions.ToList();
                emptyState1.Insert(0, new CustomStop());
                state1.Actions = emptyState1.ToArray();
                state1.SaveActions();

                useFsm.FindFsmState("Load").Fsm.RestartOnEnable = false;
            }
            
            // Fix for cd player disabling other vehicles radio.
            Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "cd player(Clone)")
            .transform.Find("ButtonsCD/RadioVolume").GetPlayMakerByName("Knob").Fsm.RestartOnEnable = false;
            GameObject.Find("radio(Clone)").transform.Find("ButtonsRadio/RadioVolume").GetPlayMakerByName("Knob").Fsm.RestartOnEnable = false;

            // Fix for window grille paint resetting to the default.
            Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "window grille(Clone)" && g.GetPlayMakerByName("Paint") != null)
                .GetPlayMakerByName("Paint").Fsm.RestartOnEnable = false;

            // Apply hood fix.
            GameFixes.Instance.HoodFix(transform.Find("Body/pivot_hood"),
                transform.Find("MiscParts/trigger_battery"), transform.Find("MiscParts/pivot_battery"));

            // Rear bumper detachng fix.
            rearBumper = GameObject.Find("bumper rear(Clone)");
            rearBumperTrigger = transform.Find("Body/trigger_bumper_rear").gameObject;
            databaseBumper = GameObject.Find("Database/DatabaseBody/Bumper_Rear");
            databaseBumper.SetActive(false);
            databaseBumper.SetActive(true);
            isBumperBolted = databaseBumper.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Bolted");
            doBumperFixes = isBumperBolted.Value;
            DoBumperFix();

            // Fix suspension adding a weight to the car on each car respawn.
            GameObject[] suspensionParts = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(g => g.name.ContainsAny("strut", "coil spring", "shock absorber") 
                         && g.transform.root != null 
                         && g.transform.root == this.gameObject.transform)
                .ToArray();
            foreach (GameObject part in suspensionParts)
            {
                try
                {
                    part.AddComponent<SatsumaPartMassManager>();
                }
                catch
                {
                    ExceptionManager.New(new System.Exception("SatsumaPartMassManager: Suspension"), false, "SatsumaPartsMassManager: Adding to suspension parts");
                }
            }
            // Appparently not only suspension fucks over the Satsuma...
            GameObject[] others = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(g => g.name.EqualsAny("grille gt(Clone)", "grille(Clone)", "subwoofer panel(Clone)",
                "seat rear(Clone)", "amplifier(Clone)", "rearlight(leftx)", "rearlight(right)", "racing radiator(xxxxx)", "radiator(xxxxx)",
                "radiator hose1(xxxxx)", "radiator hose3(xxxxx)", "marker light left(xxxxx)", "marker light right(xxxxx)", "antenna(leftx)",
                "antenna(right)", "dashboard(Clone)")).ToArray();
            foreach (GameObject other in others)
            {
                try
                {
                    other.AddComponent<SatsumaPartMassManager>();
                }
                catch
                {
                    ExceptionManager.New(new System.Exception("SatsumaPartMassManager: Others"), false, "SatsumaPartsMassManager: Others adding.");
                }
            }

            key = transform.Find("Dashboard/Steering/steering_column2/Ignition/Keys/Key").gameObject;
            cooldownTick = GameObject.Find("block(Clone)").transform.Find("CooldownTick").gameObject;

            // Fix for reg plates Z fighting.
            try
            {
                GameObject.Find("bootlid(Clone)").transform.Find("RegPlateRear").gameObject.GetComponent<Renderer>().material.renderQueue = 100;
                GameObject.Find("bumper front(Clone)").transform.Find("RegPlateFront").gameObject.GetComponent<Renderer>().material.renderQueue = 100;

                // Z-fighting of the Satsuma dashboard meters.
                if (!RulesManager.Instance.SpecialRules.SatsumaIgnoreRenderers)
                {
                    dashboardMaterials = new List<Material>();
                    dashboardMaterials.Add(GameObject.Find("dashboard meters(Clone)").transform.Find("Gauges/Fuel/needle_small").gameObject.GetComponent<Renderer>().material);
                    dashboardMaterials.Add(GameObject.Find("dashboard meters(Clone)").transform.Find("Gauges/Temp/needle_small").gameObject.GetComponent<Renderer>().material);
                    dashboardMaterials.Add(GameObject.Find("dashboard meters(Clone)").transform.Find("Gauges/Speedometer/needle_large").gameObject.GetComponent<Renderer>().material);
                    dashboardMaterials.Add(GameObject.Find("rpm gauge(Clone)").transform.Find("Pivot/needle").gameObject.GetComponent<Renderer>().material);
                    dashboardMaterials.Add(GameObject.Find("clock gauge(Clone)").transform.Find("ClockCar/hour/needle_hour").gameObject.GetComponent<Renderer>().material);
                    dashboardMaterials.Add(GameObject.Find("clock gauge(Clone)").transform.Find("ClockCar/minute/needle_minute").gameObject.GetComponent<Renderer>().material);

                    foreach (Material mat in dashboardMaterials)
                        mat.renderQueue = 100;


                    lightSelection = Resources.FindObjectsOfTypeAll<GameObject>()
                        .First(g => g.name == "dashboard meters(Clone)").transform.Find("Knobs/ButtonsDash/LightModes")
                        .gameObject.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("Selection");
                }
            }
            catch { }

            // Fixing the door hinges getting stuck.
            GameObject.Find("door left(Clone)").AddComponent<SatsumaHingeManager>();
            GameObject.Find("door right(Clone)").AddComponent<SatsumaHingeManager>();
            GameObject.Find("bootlid(Clone)").AddComponent<SatsumaHingeManager>();

            // Fixes doors resetting it's paint colour.
            GameObject.Find("door left(Clone)").GetPlayMakerByName("Paint").Fsm.RestartOnEnable = false;
            GameObject.Find("door right(Clone)").GetPlayMakerByName("Paint").Fsm.RestartOnEnable = false;

            // Setup stuff that gets disabled using ToggleElements
            satsumaOnActionObjects = new List<SatsumaOnActionObjects>();
            satsumaOnActionObjects.Add(new SatsumaOnActionObjects(transform.Find("CarSimulation/MechanicalWear").gameObject, SatsumaEnableOn.OnEngine));
            satsumaOnActionObjects.Add(new SatsumaOnActionObjects(transform.Find("CarSimulation/Fixes").gameObject, SatsumaEnableOn.OnEngine));
            satsumaOnActionObjects.Add(new SatsumaOnActionObjects(transform.Find("CarSimulation/DynoDistance").gameObject, SatsumaEnableOn.OnEngine));
            satsumaOnActionObjects.Add(new SatsumaOnActionObjects(transform.Find("CarSimulation/RandomBolt").gameObject, SatsumaEnableOn.OnEngine));
            
            satsumaOnActionObjects.Add(new SatsumaOnActionObjects(transform.Find("RainScript").gameObject, SatsumaEnableOn.OnPlayerClose));
            satsumaOnActionObjects.Add(new SatsumaOnActionObjects(transform.Find("DriverHeadPivot").gameObject, SatsumaEnableOn.OnPlayerClose));
            satsumaOnActionObjects.Add(new SatsumaOnActionObjects(transform.Find("AirIntake").gameObject, SatsumaEnableOn.OnPlayerClose));
            satsumaOnActionObjects.Add(new SatsumaOnActionObjects(this.gameObject.GetPlayMakerByName("ButtonShifter"), SatsumaEnableOn.OnPlayerClose));
            
            satsumaOnActionObjects.Add(new SatsumaOnActionObjects(transform.Find("Chassis").gameObject, SatsumaEnableOn.OnPlayerFar));

            // Replace on assemble sound playing with custom script.
            PlayMakerFSM blockBoltCheck = GameObject.Find("block(Clone)").GetPlayMakerByName("BoltCheck");
            FsmState boltsONState = blockBoltCheck.FindFsmState("Bolts ON");
            FsmStateAction[] boltsONActions = boltsONState.Actions;
            boltsONActions[1] = new MasterAudioAssembleCustom();
            boltsONState.Actions = boltsONActions;
            boltsONState.SaveActions();

            // Disable the "Fix Collider" state in FSM Setup, so it won't make items fall through the car.
            this.gameObject.GetPlayMakerByName("Setup").Fsm.RestartOnEnable = false;

            MeshCollider bootFloor = transform.Find("Colliders/collider_floor3").gameObject.GetComponent<MeshCollider>();
            bootFloor.isTrigger = false;
            bootFloor.enabled = true;

            // Fixes driver dying way too easily from small impacts (hopefully).
            transform.Find("DriverHeadPivot").GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            maskedElements = new Dictionary<GameObject, bool>();
            maskedElements.Add(Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "MaskedClutchCover"), key.activeSelf);
            maskedElements.Add(Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "MaskedBearing2"), key.activeSelf);
            maskedElements.Add(Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "MaskedBearing3"), key.activeSelf);
            maskedElements.Add(Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "MaskedFlywheel"), key.activeSelf);
            maskedElements.Add(Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "MaskedFlywheelRacing"), key.activeSelf);
            maskedElements.Add(Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "MaskedPiston2"), key.activeSelf);
            maskedElements.Add(Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "MaskedPiston3"), key.activeSelf);
            maskedElements.Add(Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "MaskedPiston4"), key.activeSelf);

            drivingAI = transform.Find("AI").GetPlayMakerByName("Driving");

            // radiator hose 3
            try
            {
                GameObject radiatorHosePart = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "radiator hose3(xxxxx)");
                if (radiatorHosePart)
                {
                    radiatorHosePart.AddComponent<SatsumaRadiatorHoseFix>();
                }
            }
            catch
            {
                throw new System.Exception("Radiator hose 3 error");
            }

            // Windscreen fixes :)
            try
            {
                Transform windscreen = transform.Find("Body/Windshield");
                if (!windscreen)
                {
                    throw new System.Exception("Couldn't find Windscreen.");
                }
                SatsumaWindscreenFixer swf = windscreen.gameObject.AddComponent<SatsumaWindscreenFixer>();
                // And now we are hooking up the windscreen job in repairshop.
                Transform windshieldJob = GameObject.Find("REPAIRSHOP").transform.Find("Jobs/Windshield");
                if (!windshieldJob)
                {
                    throw new System.Exception("Couldn't find windshield job.");
                }

                if (!windshieldJob.parent.gameObject.activeSelf)
                {
                    windshieldJob.parent.gameObject.SetActive(true);
                    windshieldJob.parent.gameObject.SetActive(false);
                }

                PlayMakerFSM windshieldJobFSM = windshieldJob.gameObject.GetComponent<PlayMakerFSM>();
                List<FsmStateAction> wait1Actions = windshieldJobFSM.FindFsmState("Wait1").Actions.ToList();
                wait1Actions.Insert(0, new WindscreenRepairJob(swf));
                windshieldJobFSM.FindFsmState("Wait1").Actions = wait1Actions.ToArray();
            }
            catch
            {
                throw new System.Exception("Windshield repair fix error.");
            }

            if (RulesManager.Instance.SpecialRules.ExperimentalSatsumaPhysicsFix)
            {
                CustomPlayMakerFixedUpdate wheelFL = transform.Find("FL/AckerFL/wheelFL").gameObject.AddComponent<CustomPlayMakerFixedUpdate>();
                CustomPlayMakerFixedUpdate wheelFR = transform.Find("FR/AckerFR/wheelFR").gameObject.AddComponent<CustomPlayMakerFixedUpdate>();
                wheelFL.StartFixedUpdate();
                wheelFR.StartFixedUpdate();
            }
            
            if (MopSettings.GenerateToggledItemsListDebug)
            {
                if (System.IO.File.Exists("sats.txt"))
                    System.IO.File.Delete("sats.txt");
                string vehiclez = "";
                foreach (var w in disableableObjects)
                    vehiclez += w.gameObject.name + ", ";
                System.IO.File.WriteAllText("sats.txt", vehiclez);
                System.Diagnostics.Process.Start("sats.txt");
            }
        }

        /// <summary>
        /// Enable or disable car
        /// </summary>
        new void ToggleActive(bool enabled)
        {
            if (enabled && rearBumperTrigger.activeSelf && rearBumper.activeSelf)
            {
                DoBumperFix();
            }

            // Applying hood fix after first enabling.
            if (!AfterFirstEnable && enabled)
            {
                AfterFirstEnable = true;
            }

            // Force enable if AI is driving the car.
            if (drivingAI != null && drivingAI.enabled) 
                enabled = true;

            // If the car is left in ParcFerme, the renderes may not re-enable, so just in case we force them to re-enable.
            if (IsSatsumaInParcFerme && !renderers[0].enabled)
                RenderersCulling(enabled);

            if (!RulesManager.Instance.SpecialRules.SatsumaIgnoreRenderers)
            {
                if (IsKeyInserted())
                {
                    if (lightSelection != null && lightSelection.Value > 0)
                        ToggleDashIllumination(true);
                    else
                        ToggleDashIllumination(false);
                }
                else
                {
                    ToggleDashIllumination(false);
                }
            }

            // Don't run the code, if the value is the same
            if (gameObject == null || disableableObjects[0].gameObject.activeSelf == enabled) return;

            if (!GameFixes.Instance.HoodFixDone && !GameFixes.Instance.RearBumperFixDone) return;

            if (!enabled)
            {
                lastGoodRotation = transform.rotation;
                lastGoodPosition = transform.position;
            }

            if (!GameFixes.Instance.HoodFixDone)
                enabled = true;

            for (int i = 0; i < disableableObjects.Length; i++)
            {
                if (disableableObjects[i] == null)
                    continue;

                bool isElementEnabled = IsSatsumaInParcFerme ? true : enabled;
                disableableObjects[i].gameObject.SetActive(isElementEnabled);
            }

            RenderersCulling(enabled);
        }

        /// <summary>
        /// Get list of disableable childs.
        /// It looks for objects that contian the name from the whiteList
        /// </summary>
        /// <returns></returns>
        internal Transform[] GetDisableableChilds()
        {
            if (RulesManager.Instance.IgnoreRules.Count > 0)
            {
                return gameObject.GetComponentsInChildren<Transform>(true)
                    .Where(t => t.gameObject.name.ContainsAny(whiteList) && RulesManager.Instance.IgnoreRules.Find(g => g.ObjectName == t.gameObject.name) == null).ToArray();
            }

            return gameObject.GetComponentsInChildren<Transform>(true)
                .Where(trans => trans.gameObject.name.ContainsAny(whiteList)).ToArray();
        }

        /// <summary>
        /// Toggles all renderes related to the car engine block.
        /// </summary>
        /// <param name="enabled"></param>
        public void EngineCulling(bool enabled)
        {
            if (RulesManager.Instance.SpecialRules.SatsumaIgnoreRenderers || engineBayRenderers.Count == 0) return;

            // Don't disable engine renderers, if the all car's renderers are disabled.
            if (!renderers[0].enabled && !enabled) return;

            // Force enable engine renderers, if the car's hood is not on the car.
            if (engineBayRenderers[0].enabled == false && !IsHoodAttached()) enabled = true;

            // Don't execute the loop, if the first renderer state is the same as for others.
            if (engineBayRenderers[0].enabled == enabled) return;

            for (int i = 0; i < engineBayRenderers.Count; i++)
            {
                try
                {
                    if (engineBayRenderers[i] == null)
                        continue;

                    // Skip renderer if it's root is not Satsuma.
                    if (engineBayRenderers[i].transform.root.gameObject != this.gameObject)
                        continue;
                    
                    // ItemBehaviour - don't disable renderer.
                    if (engineBayRenderers[i].gameObject.GetComponent<ItemBehaviour>())
                    {
                        engineBayRenderers[i].gameObject.GetComponent<ItemBehaviour>().IgnoreRenderer = !enabled;
                    }

                    engineBayRenderers[i].enabled = enabled;

                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, false, "SATSUMA_ENGINE_RENDERER_TOGGLE_ISSUE");
                }
            }
        }

        /// <summary>
        /// Returns true, if any hood is attached to the car.
        /// </summary>
        /// <returns></returns>
        bool IsHoodAttached()
        {
            return pivotHood.childCount > 0;
        }

        /// <summary>
        /// Toggles all renderers of the Satsuma,
        /// </summary>
        /// <param name="enabled"></param>
        void RenderersCulling(bool enabled)
        {
            if (RulesManager.Instance.SpecialRules.SatsumaIgnoreRenderers) enabled = true;

            for (int i = 0; i < renderers.Count; i++)
            {
                try
                {
                    if (renderers[i] == null)
                        continue;

                    // Skip renderer if it's root is not Satsuma.
                    if (renderers[i].transform.root.gameObject != this.gameObject)
                        continue;

                    renderers[i].enabled = enabled;
                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, true, "SATSUMA_RENDERER_TOGGLE_ISSUE");
                }
            }
        }

        /// <summary>
        /// We're forcing to keep his rotation.
        /// For some reason it keeps shaking otherwise.
        /// </summary>
        public void ForceRotation()
        {
            if (MopSettings.Mode == PerformanceMode.Safe) return;
            if (carDynamics == null) return;

            if (!hasBeenMovedByFleetari && !carDynamics.enabled)
            {
                transform.localRotation = lastGoodRotation;
                transform.localPosition = lastGoodPosition;
            }
        }

        /// <summary>
        /// Returns true, if the key object is active.
        /// </summary>
        /// <returns></returns>
        public bool IsKeyInserted()
        {
            return key.activeSelf;
        }

        /// <summary>
        /// Toggles some of the elements of that car, depneding on the of player to Satsuma.
        /// </summary>
        /// <param name="distance"></param>
        public void ToggleElements(float distance)
        {
            if (Toggle == IgnoreToggle)
                distance = 0;

            // Don't disable any elements, if the AI is driving the Satsuma.
            if (drivingAI != null && drivingAI.enabled)
                distance = 0;

            try
            {
                bool onEngine = distance < 2;
                bool onClose = distance <= 10 * MopSettings.ActiveDistanceMultiplicationValue;
                bool onFar = distance <= 20 * MopSettings.ActiveDistanceMultiplicationValue;

                if (IsKeyInserted() || IsSatsumaInInspectionArea || IsMoving())
                {
                    onEngine = true;
                    onClose = true;
                    onFar = true;
                }

                for (int i = 0; i < satsumaOnActionObjects.Count; i++)
                {
                    if (satsumaOnActionObjects[i].FSM == null && satsumaOnActionObjects[i].GameObject == null) continue;

                    if (satsumaOnActionObjects[i].FSM != null)
                    {
                        switch (satsumaOnActionObjects[i].EnableOn)
                        {
                            case SatsumaEnableOn.OnEngine:
                                satsumaOnActionObjects[i].FSM.enabled = onEngine;
                                break;
                            case SatsumaEnableOn.OnPlayerClose:
                                satsumaOnActionObjects[i].FSM.enabled = onClose;
                                break;
                            case SatsumaEnableOn.OnPlayerFar:
                                satsumaOnActionObjects[i].FSM.enabled = onFar;
                                break;
                        }
                    }
                    
                    if (satsumaOnActionObjects[i].GameObject != null)
                    {
                        switch (satsumaOnActionObjects[i].EnableOn)
                        {
                            case SatsumaEnableOn.OnEngine:
                                satsumaOnActionObjects[i].GameObject.SetActive(onEngine);
                                break;
                            case SatsumaEnableOn.OnPlayerClose:
                                satsumaOnActionObjects[i].GameObject.SetActive(onClose);
                                break;
                            case SatsumaEnableOn.OnPlayerFar:
                                satsumaOnActionObjects[i].GameObject.SetActive(onFar);
                                break;
                        }
                    }
                }

                if (onEngine)
                {
                    bumperFixRetries = MaxBumperFixRetries + 1;

                    // This script fixes the issue with bolts staying unbolted, with parts internally being fully bolted.
                    if (maskedFixStages < 2)
                    {
                        switch (maskedFixStages)
                        {
                            case 0:
                                for (int i = 0; i < maskedElements.Count; i++)
                                    maskedElements.ElementAt(i).Key.SetActive(true);
                                break;
                            case 1:
                                for (int i = 0; i < maskedElements.Count; i++)
                                    maskedElements.ElementAt(i).Key.SetActive(maskedElements.ElementAt(i).Value);
                                break;
                        }
                        maskedFixStages++;
                    }
                }
                else
                {
                    cooldownTick.SetActive(false);
                }

                if (onFar)
                {
                    hasBeenMovedByFleetari = false;
                }
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_TOGGLE_ELEMENTS_ERROR");
            }
        }

        /// <summary>
        /// Toggles Satsuma's dashboard illumination.
        /// </summary>
        /// <param name="enabled"></param>
        void ToggleDashIllumination(bool enabled)
        {
            if (dashboardMaterials == null) return;
            if (dashboardMaterials[0].GetFloat("_Intensity") == 0 && !enabled) return;
            else if (dashboardMaterials[0].GetFloat("_Intensity") == 0.2f && enabled) return;

            for (int i = 0; i < dashboardMaterials.Count; i++)
                dashboardMaterials[i].SetFloat("_Intensity", enabled ? 0.2f : 0);
        }

        void DoBumperFix()
        {
            if (bumperFixRetries > MaxBumperFixRetries) return;
            bumperFixRetries++;

            if (rearBumper.transform.parent == null && doBumperFixes)
            {
                GameFixes.Instance.RearBumperFix(rearBumperTrigger, rearBumper);
            }
        }

        public GameObject GetCarBody()
        {
            return transform.Find("Body/car body(xxxxx)").gameObject;
        }

        public void FleetariIsMovingCar()
        {
            hasBeenMovedByFleetari = true;
        }
    }
}
