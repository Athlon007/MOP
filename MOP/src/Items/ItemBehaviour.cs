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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MSCLoader;
using MSCLoader.Helper;
using MSCLoader.PartMagnet;

using MOP.Common;
using MOP.FSM.Actions;
using MOP.Managers;
using MOP.Vehicles.Cases;
using MOP.Items.Cases;
using MOP.Items.Helpers;
using MOP.Rules;
using MOP.Rules.Types;

namespace MOP.Items
{
    class ItemBehaviour : MonoBehaviour
    {
        // This class is an absolute disaster and must be rewritten.

        bool firstLoad;

        bool dontDisable;
        internal bool DontDisable
        {
            get => dontDisable;
            set
            {
                if (!value && dontDisable)
                {
                    TogglePhysicsOnly(true);
                }

                dontDisable = value;
            }
        }

        bool ignoreRenderer;
        public bool IgnoreRenderer
        {
            get => ignoreRenderer;
            set
            {
                if (value && !ignoreRenderer && renderer)
                {
                    renderer.enabled = true;
                }
                ignoreRenderer = value;
            }
        }

        internal readonly Rigidbody rb;
        Renderer renderer;

        Vector3 position;

        FsmBool batteryOnCharged;
        readonly FsmFloat floorJackTriggerY;

        bool kiljuInitialReset;

        PartMagnet partMagnet;
        BoltMagnet boltMagnet;

        public ItemBehaviour()
        {
            if (gameObject.GetComponents<ItemBehaviour>().Length > 1)
            {
                Destroy(this);
            }

            SetInitialTogglingMethod();

            // Get object's components
            rb = GetComponent<Rigidbody>();
            PlayMakerFSM fsm = GetComponent<PlayMakerFSM>();
            renderer = GetComponent<Renderer>();
            partMagnet = GetComponent<PartMagnet>();
            boltMagnet = GetComponent<BoltMagnet>();

            // From PlayMakerFSM, find states that contain one of the names that relate to destroying object,
            // and inject RemoveSelf void.
            if (fsm != null)
            {
                foreach (var st in fsm.FsmStates)
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

                // Destroys empty shopping bags appearing in the back of the yard.
                PlayMakerArrayListProxy list = gameObject.GetComponent<PlayMakerArrayListProxy>();
                if (list.arrayList.Count == 0)
                {
                    ItemsManager.Instance.Remove(this);
                    Destroy(this.gameObject);
                }
            }

            position = transform.position;

            // Fixes a bug which would prevent player from putting on the helmet, after taking it off.
            if (this.gameObject.name == "helmet(itemx)")
            {
                return;
            }

            // Floor jack and car jack.
            // Find the Trigger object and get its Y FsmFloat.
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

            if (!gameObject.name.EqualsAny("empty bottle(Clone)", "empty pack(Clone)"))
            {
                if (IsPartMagnetAttached()) return;

                if (rb?.velocity.magnitude > 0.1f) return;
                rb?.Sleep();
            }
        }

        void Awake()
        {
            // Add self to the MinorObjects.objectHooks list
            ItemsManager.Instance.Add(this);

            // Fixes a bug which would cause Toggle delegate being null.
            if (Toggle == null)
            {
                SetInitialTogglingMethod();
            }
        }

        void OnEnable()
        {
            // If is an empty plastic can (aka, empty kilju/orange juice bottle), we check if the distance to Jokke's can trigger is low.
            // If that's the case, we teleport the object to lost item spawner (junkyard).
            if (!kiljuInitialReset)
            {
                kiljuInitialReset = true;
                ResetKiljuContainer();
            }
        }

        // Triggered before the object is destroyed.
        // Removes self from MinorObjects.instance.objectHooks.
        public void RemoveSelf()
        {
            ItemsManager.Instance.Remove(this);
        }

        void OnDestroy() => RemoveSelf();

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
                        transform.position = ItemsManager.Instance.LostSpawner.position;

                    firstLoad = true;
                }

                if (!Hypervisor.Instance.IsItemInitializationDone())
                {
                    if (transform.root != Satsuma.Instance.transform || IsPartMagnetAttached())
                        transform.position = position;
                }

                if (DontDisable)
                {
                    TogglePhysicsOnly(enabled);
                    return;
                }

                // Disable empty items function.
                // Items that are marked as empty are disabled by the game.
                if (MOP.DisableEmptyItems.Value && this.gameObject.name == "empty(itemx)" && this.gameObject.transform.parent == null)
                {
                    enabled = !MopSettings.IsModActive;
                }

