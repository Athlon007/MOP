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
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using MSCLoader;

using MOP.Managers;
using MOP.Common;
using MOP.Common.Enumerations;
using MOP.FSM;
using MOP.FSM.Actions;
using MOP.Items;
using MOP.Vehicles;
using MOP.Vehicles.Managers;
using MOP.Vehicles.Managers.SatsumaManagers;
using MOP.Helpers;

namespace MOP
{
    class GameFixes : MonoBehaviour
    {
        // This class fixes shit done by ToplessGun.
        // Also some caused by MOP itself...

        static GameFixes instance;
        public static GameFixes Instance { get => instance; }

        public bool HoodFixDone { get; private set; }
        public bool RearBumperFixDone { get; private set; }

        public GameFixes()
        {
            instance = this;
        }

        public void MainFixes()
        {
            // GT Grille resetting fix.
            try
            {
                PlayMakerFSM[] gtGrille = GameObject.Find("grille gt(Clone)").GetComponents<PlayMakerFSM>();
                foreach (var fsm in gtGrille)
                    fsm.Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "GT_GRILLE_ERROR");
            }

            // Random fixes.
            try
            {
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

                // Fix for floppies at Jokke's new house
                while (perajarvi.transform.Find("TerraceHouse/diskette(itemx)") != null)
                {
                    Transform diskette = perajarvi.transform.Find("TerraceHouse/diskette(itemx)");
                    if (diskette != null && diskette.parent != null)
                        diskette.parent = null;
                }

                // Fix for Jokke's house furnitures clipping through floor
                perajarvi.transform.Find("TerraceHouse/Apartments/Colliders").parent = null;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "PERAJARVI_FIXES_ERROR");
            }

