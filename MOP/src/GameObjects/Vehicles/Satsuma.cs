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

        public bool IsSatsumaInInspectionArea;
        public bool AfterFirstEnable;

        readonly Transform[] disableableObjects;
        readonly string[] whiteList;

        readonly List<Renderer> renderers;
        readonly List<Renderer> engineBayRenderers;
        
        readonly Transform pivotHood;

        bool preventDespawnDuringThisSession;

        public List<SatsumaTrunk> Trunks;
        readonly Transform key;

        /// <summary>
        /// Initialize class
        /// </summary>
        public Satsuma(string gameObject) : base(gameObject)
        {
            instance = this;
            satsumaScript = this;

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

            lastGoodRotation = transform.rotation;
            lastGoodPosition = transform.position;

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
            
            // Fix for doors getting jammed.
            GameObject.Find("door right(Clone)").AddComponent<SatsumaDoorManager>();
            GameObject.Find("door left(Clone)").AddComponent<SatsumaDoorManager>();
            
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
                    parent.gameObject.AddComponent<SatsumaBoltsAntiReload>();
            }

            // Halfshafts hook.
            GameObject[] halfshafts = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "halfshaft(xxxxx)").ToArray();
            foreach (GameObject halfshaft in halfshafts)
            {
                if (halfshaft.GetComponent<SatsumaBoltsAntiReload>() == null)
                    halfshaft.AddComponent<SatsumaBoltsAntiReload>();
            }

            // Engine Block.
            GameObject.Find("block(Clone)").AddComponent<SatsumaBoltsAntiReload>();

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
            GameObject.Find("cd player(Clone)").transform.Find("ButtonsCD/RadioVolume").gameObject.GetPlayMakerByName("Knob").Fsm.RestartOnEnable = false;
            GameObject.Find("radio(Clone)").transform.Find("ButtonsRadio/RadioVolume").gameObject.GetPlayMakerByName("Knob").Fsm.RestartOnEnable = false;

            // Create trunk trigger.
            if (Rules.instance.SpecialRules.ExperimentalSatsumaTrunk)
            {
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
            }

            if (Rules.instance.SpecialRules.ExperimentalOptimization)
                key = transform.Find("Dashboard/Steering/steering_column2/Ignition/Keys/Key");
        }

        /// <summary>
        /// Enable or disable car
        /// </summary>
        new void ToggleActive(bool enabled)
        {
            // Don't run the code, if the value is the same
            if (gameObject == null || disableableObjects[0].gameObject.activeSelf == enabled) return;

            if (!enabled)
            {
                lastGoodRotation = transform.rotation;
                lastGoodPosition = transform.position;
            }

            if (MopSettings.SatsumaTogglePhysicsOnly) return;

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

            // Applying hood fix after first enabling.
            if (!AfterFirstEnable && enabled)
            {
                if (Trunks != null)
                {
                    foreach (var trunk in Trunks)
                        trunk.Initialize();
                }

                AfterFirstEnable = true;
                HoodFix();
            }
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
            if (Rules.instance.SpecialRules.SatsumaIgnoreRenderers || MopSettings.SatsumaTogglePhysicsOnly) return;

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
                transform.rotation = lastGoodRotation;
                transform.position = lastGoodPosition;
            }
        }

        public void HoodFix()
        {
            WorldManager.instance.HoodFix(transform.Find("Body/pivot_hood"), transform.Find("MiscParts/trigger_battery"), transform.Find("MiscParts/pivot_battery"));
        }

        public bool IsKeyInserted()
        {
            return key.gameObject.activeSelf;
        }
    }
}
