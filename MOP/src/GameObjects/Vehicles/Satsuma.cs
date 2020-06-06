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
    class Satsuma : Vehicle
    {
        // Satsuma class - made by Konrad "Athlon" Figura
        //
        // This class extends the functionality of Vehicle class, which is tailored for Gifu.
        // It fixes the issue with Gifu's beams being turned on after respawn.

        public static Satsuma instance;

        // If true, prevents Satsuma physics from disabling.
        public bool IsSatsumaInInspectionArea;

        // If is false, the first enabling script will be toggled.
        public bool AfterFirstEnable;

        // Objects that can be toggled.
        readonly Transform[] disableableObjects;

        // Whitelist of object that cannot be toggled.
        readonly string[] whiteList;

        // Renderers.
        // Renderes of the entire car.
        readonly List<Renderer> renderers;
        // Engine bay renderers
        readonly List<Renderer> engineBayRenderers;

        // Pivot transform of hood.
        readonly Transform pivotHood;

        // If this boolean is ticket, Satsuma won't be despawned.
        bool preventDespawnDuringThisSession;

        // Storage system.
        public List<SatsumaTrunk> Trunks;

        // Key object.
        readonly GameObject key;

        // Rear bumper transform.
        readonly GameObject rearBumper;

        // Elements toggling.
        List<GameObject> onEngineOffToggle;
        List<GameObject> onCloseToggle;
        List<PlayMakerFSM> onCloseTogglePlayMaker;
        List<GameObject> onFarToggle;

        /// <summary>
        /// Initialize class
        /// </summary>
        public Satsuma(string gameObject) : base(gameObject)
        {
            instance = this;
            SatsumaScript = this;

            whiteList = Properties.Resources.whitelist_satsuma.Replace("\n", "").Split(',');
            disableableObjects = GetDisableableChilds();

            Toggle = ToggleActive;

            // Get engine bay renderers
            engineBayRenderers = new List<Renderer>();
            Transform block = this.gameObject.transform.Find("Chassis/sub frame(xxxxx)/CarMotorPivot");
            engineBayRenderers = block.GetComponentsInChildren<Renderer>(true).ToList();
            pivotHood = this.gameObject.transform.Find("Body/pivot_hood");

            // Get all the other renderers
            renderers = new List<Renderer>();
            renderers = this.gameObject.transform.GetComponentsInChildren<Renderer>(true)
                .Where(t => !t.gameObject.name.ContainsAny("Sphere", "Capsule", "Cube", "Mokia")).ToList();

            // Ignore Rule
            IgnoreRule vehicleRule = Rules.instance.IgnoreRules.Find(v => v.ObjectName == this.gameObject.name);
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
            bucketDriver.AddComponent<SatsumaSeatsManager>();
            bucketPassanger.AddComponent<SatsumaSeatsManager>();

            // Fix for mechanical wear of the car.
            PlayMakerFSM mechanicalWearFsm = transform.Find("CarSimulation/MechanicalWear").gameObject.GetComponent<PlayMakerFSM>();
            FsmState loadGame = mechanicalWearFsm.FindFsmState("Load game");
            List<FsmStateAction> loadArrayActions = new List<FsmStateAction> { new CustomNullState() };
            loadGame.Actions = loadArrayActions.ToArray();
            loadGame.SaveActions();

            // Fix for engine freezing car.
            GameObject.Find("block(Clone)").AddComponent<SatsumaEngineManager>();

            // Fix for not working handbrake after respawn.
            GameObject.Find("HandBrake").AddComponent<SatsumaHandbrakeManager>();

            // Fixes handbrake lever position.
            PlayMakerFSM handbrakeLeverFsm = transform.Find("MiscParts/HandBrake/handbrake(xxxxx)/handbrake lever")
                .gameObject.GetPlayMakerByName("Use");
            FsmState loadHandbrake = handbrakeLeverFsm.FindFsmState("Load");
            List<FsmStateAction> loadHandbrakeArrayActions = new List<FsmStateAction> { new CustomNullState() };
            loadHandbrake.Actions = loadHandbrakeArrayActions.ToArray();
            loadGame.SaveActions();

            // Get all bolts.
            List<SatsumaBoltsAntiReload> satsumaBoltsAntiReloads = new List<SatsumaBoltsAntiReload>();
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
            foreach (var s in satsumaBoltsAntiReloads)
                Object.Destroy(s);
            satsumaBoltsAntiReloads.Clear();

            // Fixes car body color resetting to default.
            PlayMakerFSM carBodyPaintFsm = transform.Find("Body/car body(xxxxx)").gameObject.GetPlayMakerByName("Paint");
            FsmState carBodyLoadState = carBodyPaintFsm.FindFsmState("Load");
            carBodyLoadState.Actions = new FsmStateAction[] { new CustomNullState() };
            carBodyLoadState.SaveActions();

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
                emptyState1.Insert(0, new CustomStopAction());
                state1.Actions = emptyState1.ToArray();
                state1.SaveActions();

                FsmState loadState = useFsm.FindFsmState("Load");
                List<FsmStateAction> emptyActions = loadState.Actions.ToList();
                emptyActions.Insert(0, new CustomStopAction());
                loadState.Actions = emptyActions.ToArray();
                loadState.SaveActions();

                // Fix for a bug that prevents player detaching the some of the parts.
                if (part.name == "spark plug(Clone)")
                {
                    // Spark plugs save their shit differently (because fuck you)...
                    for (int i = 1; i <= 4; i++)
                    {
                        FsmState installState = useFsm.FindFsmState(i.ToString());
                        FsmStateAction[] installActions = installState.Actions;
                        installActions[1] = new CustomNullState();
                        installState.Actions = installActions;
                        installState.SaveActions();
                    }
                }
                else
                {
                    FsmState installState = useFsm.FindFsmState("Install");
                    FsmStateAction[] installActions = installState.Actions;
                    installActions[2] = new CustomNullState();
                    installState.Actions = installActions;
                    installState.SaveActions();
                }
            }

            // Fix for cd player disabling other vehicles radio.
            Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "cd player(Clone)")
            .transform.Find("ButtonsCD/RadioVolume").gameObject.GetPlayMakerByName("Knob").Fsm.RestartOnEnable = false;
            GameObject.Find("radio(Clone)").transform.Find("ButtonsRadio/RadioVolume").gameObject.GetPlayMakerByName("Knob").Fsm.RestartOnEnable = false;

            // Fix for window grille paint resetting to the default.
            Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "window grille(Clone)" && g.GetPlayMakerByName("Paint") != null)
                .GetPlayMakerByName("Paint").Fsm.RestartOnEnable = false;

            HoodFix();

            // Rear bumper detachng fix.
            rearBumper = GameObject.Find("bumper rear(Clone)");
            if (rearBumper.transform.parent == null)
            {
                GameObject databaseBumper = GameObject.Find("Database/DatabaseBody/Bumper_Rear");
                databaseBumper.SetActive(false);
                databaseBumper.SetActive(true);
                if (databaseBumper.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Bolted").Value)
                {
                    GameObject triggerBumper = transform.Find("Body/trigger_bumper_rear").gameObject;
                    GameObject bumper = GameObject.Find("bumper rear(Clone)");
                    GameFixes.Instance.RearBumperFix(triggerBumper, bumper);
                }
            }

            if (Rules.instance.SpecialRules.ExperimentalSatsumaTrunk)
            {
                // Create trunk trigger.
                Trunks = new List<SatsumaTrunk>();

                // Boot
                GameObject trunkTrigger = new GameObject("MOP_Trunk");
                SatsumaTrunk trunk = trunkTrigger.AddComponent<SatsumaTrunk>();
                trunk.Initialize(new Vector3(0, 0.15f, -1.37f), new Vector3(1.25f, 0.4f, 0.75f), GameObject.Find("bootlid(Clone)").transform.Find("Handles").gameObject);
                Trunks.Add(trunk);

                // Glovebox
                GameObject gloveboxTrigger = new GameObject("MOP_Glovebox");
                SatsumaTrunk glovebox = gloveboxTrigger.AddComponent<SatsumaTrunk>();
                glovebox.Initialize(new Vector3(0.32f, 0.3f, 0.6f), new Vector3(0.3f, 0.12f, 0.1f), GameObject.Find("dashboard(Clone)").transform.Find("glovbox").gameObject);
                Trunks.Add(glovebox);

                foreach (var storage in Trunks)
                    storage.Initialize();
            }
           
            key = transform.Find("Dashboard/Steering/steering_column2/Ignition/Keys/Key").gameObject;

            // Fix for reg plates Z fighting.
            try
            {
                GameObject.Find("bootlid(Clone)").transform.Find("RegPlateRear").gameObject.GetComponent<Renderer>().material.renderQueue = 100;
                GameObject.Find("bumper front(Clone)").transform.Find("RegPlateFront").gameObject.GetComponent<Renderer>().material.renderQueue = 100;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, "SATSUMA_REG_PLATE_FIX_ERROR");
            }

            // Setup stuff that gets disabled using ToggleElements
            onEngineOffToggle = new List<GameObject>();
            onCloseToggle = new List<GameObject>();
            onFarToggle = new List<GameObject>();

            onEngineOffToggle.Add(transform.Find("CarSimulation/MechanicalWear").gameObject);
            onEngineOffToggle.Add(transform.Find("CarSimulation/Fixes").gameObject);
            onEngineOffToggle.Add(transform.Find("CarSimulation/DynoDistance").gameObject);
            onEngineOffToggle.Add(transform.Find("CarSimulation/RandomBolt").gameObject);
            
            onCloseToggle.Add(transform.Find("RainScript").gameObject);
            onCloseToggle.Add(transform.Find("DriverHeadPivot").gameObject);
            onCloseToggle.Add(transform.Find("AirIntake").gameObject);
            
            onFarToggle.Add(transform.Find("Chassis").gameObject);

            onCloseTogglePlayMaker = new List<PlayMakerFSM>();
            onCloseTogglePlayMaker.Add(this.gameObject.GetPlayMakerByName("ButtonShifter"));
            onCloseTogglePlayMaker.Add(this.gameObject.GetPlayMakerByName("SteerLimit"));

            // Replace on assemble sound playing with custom script.
            PlayMakerFSM blockBoltCheck = GameObject.Find("block(Clone)").GetPlayMakerByName("BoltCheck");
            FsmState boltsONState = blockBoltCheck.FindFsmState("Bolts ON");
            FsmStateAction[] boltsONActions = boltsONState.Actions;
            boltsONActions[1] = new MasterAudioAssembleCustom();
            boltsONState.Actions = boltsONActions;
            boltsONState.SaveActions();
        }

        /// <summary>
        /// Enable or disable car
        /// </summary>
        new void ToggleActive(bool enabled)
        {
            // Applying hood fix after first enabling.
            if (!AfterFirstEnable && enabled)
            {
                AfterFirstEnable = true;
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

            if (preventDespawnDuringThisSession || MopFsmManager.IsRepairshopJobOrdered())
            {
                enabled = true;
                preventDespawnDuringThisSession = true;
            }

            for (int i = 0; i < disableableObjects.Length; i++)
            {
                if (disableableObjects[i] == null)
                    continue;

                disableableObjects[i].gameObject.SetActive(enabled);
            }

            ToggleAllRenderers(enabled);
        }

        /// <summary>
        /// Get list of disableable childs.
        /// It looks for objects that contian the name from the whiteList
        /// </summary>
        /// <returns></returns>
        internal Transform[] GetDisableableChilds()
        {
            return gameObject.GetComponentsInChildren<Transform>(true)
                .Where(trans => trans.gameObject.name.ContainsAny(whiteList)).ToArray();
        }

        public void ToggleEngineRenderers(bool enabled)
        {
            if (Rules.instance.SpecialRules.SatsumaIgnoreRenderers || engineBayRenderers.Count == 0
                || engineBayRenderers[0].enabled == enabled || !IsHoodAttached()) return;

            // Don't disable engine renderers, if the all car's renderers are disabled.
            if (!renderers[0].enabled && !enabled) return;

            for (int i = 0; i < engineBayRenderers.Count; i++)
            {
                try
                {
                    if (engineBayRenderers[i] == null)
                        continue;

                    // Skip renderer if it's root is not Satsuma.
                    if (engineBayRenderers[i].transform.root.gameObject != this.gameObject)
                        continue;

                    engineBayRenderers[i].enabled = enabled;
                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, "SATSUMA_ENGINE_RENDERER_TOGGLE_ISSUE");
                }
            }
        }

        bool IsHoodAttached()
        {
            return pivotHood.childCount > 0;
        }

        void ToggleAllRenderers(bool enabled)
        {
            if (Rules.instance.SpecialRules.SatsumaIgnoreRenderers) return;

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
                    ExceptionManager.New(ex, "SATSUMA_RENDERER_TOGGLE_ISSUE");
                }
            }
        }

        /// <summary>
        /// Yeah, we're literally forcing this fucker to stay rotated at the last good rotation.
        /// For some fucking reason it keeps spinning and shit.
        /// </summary>
        public void ForceFuckingRotation()
        {
            if (carDynamics == null) return;

            if (!carDynamics.enabled)
            {
                transform.localRotation = lastGoodRotation;
                transform.localPosition = lastGoodPosition;
            }
        }

        public void HoodFix()
        {
            GameFixes.Instance.HoodFix(transform.Find("Body/pivot_hood"), transform.Find("MiscParts/trigger_battery"), transform.Find("MiscParts/pivot_battery"));
        }

        public bool IsKeyInserted()
        {
            return key.activeSelf;
        }

        public void ToggleElements(float distance)
        {
            if (Toggle == IgnoreToggle)
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

                for (int i = 0; i < onEngineOffToggle.Count; i++)
                    onEngineOffToggle[i].SetActive(onEngine);

                for (int i = 0; i < onCloseToggle.Count; i++)
                    onCloseToggle[i].SetActive(onClose);

                for (int i = 0; i < onCloseTogglePlayMaker.Count; i++)
                    onCloseTogglePlayMaker[i].enabled = onClose;

                for (int i = 0; i < onFarToggle.Count; i++)
                    onFarToggle[i].SetActive(onFar);
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, "SATSUMA_TOGGLE_ELEMENTS_ERROR");
            }
        }
    }
}
