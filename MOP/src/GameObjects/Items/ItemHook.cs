﻿// Modern Optimization Plugin
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOP
{
    class ItemHook : MonoBehaviour
    {
        // This MonoBehaviour hooks to all items from shop and other interactable ones. (Such as sausages, or beer cases)
        // ObjectHook class by Konrad "Athlon" Figura

        /// <summary>
        /// PlayMakerFSM script of the object
        /// </summary>
        PlayMakerFSM playMakerFSM;

        /// <summary>
        /// Rigidbody of this object.
        /// </summary>
        Rigidbody rb;

        /// <summary>
        /// Object's renderer
        /// </summary>
        Renderer renderer;

        Vector3 position;

        bool firstLoad;

        public bool IsInHood;

        public ItemHook()
        {
            Toggle = ToggleActive;

            IgnoreRule rule = Rules.instance.IgnoreRules.Find(f => f.ObjectName == this.gameObject.name);
            if (rule != null)
            {
                Toggle = ToggleActiveOldMethod;

                if (rule.TotalIgnore)
                {
                    Destroy(this);
                    return;
                }
            }

            // Add self to the MinorObjects.objectHooks list
            Items.instance.Add(this);

            // Get object's components
            rb = GetComponent<Rigidbody>();
            playMakerFSM = GetComponent<PlayMakerFSM>();
            renderer = GetComponent<Renderer>();

            // From PlayMakerFSM, find states that contain one of the names that relate to destroying object,
            // and inject RemoveSelf void.
            if (playMakerFSM != null)
            {
                foreach (var st in playMakerFSM.FsmStates)
                {
                    switch (st.Name)
                    {
                        case "Destroy self":
                            FsmHook.FsmInject(this.gameObject, "Destroy self", RemoveSelf);
                            break;
                        case "Destroy":
                            FsmHook.FsmInject(this.gameObject, "Destroy", RemoveSelf);
                            break;
                        case "Destroy 2":
                            FsmHook.FsmInject(this.gameObject, "Destroy 2", RemoveSelf);
                            break;
                    }
                }
            }

            // If the item is a shopping bag, hook the RemoveSelf to "Is garbage" FsmState
            if (gameObject.name.Contains("shopping bag"))
            {
                FsmHook.FsmInject(this.gameObject, "Is garbage", RemoveSelf);

                // Destroys empty shopping bags appearing at the back of the yard.
                PlayMakerArrayListProxy list = gameObject.GetComponent<PlayMakerArrayListProxy>();
                if (list.arrayList.Count == 0)
                {
                    Items.instance.Remove(this);
                    Destroy(this.gameObject);
                }
            }

            // If the item is beer case, hook the DestroyBeerBottles void uppon removing a bottle.
            if (gameObject.name.StartsWith("beer case"))
            {
                FsmHook.FsmInject(this.gameObject, "Remove bottle", DestroyBeerBottles);
                FsmHook.FsmInject(this.gameObject, "Remove bottle", HookBottles);
            }

            // If ignore, disable renderer
            if (rule != null)
                renderer = null;

            position = transform.position;

            // Fixes a bug which would prevent player from putting on the helmet, after taking it off.
            if (this.gameObject.name == "helmet(itemx)")
                return;

            // We're preventing the execution of State 1 and Load,
            // because these two reset the variables of the item
            // (such as position, state or rotation).
            PlayMakerFSM useFsm = gameObject.GetPlayMakerByName("Use");
            if (useFsm != null)
            {
                useFsm.Fsm.RestartOnEnable = false;
                FsmState state1 = useFsm.FindFsmState("State 1");
                if (state1 != null)
                {
                    List<FsmStateAction> emptyState1 = state1.Actions.ToList();
                    emptyState1.Insert(0, new CustomStopAction());
                    state1.Actions = emptyState1.ToArray();
                    state1.SaveActions();
                }

                FsmState loadState = useFsm.FindFsmState("Load");
                if (loadState != null)
                {
                    List<FsmStateAction> emptyActions = loadState.Actions.ToList();
                    emptyActions.Insert(0, new CustomStopAction());
                    loadState.Actions = emptyActions.ToArray();
                    loadState.SaveActions();
                }
            }
        }

        // Triggered before the object is destroyed.
        // Removes self from MinorObjects.instance.objectHooks.
        public void RemoveSelf()
        {
            Items.instance.Remove(this);
        }

        public delegate void ToggleHandler(bool enabled);
        public ToggleHandler Toggle;

        /// <summary>
        /// Doesn't toggle object itself, rather the Rigidbody and Renderer.
        /// </summary>
        /// <param name="enabled"></param>
        void ToggleActive(bool enabled)
        {
            try
            {
                if (!firstLoad)
                {
                    if (transform.position.y < -100 && transform.position.x != 0 && transform.position.z != 0)
                        transform.position = new Vector3(0, 2, 0);

                    firstLoad = true;
                }

                if (IsInHood)
                    return;

                if (gameObject.activeSelf == enabled)
                    return;

                // Don't disabler wheels that are attached to the car.
                if (this.gameObject.name == "wheel_regula")
                {
                    Transform root = this.gameObject.transform.parent;
                    if (root != null && root.gameObject.name == "pivot_wheel_standard")
                        return;
                }

                // Fix for batteries popping out of the car.
                if (this.gameObject.name == "battery(Clone)" && this.gameObject.transform.parent.gameObject.name == "pivot_battery")
                    return;

                if (this.gameObject.name == "helmet(itemx)" && MopFsmManager.PlayerHelmetOn())
                {
                    return;
                }

                // Check if item is in CarryMore inventory.
                // If so, ignore that item.
                if (CompatibilityManager.instance.CarryMore && transform.position.y < -900)
                    return;

                gameObject.SetActive(enabled);
            }
            catch { }
        }

        void ToggleActiveOldMethod(bool enabled)
        {
            try
            {
                if (rb == null || rb.useGravity == enabled)
                    return;

                if (this.gameObject.name == "wheel_regula")
                {
                    Transform root = this.gameObject.transform.parent;
                    if (root != null && root.gameObject.name == "pivot_wheel_standard")
                        return;
                }

                // Fix for batteries popping out of the car.
                if (this.gameObject.name == "battery(Clone)" && this.gameObject.transform.parent.gameObject.name == "pivot_battery")
                    return;

                // Check if item is in CarryMore inventory.
                // If so, ignore that item.
                if (CompatibilityManager.instance.CarryMore && transform.position.y < -900)
                    return;

                rb.detectCollisions = enabled;
                rb.isKinematic = !enabled;
                rb.useGravity = enabled;

                if (enabled)
                {
                    transform.position = position;
                    rb.velocity = Vector3.zero;
                }
                else
                    position = transform.position;

                // Disable object's renderer on distance
                if (renderer != null)
                    renderer.enabled = enabled;
            }
            catch { }
        }
        
        /// <summary>
        /// Used when this item is a beer case.
        /// Starts the coroutine that initializes Items.DestroyEmptyBottles after 7 seconds.
        /// </summary>
        void DestroyBeerBottles()
        {
            // If the setting is not enabled, return.
            if (!MopSettings.RemoveEmptyBeerBottles)
                return;

            // If Bottle Recycling mod is present, prevent destroying beer bottles
            if (Rules.instance.SpecialRules.DontDestroyEmptyBeerBottles)
            {
                ModConsole.Print("<color=yellow>[MOP] Beer bottles won't be destroyed, because one or more mods prevent it. " +
                    "Disable 'Destroy empty beer bottles' in the MOP settings.</color>");
                return;
            }

            if (currentBottleDestroyerRoutine != null)
            {
                StopCoroutine(currentBottleDestroyerRoutine);
            }

            currentBottleDestroyerRoutine = BottleDestroyerRoutine();
            StartCoroutine(currentBottleDestroyerRoutine);
        }

        IEnumerator currentBottleDestroyerRoutine;
        IEnumerator BottleDestroyerRoutine()
        {
            yield return new WaitForSeconds(7);
            Items.instance.DestroyBeerBottles();
        }

        /// <summary>
        /// Used when this item is a beer case.
        /// Starts the coroutine that initializes Items.HookEmptyBeerBottles after 7 seconds.
        /// </summary>
        void HookBottles()
        {
            if (currentBottleHooker != null)
            {
                StopCoroutine(currentBottleHooker);
            }

            currentBottleHooker = BottleHooker();
            StartCoroutine(currentBottleHooker);
        }

        IEnumerator currentBottleHooker;
        IEnumerator BottleHooker()
        {
            yield return new WaitForSeconds(7);
            Items.instance.HookEmptyBeerBottles();
        }

        public float GetMass()
        {
            return rb.mass;
        }
    }
}