            // Fix for cottage items disappearing when moved
            try
            {
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
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "ITEMS_FIXES_ERROR");
            }

            // Applying a script to vehicles that can pick up and drive the player as a passanger to his house.
            // This script makes it so when the player enters the car, the parent of the vehicle is set to null.
            try
            {
                GameObject fittan = GameObject.Find("TRAFFIC").transform.Find("VehiclesDirtRoad/Rally/FITTAN").gameObject;
                GameObject kuski = GameObject.Find("NPC_CARS").transform.Find("KUSKI").gameObject;
                FsmHook.FsmInject(fittan.transform.Find("PlayerTrigger/DriveTrigger").gameObject, "Player in car", () => fittan.transform.parent = null);
                FsmHook.FsmInject(kuski.transform.Find("PlayerTrigger/DriveTrigger").gameObject, "Player in car", () => kuski.transform.parent = null);

            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "TAXI_MANAGERS_ERROR");
            }

            // Fixed Ventii bet resetting to default on cabin load.
            try
            {
                GameObject.Find("CABIN").transform.Find("Cabin/Ventti/Table/GameManager").GetPlayMakerByName("Use").Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VENTII_FIX_ERROR");
            }

            // Junk cars - setting Load game to null.
            int junkCarCounter = 1;
            try
            {
                for (junkCarCounter = 1; GameObject.Find($"JunkCar{junkCarCounter}") != null; junkCarCounter++)
                {
                    GameObject junk = GameObject.Find($"JunkCar{junkCarCounter}");
                    junk.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

                    WorldObjectManager.Instance.Add(junk, DisableOn.Distance);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, $"JUNK_CARS_{junkCarCounter}_ERROR");;
            }

            // Toggle Humans (apart from Farmer and Fighter2).
            try
            {
                foreach (Transform t in GameObject.Find("HUMANS").GetComponentsInChildren<Transform>())
                {
                    if (t.gameObject.name.EqualsAny("HUMANS", "Fighter2", "Farmer"))
                        continue;

                    WorldObjectManager.Instance.Add(t.gameObject, DisableOn.Distance);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "HUMANS_ERROR");
            }

            // Fixes wasp hives resetting to on load values.
            try
            {
                GameObject[] wasphives = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "WaspHive").ToArray();
                foreach (GameObject wasphive in wasphives)
                {
                    wasphive.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "WASPHIVES_ERROR");
            }

            // Disabling the script that sets the kinematic state of Satsuma to False.
            try
            {
                GameObject hand = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/Hand");
                PlayMakerFSM pickUp = hand.GetPlayMakerByName("PickUp");

                pickUp.FindFsmState("Drop part").Actions = pickUp.FindFsmState("Drop part").Actions.RemoveAt(0);
                pickUp.FindFsmState("Drop part 2").Actions = pickUp.FindFsmState("Drop part 2").Actions.RemoveAt(0);
                pickUp.FindFsmState("Tool picked").Actions = pickUp.FindFsmState("Tool picked").Actions.RemoveAt(2);
                pickUp.FindFsmState("Drop tool").Actions = pickUp.FindFsmState("Drop tool").Actions.RemoveAt(0);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_HAND_BS_FIX");
            }

            // Preventing mattres from being disabled.
            try
            {
                Transform mattres = GameObject.Find("DINGONBIISI").transform.Find("mattres");
                if (mattres != null)
                    mattres.parent = null;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "MANSION_MATTRES_ERROR");
            }

            // Item anti clip for cottage.
            try
            {
                GameObject area = new GameObject("MOP_ItemAntiClip");
                area.transform.position = new Vector3(-848.3f, -5.4f, 505.5f);
                area.transform.eulerAngles = new Vector3(0, 343.0013f, 0);
                area.AddComponent<ItemAntiClip>();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "ITEM_ANTICLIP_ERROR");
            }

            // Z-fighting fix for wristwatch.
            try
            {
                GameObject player = GameObject.Find("PLAYER");

                if (player)
                {
                    Transform hour = player.transform.Find("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Watch/Animate/BreathAnim/WristwatchHand/Clock/Pivot/Hour/hour");
                    if (hour)
                    {
                        hour.GetComponent<Renderer>().material.renderQueue = 3001;
                    }

                    Transform minute = player.transform.Find("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Watch/Animate/BreathAnim/WristwatchHand/Clock/Pivot/Minute/minute");
                    if (minute)
                    {
                        minute.GetComponent<Renderer>().material.renderQueue = 3002;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "HANDWATCH_RENDERER_QUEUE_ERROR");
            }

            // Adds roll fix to the bus.
            try
            {
                GameObject bus = GameObject.Find("BUS");
                if (bus)
                {
                    bus.AddComponent<BusRollFix>();
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "BUS_ROLL_FIX_ERROR");
            }

            // Fixes bedroom window wrap resetting to default value.
            try
            {
                Transform triggerWindowWrap = GameObject.Find("YARD").transform.Find("Building/BEDROOM1/trigger_window_wrap");
                if (triggerWindowWrap != null)
                    triggerWindowWrap.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TRIGGER_WINDOW_WRAP_ERROR");
            }

            // Fixes diskette ejecting not wokring.
            try
            {
                Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "TriggerDiskette")
                    .GetPlayMakerByName("Assembly").Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TRIGGER_DISKETTE_ERROR");
            }

            // Fixed computer memory resetting.
            try
            {
                Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "TriggerPlayMode").GetPlayMakerByName("PlayerTrigger").Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TRIGGER_DISKETTE_ERROR");
            }

            // Fixes berry picking skill resetting to default.
            try
            {
                GameObject.Find("JOBS").transform.Find("StrawberryField").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "STRAWBERRY_FIELD_FSM");
            }

            // GrandmaHiker fixes.
            try
            {
                GameObject grandmaHiker = GameObject.Find("GrannyHiker");
                if (grandmaHiker)
                {
                    GameObject skeleton = grandmaHiker.transform.Find("Char/skeleton").gameObject;

                    PlayMakerFSM logicFSM = grandmaHiker.GetPlayMakerByName("Logic");

                    FsmState openDoorFsmState = logicFSM.FindFsmState("Open door");
                    List<FsmStateAction> openDoorActions = openDoorFsmState.Actions.ToList();
                    openDoorActions.Add(new GrandmaHiker(skeleton, false));
                    openDoorFsmState.Actions = openDoorActions.ToArray();

                    FsmState setMass2State = logicFSM.FindFsmState("Set mass 2");
                    List<FsmStateAction> setMass2Actions = setMass2State.Actions.ToList();
                    setMass2Actions.Add(new GrandmaHiker(skeleton, true));
                    setMass2State.Actions = setMass2Actions.ToArray();
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "GRANDMA_HIKER_FIXES");
            }

            // Cabin door resetting fix.
            try
            {
                GameObject.Find("CABIN").transform.Find("Cabin/Door/Pivot/Handle").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "CABIN_DOOR_RESET_FIX_ERROR");
            }

            // Construction site
            try
            {
                Transform construction = GameObject.Find("PERAJARVI").transform.Find("ConstructionSite");
                if (construction)
                {
                    construction.parent = null;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "PERAJARVI_CONSTRUCTION_ERROR");
            }

            // Satsuma parts trigger fix.
            try
            {
                gameObject.AddComponent<SatsumaTriggerFixer>();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_TRIGGER_FIXER_ERROR");
            }

            // MailBox fix
            try
            {
                foreach (var g in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "MailBox"))
                {
                    Transform hatch = g.transform.Find("BoxHatch");
                    if (hatch)
                    {
                        hatch.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "MAILBOX_ERROR");
            }

            ModConsole.Print("[MOP] Finished applying fixes");
        }

        /// <summary>
        /// Fixes hood popping out of the car on game load.
        /// </summary>
        public void HoodFix(Transform hoodPivot, Transform batteryPivot, Transform batteryTrigger)
        {
            StartCoroutine(HoodFixCoroutine(hoodPivot, batteryPivot, batteryTrigger));
        }

        IEnumerator HoodFixCoroutine(Transform hoodPivot, Transform batteryPivot, Transform batteryTrigger)
        {
            yield return new WaitForSeconds(2);

            // Hood
            Transform hood = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "hood(Clone)").transform;
            CustomPlayMakerFixedUpdate hoodFixedUpdate = hood.gameObject.AddComponent<CustomPlayMakerFixedUpdate>();

            // Fiber Hood
            GameObject fiberHood = Resources.FindObjectsOfTypeAll<GameObject>()
                .First(obj => obj.name == "fiberglass hood(Clone)"
                && obj.GetComponent<PlayMakerFSM>() != null
                && obj.GetComponent<MeshCollider>() != null);

            int retries = 0;
            if (FsmManager.IsStockHoodBolted() && hood.parent != hoodPivot)
            {
                hood.gameObject.SetActive(true);

                while (hood.parent != hoodPivot)
                {
                    // Satsuma got disabled while trying to fix the hood.
                    // Attempt to fix it later.
                    if (!hoodPivot.gameObject.activeSelf)
                    {
                        yield break;
                    }

                    FsmManager.ForceHoodAssemble();
                    yield return null;

                    // If 10 retries failed, quit the loop.
                    retries++;
                    if (retries == 10)
                    {
                        break;
                    }
                }
            }

            hoodFixedUpdate.StartFixedUpdate();

            if (fiberHood != null && FsmManager.IsFiberHoodBolted() && fiberHood.transform.parent != hoodPivot)
            {
                retries = 0;
                while (fiberHood.transform.parent != hoodPivot)
                {
                    // Satsuma got disabled while trying to fix the hood.
                    // Attempt to fix it later.
                    if (!hoodPivot.gameObject.activeSelf)
                    {
                        Satsuma.Instance.AfterFirstEnable = false;
                        yield break;
                    }

                    FsmManager.ForceHoodAssemble();
                    yield return null;

                    // If 10 retries failed, quit the loop.
                    retries++;
                    if (retries == 60)
                    {
                        break;
                    }
                }
            }

            hood.gameObject.AddComponent<SatsumaBoltsAntiReload>();
            fiberHood.gameObject.AddComponent<SatsumaBoltsAntiReload>();

            // Adds delayed initialization for hood hinge.
            if (hood.gameObject.GetComponent<DelayedHingeManager>() == null)
                hood.gameObject.AddComponent<DelayedHingeManager>();

            // Fix for hood not being able to be closed.
            if (hood.gameObject.GetComponent<SatsumaCustomHoodUse>() == null)
                hood.gameObject.AddComponent<SatsumaCustomHoodUse>();

            // Fix for battery popping out.
            if (FsmManager.IsBatteryInstalled() && batteryPivot.parent == null)
            {
                batteryTrigger.gameObject.SetActive(true);
                batteryTrigger.gameObject.GetComponent<PlayMakerFSM>().SendEvent("ASSEMBLE");
            }

            HoodFixDone = true;
        }

        public void RearBumperFix(GameObject triggerBumper, GameObject bumper)
        {
            StartCoroutine(RearBumperCoroutine(triggerBumper, bumper));
        }

        IEnumerator RearBumperCoroutine(GameObject triggerBumper, GameObject bumper)
        {
            yield return new WaitForSeconds(1);
            bumper.GetComponent<ItemBehaviour>().enabled = false;
            
            Rigidbody rb = bumper.GetComponent<Rigidbody>();
            float originalMass = rb.mass;
            rb.mass = .1f;
            
            yield return new WaitForSeconds(1);

            triggerBumper.SetActive(true);
            yield return null;
            PlayMakerFSM assemblyTrigger = triggerBumper.GetPlayMakerByName("Assembly");
            assemblyTrigger.Fsm.Variables.GetFsmBool("Setup").Value = true;
            triggerBumper.SetActive(false);
            yield return null;
            triggerBumper.SetActive(true);
            yield return null;

            int childsNumber = bumper.transform.Find("Bolts").childCount;
            for (int i = 0; i < childsNumber; i++)
            {
                PlayMakerFSM fsm = bumper.transform.Find("Bolts").GetChild(i).gameObject.GetComponent<PlayMakerFSM>();
                fsm.FsmVariables.GetFsmInt("Stage").Value = 7;
                yield return null;
                fsm.SendEvent("ASSEMBLE");
                yield return null;
                for (int j = 0; j < 8; j++)
                {
                    fsm.SendEvent("TIGHTEN");
                    yield return null;
                }
                fsm.SendEvent("FINISHED");
            }

            bumper.GetPlayMakerByName("BoltCheck").Fsm.GetFsmFloat("Tightness").Value = 14;
            yield return new WaitForSeconds(1);

            bumper.GetComponent<ItemBehaviour>().enabled = false;
            rb.mass = originalMass;
        }
    }
}
