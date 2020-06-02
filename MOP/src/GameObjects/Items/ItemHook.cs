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
        
        bool firstLoad;

        /// <summary>
        /// Rigidbody of this object.
        /// </summary>
        readonly Rigidbody rb;

        /// <summary>
        /// Object's renderer
        /// </summary>
        readonly Renderer renderer;

        Vector3 position;

        // Used by Satsuma storage system.
        public bool IsInStorage;

        public bool DontDisable;

        FsmBool batteryOnCharged;

        FsmFloat floorJackTriggerY;

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

            // Use the old method, if for some reason item cannot be disabled.
            if (this.gameObject.name.EqualsAny("fish trap(itemx)", "bucket(itemx)", "pike(itemx)", "envelope(xxxxx)", "lottery ticket(xxxxx)"))
            {
                Toggle = ToggleActiveOldMethod;
            }

            // Add self to the MinorObjects.objectHooks list
            Items.instance.Add(this);

            // Get object's components
            rb = GetComponent<Rigidbody>();
            PlayMakerFSM playMakerFSM = GetComponent<PlayMakerFSM>();
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
            {
                renderer = null;
            }

            position = transform.position;

            // Fixes a bug which would prevent player from putting on the helmet, after taking it off.
            if (this.gameObject.name == "helmet(itemx)")
            {
                return;
            }

            if (this.gameObject.name.EqualsAny("floor jack(itemx)", "car jack(itemx)"))
            {
                floorJackTriggerY = gameObject.transform.Find("Trigger").gameObject.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("Y");
            }

            // We're preventing the execution of State 1 and Load,
            // because these two reset the variables of the item
            // (such as position, state or rotation).
            FsmFixes();

            // HACK: For some reason the trigger that's supposed to fix tire job not working doesn't really work on game load,
            // toggle DontDisable to true, if tire is close to repair shop cash register.
            if (this.gameObject.name.StartsWith("wheel_") && Vector3.Distance(gameObject.transform.position, GameObject.Find("REPAIRSHOP").transform.Find("LOD/Store/ShopCashRegister").position) < 5)
            {
                DontDisable = true;
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
                // If the item has fallen under the detection range of the game's built in garbage collector,
                // teleport that item manually to the landfill.
                if (!firstLoad)
                {
                    if (transform.position.y < -100 && transform.position.x != 0 && transform.position.z != 0)
                        transform.position = GameObject.Find("LostSpawner").transform.position;

                    firstLoad = true;
                }

                // This is for the hood system.
                // If the item is stored in the Satsuma's storages (trunk or glovebox),
                // the storage itself toggles the item.
                if (IsInStorage)
                {
                    return;
                }

                if (DontDisable)
                {
                    ToggleActiveOldMethod(enabled);
                    return;
                }

                // Don't execute rest of the code, if the enabled is the same as activeSelf.
                if (gameObject.activeSelf == enabled)
                {
                    return;
                }

                // Don't toggle, if the item is attached to Satsuma.
                if (transform.root.gameObject.name == "SATSUMA(557kg, 248)")
                {
                    return;
                }

                switch (gameObject.name)
                {
                    // Don't disable wheels that are attached to the car.
                    case "wheel_regula":
                        Transform root = this.gameObject.transform.parent;
                        if (root != null && root.gameObject.name == "pivot_wheel_standard")
                            return;
                        break;
                    // Fix for batteries popping out of the car.
                    case "battery(Clone)":
                        if (gameObject.transform.parent.gameObject.name == "pivot_battery")
                            return;

                        // Don't toggle if battery is left on charger.
                        if (!enabled && batteryOnCharged.Value)
                            return;

                        break;
                    // Don't disable the helmet, if player has put it on.
                    case "helmet(itemx)":
                        if (Vector3.Distance(gameObject.transform.position, WorldManager.instance.GetPlayer().position) < 5)
                            return;
                        break;
                    // Don't despawn floor or car jack if it's not in it's default position.
                    case "floor jack(itemx)":
                        if (floorJackTriggerY.Value >= 0.15f)
                            return;
                        break;
                    case "car jack(itemx)":
                        if (floorJackTriggerY.Value >= 0.15f)
                            return;
                        break;
                }

                // Check if item is in CarryMore inventory.
                // If so, ignore that item.
                if (CompatibilityManager.CarryMore && transform.position.y < -900)
                {
                    return;
                }

                gameObject.SetActive(enabled);
            }
            catch { }
        }

        void ToggleActiveOldMethod(bool enabled)
        {
            try
            {
                // If the item has fallen under the detection range of the game's built in garbage collector,
                // teleport that item manually to the landfill.
                if (!firstLoad)
                {
                    if (transform.position.y < -100 && transform.position.x != 0 && transform.position.z != 0)
                        transform.position = GameObject.Find("LostSpawner").transform.position;

                    firstLoad = true;
                }

                if (rb == null || rb.useGravity == enabled)
                {
                    return;
                }

                if (this.gameObject.name == "wheel_regula")
                {
                    Transform root = this.gameObject.transform.parent;
                    if (root != null && root.gameObject.name == "pivot_wheel_standard")
                    {
                        return;
                    }
                }

                // Fix for batteries popping out of the car.
                if (this.gameObject.name == "battery(Clone)" && this.gameObject.transform.parent.gameObject.name == "pivot_battery")
                {
                    return;
                }

                // Check if item is in CarryMore inventory.
                // If so, ignore that item.
                if (CompatibilityManager.CarryMore && transform.position.y < -900)
                {
                    return;
                }

                // CD Player Enhanced mod
                if (this.gameObject.name.StartsWith("cd") && this.transform.parent != null)
                {
                    // Prevent CDs to clip through CD Case
                    if (this.gameObject.name == "cd(itemy)" && this.transform.parent.name == "PivotCD")
                        return;

                    // Prevent CDs from clipping through the Radio
                    if (this.gameObject.name == "cd(itemy)" && this.transform.parent.name == "cd_sled_pivot")
                        return;

                    // Prevents CD cases from clipping through the CD rack
                    if (this.gameObject.name.StartsWith("cd case") && this.transform.parent.name.StartsWith("cd_trigger"))
                        return;
                }

                if (enabled && this.gameObject.name == "battery(Clone)" && !batteryOnCharged.Value)
                {
                    Toggle = ToggleActive;
                }

                rb.detectCollisions = enabled;
                rb.isKinematic = !enabled;
                rb.useGravity = enabled;

                if (gameObject.name != "lottery ticket(xxxxx)")
                {
                    if (enabled)
                    {
                        transform.position = position;
                        rb.velocity = Vector3.zero;
                    }
                    else
                    {
                        position = transform.position;
                    }
                }

                // Disable object's renderer on distance
                if (renderer != null)
                {
                    renderer.enabled = enabled;
                }
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
            {
                return;
            }

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

        void FsmFixes()
        {
            PlayMakerFSM useFsm = gameObject.GetPlayMakerByName("Use");
            if (useFsm != null)
            {
                useFsm.Fsm.RestartOnEnable = false;

                if (gameObject.name.StartsWith("door ")) return;
                if (gameObject.name == "lottery ticket(xxxxx)") return;

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

                if (this.gameObject.name == "battery(Clone)")
                {
                    batteryOnCharged = useFsm.FsmVariables.GetFsmBool("OnCharged");
                }
            }

            PlayMakerFSM dataFsm = gameObject.GetPlayMakerByName("Data");
            if (dataFsm != null)
            {
                dataFsm.Fsm.RestartOnEnable = false;
            }

            // Fixes for particular items.
            switch (gameObject.name)
            {
                case "diesel(itemx)":
                    transform.Find("FluidTrigger").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    break;
                case "gasoline(itemx)":
                    transform.Find("FluidTrigger").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    break;
                case "motor oil(itemx)":
                    transform.Find("MotorOilTrigger").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    break;
                case "coolant(itemx)":
                    transform.Find("CoolantTrigger").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    break;
                case "brake fluid(itemx)":
                    transform.Find("BrakeFluidTrigger").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    break;
                case "wood carrier(itemx)":
                    transform.Find("WoodTrigger").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    break;
            }

            PlayMakerFSM paintFSM = gameObject.GetPlayMakerByName("Paint");
            if (paintFSM != null)
            {
                paintFSM.Fsm.RestartOnEnable = false;
            }
        }
    }
}