                // Don't execute rest of the code, if the enabled is the same as activeSelf.
                if (gameObject.activeSelf == enabled)
                {
                    return;
                }

                // Don't toggle, if the item is attached to Satsuma.
                if (transform.root.gameObject.name == "SATSUMA(557kg, 248)" || IsPartMagnetAttached())
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
                        if (Vector3.Distance(gameObject.transform.position, Hypervisor.Instance.GetPlayer().position) < 5)
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
                    // Fixes infinitely burnign garbage barrel fire.
                    case "garbage barrel(itemx)":
                        if (!enabled)
                            transform.Find("Fire").gameObject.SetActive(false);
                        break;
                    // Fixes unpickable empty plastic cans
                    case "emptyca":
                        if (Vector3.Distance(transform.position, ItemsManager.Instance.LandfillSpawn.position) < 5)
                        {
                            gameObject.name = "empty plastic can(itemx)";
                            gameObject.MakePickable(true);
                        }
                        break;
                }

                // CDs resetting fix.
                if (this.gameObject.name.StartsWith("cd(item") && this.transform.parent != null && this.transform.parent.name == "cd_sled_pivot")
                {
                    return;
                }

                // Check if item is in CarryMore inventory.
                // If so, ignore that item.
                if ((CompatibilityManager.CarryMore || CompatibilityManager.CarryEvenMore) && transform.position.y < -900)
                {
                    return;
                }

                gameObject.SetActive(enabled);
            }
            catch { }
        }

        void TogglePhysicsOnly(bool enabled)
        {
            try
            {
                // If the item has fallen under the detection range of the game's built in garbage collector,
                // teleport that item manually to the landfill.
                if (!firstLoad)
                {
                    if (transform.position.y < -100 && transform.position.x != 0 && transform.position.z != 0)
                        transform.position = ItemsManager.Instance.LostSpawner.position;

                    firstLoad = true;
                }

                if (!Hypervisor.Instance.IsItemInitializationDone())
                {
                    if (transform.root != Satsuma.Instance.transform)
                        transform.position = position;
                }

                if (MOP.DisableEmptyItems.Value && this.gameObject.name == "empty(itemx)" && this.gameObject.transform.parent.gameObject.name != "ItemPivot")
                {
                    enabled = !MopSettings.IsModActive;
                }

                if (rb == null || rb.useGravity == enabled)
                {
                    return;
                }

                if (IsPartMagnetAttached()) return;

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
                    if (this.gameObject.name.StartsWith("cd(item") && this.transform.parent.name == "PivotCD")
                        return;

                    // Prevent CDs from clipping through the Radio
                    if (this.gameObject.name.StartsWith("cd(item") && this.transform.parent.name == "cd_sled_pivot")
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
                if (renderer != null && !IgnoreRenderer)
                {
                    renderer.enabled = enabled;
                }

            }
            catch { }
        }

        void FsmFixes()
        {
            try
            {
                PlayMakerFSM useFsm = gameObject.GetPlayMakerFSM("Use");
                if (useFsm != null)
                {
                    useFsm.Fsm.RestartOnEnable = false;
                    if (gameObject.name.StartsWith("door ")) return;
                    if (gameObject.name == "lottery ticket(xxxxx)") return;

                    FsmState state1 = useFsm.GetState("State 1");
                    if (state1 != null)
                    {
                        List<FsmStateAction> emptyState1 = state1.Actions.ToList();
                        emptyState1.Insert(0, new CustomStop());
                        state1.Actions = emptyState1.ToArray();
                        state1.SaveActions();
                    }

                    FsmState loadState = useFsm.GetState("Load");
                    if (loadState != null)
                    {
                        List<FsmStateAction> emptyActions = loadState.Actions.ToList();
                        emptyActions.Insert(0, new CustomStop());
                        loadState.Actions = emptyActions.ToArray();
                        loadState.SaveActions();
                    }

                    if (this.gameObject.name == "battery(Clone)")
                    {
                        batteryOnCharged = useFsm.FsmVariables.GetFsmBool("OnCharged");
                    }
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
                    case "suitcase(itemx)":
                        transform.Find("Money").gameObject.AddComponent<SuitcaseMoneyBehaviour>();
                        break;
                    case "radio(itemx)":
                        transform.Find("Channel").gameObject.AddComponent<RadioDisable>();
                        break;
                    case "fuel tank(Clone)":
                        transform.Find("Bolts").GetChild(7).GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                        break;
                    case "spark plug(Clone)":
                        gameObject.GetPlayMakerFSM("Screw").Fsm.RestartOnEnable = false;
                        break;
                    case "spark plug box(Clone)":
                        break;
                }
                
                PlayMakerFSM dataFsm = gameObject.GetPlayMakerFSM("Data");
                if (dataFsm != null)
                {
                    dataFsm.Fsm.RestartOnEnable = false;
                }

                PlayMakerFSM paintFSM = gameObject.GetPlayMakerFSM("Paint");
                if (paintFSM != null)
                {
                    paintFSM.Fsm.RestartOnEnable = false;
                }
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, $"FSM_FIXES | {gameObject.Path()}");
            }
        }

        /// <summary>
        /// Freezes the item by adding the ItemFreezer item, and setting rigidbody to kinematic and freezing constraints.
        /// </summary>
        public void Freeze()
        {
            // If the item is an Kilju or Empty Plastic Can, and is close to the CanTrigger object,
            // teleport the object to LostSpawner (junk yard).
            ResetKiljuContainer();

            gameObject.AddComponent<ItemFreezer>();
        }

        /// <summary>
        /// Checks what disabling method object uses and then returns the correct value for that object.
        /// </summary>
        public bool ActiveSelf => (Toggle == TogglePhysicsOnly && rb != null) ? rb.detectCollisions : gameObject.activeSelf;

        /// <summary>
        /// Sets up the toggling method.
        /// </summary>
        private void SetInitialTogglingMethod()
        {
            Toggle = ToggleActive;

            IgnoreRule rule = RulesManager.Instance.IgnoreRules.Find(f => f.ObjectName == this.gameObject.name);
            if (rule != null)
            {
                Toggle = TogglePhysicsOnly;

                if (rule.TotalIgnore)
                {
                    Destroy(this);
                    return;
                }
            }

            // This items cannot be fully disabled, for one reason or another.
            if (this.gameObject.name.EqualsAny("fish trap(itemx)", "bucket(itemx)", "pike(itemx)", "envelope(xxxxx)", "lottery ticket(xxxxx)"))
            {
                Toggle = TogglePhysicsOnly;
            }

            // If ignore, don't disable renderer.
            if (rule != null)
            {
                renderer = null;
            }
        }

        /// <summary>
        /// Fixes renderers and physics being disabled, if item had Toggle method set to TogglePhysicsOnly and has switched to ToggleActive.
        /// </summary>
        internal void ToggleChangeFix()
        {
            if (Toggle != ToggleActive) return;

            // Fixing issue with objects not getting re-enabled (I hope).
            if (renderer != null && renderer.enabled == false && !IgnoreRenderer)
            {
                renderer.enabled = true;
            }

            if (IsPartMagnetAttached()) return;

            // Fixing disabled physics.
            if (rb != null && (rb.isKinematic || !rb.useGravity || !rb.detectCollisions))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.detectCollisions = true;
            }
        }

        internal void ResetKiljuContainer()
        {
            if (!gameObject.name.ContainsAny("empty plastic can", "kilju", "emptyca")) return;

            if (ItemsManager.Instance.GetCanTrigger())
            {
                if (Vector3.Distance(transform.position, ItemsManager.Instance.GetCanTrigger().transform.position) < 2)
                {
                    transform.position = ItemsManager.Instance.LostSpawner.position;

                    PlayMakerFSM fsm = gameObject.GetPlayMakerFSM("Use");
                    if (fsm)
                    {
                        fsm.FsmVariables.GetFsmBool("ContainsKilju").Value = false;
                    }

                    gameObject.name = "empty plastic can(itemx)";

                    return;
                }
            }

            if (ItemsManager.Instance.LandfillSpawn)
            {
                if (Vector3.Distance(transform.position, ItemsManager.Instance.LandfillSpawn.position) < 5)
                {
                    transform.position = ItemsManager.Instance.LandfillSpawn.position;

                    PlayMakerFSM fsm = gameObject.GetPlayMakerFSM("Use");
                    if (fsm)
                    {
                        fsm.FsmVariables.GetFsmBool("ContainsKilju").Value = false;
                    }

                    gameObject.name = "empty plastic can(itemx)";
                }
            }
        }

        internal bool IsPartMagnetAttached()
        {
            return (partMagnet != null && partMagnet.attached) || (boltMagnet != null && boltMagnet.attached);
        }
    }
}
