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

using HutongGames.PlayMaker;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOP.FSM;
using MOP.Common;
using MOP.FSM.Actions;
using MOP.Managers;
using MOP.Vehicles.Cases;
using MOP.Items.Cases;
using MOP.Items.Helpers;
using MOP.Rules;
using MOP.Rules.Types;
using MOP.Helpers;
using MOP.Places;

namespace MOP.Items
{
    class ItemBehaviour : MonoBehaviour
    {
        // This class is an absolute disaster and must be rewritten.

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
        
        // Grill
        GameObject grillFlame, grillTrigger;
        bool grillKeepActive;

        bool fsmFixesOnActive;

        // Item spoilage
        FsmFloat spoilRate;
        FsmFloat spoilRateFridge;
        FsmFloat condition;
        float timeDisabled;

        bool isObjectOnGrill;

        PlayMakerFSM removalFSM;

        public ItemBehaviour()
        {
            if (gameObject.GetComponents<ItemBehaviour>().Length > 1)
            {
                Destroy(this);
            }

            SetInitialTogglingMethod();

            // Get object's components
            rb = GetComponent<Rigidbody>();
            renderer = GetComponent<Renderer>();

            position = transform.position;

            removalFSM = gameObject.GetPlayMaker("Removal");

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
            if (transform.root.gameObject.name == "amis-auto ky package(xxxxx)")
            {
                fsmFixesOnActive = true;
            }
            else
            {
                FsmFixes();
            }

            // HACK: For some reason the trigger that's supposed to fix tire job not working doesn't really work on game load,
            // toggle DontDisable to true, if tire is close to repair shop cash register.
            if (this.gameObject.name.StartsWith("wheel_"))
            {
                RepairShop repairShop = PlaceManager.Instance[2] as RepairShop;
                if (Vector3.Distance(transform.position, repairShop.GetCashRegister().position) < 7)
                {
                    DontDisable = true;
                }
            }
            
            timeDisabled = Time.timeSinceLevelLoad;

            if (!gameObject.name.EqualsAny("empty bottle(Clone)", "empty pack(Clone)", "empty cup(Clone)", "coffee cup(Clone)", "empty glass(Clone)"))
            {
                if (rb?.velocity.magnitude > 0.1f) return;
                rb?.Sleep();
            }

            if (gameObject.name.Equals("spoiled pike(itemx)") && !gameObject.tag.Equals("PART"))
            {
                gameObject.MakePickable();
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

            if (transform.position.y < -100 && transform.position.x != 0 && transform.position.z != 0)
            {
                transform.position = ItemsManager.Instance.LostSpawner.position;
            }
        }

        void OnEnable()
        {
            // If is an empty plastic can (aka, empty kilju/orange juice bottle), we check if the distance to Jokke's can trigger is low.
            // If that's the case, we teleport the object to lost item spawner (junkyard).
            if (!kiljuInitialReset || gameObject.name == "emptyca")
            {
                kiljuInitialReset = true;
                ResetKiljuContainer();
            }

            if (fsmFixesOnActive)
            {
                fsmFixesOnActive = false;
                FsmFixes();
            }

            if (spoilRate != null)
            {
                float currentSpoil = condition.Value;
                float currentSpoilRate = Yard.Instance.IsItemInFridge(this.gameObject) ? spoilRateFridge.Value : spoilRate.Value;
                // Apparently MSC uses some weird way of calculating time that has barely anything to do with Time.deltaTime...
                // Making the value 1/3 of the currentSpoil somehow makes it closer to what it should've been
                currentSpoil -= (Time.timeSinceLevelLoad - timeDisabled) * currentSpoilRate * 0.33f; 
                condition.Value = currentSpoil;
            }
        }

        void OnDisable()
        {
            if (gameObject.name == "emptyca")
            {
                ResetKiljuContainer();
            }

            if (spoilRate != null)
            {
                timeDisabled = Time.timeSinceLevelLoad;
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
                if (!Hypervisor.Instance.IsItemInitializationDone() && (transform.root != Satsuma.Instance.transform))
                {
                    transform.position = position;
                }

                if (DontDisable || GrillIsOnFire() || isObjectOnGrill)
                {
                    TogglePhysicsOnly(enabled);
                    return;
                }

                // Disable empty items function.
                // Items that are marked as empty are disabled by the game.
                if (MOP.DisableEmptyItems.GetValue() && this.gameObject.name == "empty(itemx)" && this.gameObject.transform.parent == null)
                {
                    enabled = !MopSettings.IsModActive;
                }

                // Don't execute rest of the code, if the enabled is the same as activeSelf.
                // Don't toggle, if the item is attached to Satsuma.
                if (gameObject.activeSelf == enabled || transform.root.gameObject.name == "SATSUMA(557kg, 248)")
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
                        // Don't toggle if battery is left on charger.
                        if (gameObject.transform.parent.gameObject.name == "pivot_battery" || (!enabled && batteryOnCharged.Value))
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
                            gameObject.MakePickable();
                        }
                        break;
                    case "bucket lid(itemx)":
                        if (!enabled)
                        {
                            removalFSM.enabled = false;
                        }

                        if (transform.parent != null && transform.parent.gameObject.name == "PivotLid") return;
                        break;
                }

                // CDs resetting fix.
                if (gameObject.name.StartsWith("cd(item") && transform.parent != null && transform.parent.name == "cd_sled_pivot")
                {
                    return;
                }

                // Check if item is in CarryMore inventory.
                // If so, ignore that item.
                if (CompatibilityManager.IsInBackpack(this))
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
                if (!Hypervisor.Instance.IsItemInitializationDone())
                {
                    if (transform.root != Satsuma.Instance.transform)
                        transform.position = position;
                }

                if (MOP.DisableEmptyItems.GetValue() && this.gameObject.name == "empty(itemx)" && this.gameObject.transform.parent.gameObject.name != "ItemPivot")
                {
                    enabled = !MopSettings.IsModActive;
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
                if (CompatibilityManager.IsInBackpack(this))
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

                if (GrillIsOnFire()) return;

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
            // From PlayMakerFSM, find states that contain one of the names that relate to destroying object,
            // and inject RemoveSelf void.
            PlayMakerFSM fsm = GetComponent<PlayMakerFSM>();
            if (fsm != null)
            {
                foreach (var st in fsm.FsmStates)
                {
                    switch (st.Name)
                    {
                        case "Destroy self":
                            MSCLoader.FsmHook.FsmInject(this.gameObject, "Destroy self", RemoveSelf);
                            break;
                        case "Destroy":
                            MSCLoader.FsmHook.FsmInject(this.gameObject, "Destroy", RemoveSelf);
                            break;
                        case "Destroy 2":
                            MSCLoader.FsmHook.FsmInject(this.gameObject, "Destroy 2", RemoveSelf);
                            break;
                    }
                }
            }

            // If the item is a shopping bag, hook the RemoveSelf to "Is garbage" FsmState
            if (gameObject.name.Contains("shopping bag"))
            {
                MSCLoader.FsmHook.FsmInject(gameObject, "Is garbage", RemoveSelf);

                // Destroys empty shopping bags appearing in the back of the yard.
                PlayMakerArrayListProxy list = gameObject.GetComponent<PlayMakerArrayListProxy>();
                if (list.arrayList.Count == 0)
                {
                    ItemsManager.Instance.Remove(this);
                    Destroy(this.gameObject);
                }
            }

            try
            {
                PlayMakerFSM useFsm = gameObject.GetPlayMaker("Use");
                if (useFsm != null)
                {
                    useFsm.Fsm.RestartOnEnable = false;
                    if (gameObject.name.StartsWith("door ") || gameObject.name.EqualsAny("amis-auto ky package(xxxxx)", "lottery ticket(xxxxx)")) return;

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

                    if (useFsm.FsmVariables.GetFsmFloat("SpoilingRate") != null)
                    {
                        spoilRate = useFsm.FsmVariables.GetFsmFloat("SpoilingRate");
                        condition = useFsm.FsmVariables.GetFsmFloat("Condition");
                    }
                    if (useFsm.FsmVariables.GetFsmFloat("SpoilingRateFridge") != null)
                    {
                        spoilRateFridge = useFsm.FsmVariables.GetFsmFloat("SpoilingRateFridge");
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
                    case "radio(Clone)":
                        if (transform.Find("Channel") != null)
                            transform.Find("Channel").gameObject.AddComponent<RadioDisable>();
                        break;
                    case "fuel tank(Clone)":
                        transform.Find("Bolts").GetChild(7).GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                        break;
                    case "spark plug(Clone)":
                        gameObject.GetPlayMaker("Screw").Fsm.RestartOnEnable = false;
                        break;
                    case "spark plug box(Clone)":
                        break;
                    case "grill(itemx)":
                        grillFlame = transform.Find("Fireplace/Flame/FireEffects").gameObject;
                        grillTrigger = transform.Find("Fireplace/GrillTrigger").gameObject;
                        transform.Find("Fireplace/SausageTrigger").gameObject.AddComponent<GrillTriggerBehaviour>();
                        break;
                }
                
                PlayMakerFSM dataFsm = gameObject.GetPlayMaker("Data");
                if (dataFsm != null)
                {
                    dataFsm.Fsm.RestartOnEnable = false;
                }

                PlayMakerFSM paintFSM = gameObject.GetPlayMaker("Paint");
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
            // Fixes bucket lid detaching itself while player's away
            if (gameObject.name == "bucket lid(itemx)")
            {
                if (transform.parent != null && transform.parent.gameObject.name == "PivotLid")
                {
                    removalFSM.enabled = true;
                    transform.localEulerAngles = Vector3.zero;
                }
            }

            if (Toggle != ToggleActive) return;

            // Fixing issue with objects not getting re-enabled (I hope).
            if (renderer != null && renderer.enabled == false && !IgnoreRenderer)
            {
                renderer.enabled = true;
            }

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
           
            gameObject.MakePickable();
            gameObject.tag = "ITEM";

            if (ItemsManager.Instance.GetCanTrigger())
            {
                if (Vector3.Distance(transform.position, ItemsManager.Instance.GetCanTrigger().transform.position) < 10)
                {
                    transform.position = ItemsManager.Instance.LostSpawner.position;
                    kiljuInitialReset = false;

                    PlayMakerFSM fsm = gameObject.GetPlayMaker("Use");
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

                    PlayMakerFSM fsm = gameObject.GetPlayMaker("Use");
                    if (fsm)
                    {
                        fsm.FsmVariables.GetFsmBool("ContainsKilju").Value = false;
                    }
                    gameObject.name = "empty plastic can(itemx)";
                }
            }
        }

        internal void SaveGame()
        {
            if (gameObject.GetPlayMaker("Use"))
            {
                PlayMakerFSM useFSM = gameObject.GetPlayMaker("Use");
                string id = useFSM.FsmVariables.GetFsmString("ID").Value;

                if (gameObject.name.StartsWith("wheel"))
                {
                    SaveManager.SaveToDefault(id + "Transform", gameObject.transform);
                }
                else
                {
                    SaveManager.SaveToItem(id + "Transform", gameObject.transform);
                    SaveManager.SaveToItem(id + "Consumed", useFSM.FsmVariables.GetFsmBool("Consumed").Value);

                    if (id.Contains("juiceconcentrate"))
                    {
                        if (gameObject.name.ContainsAny("emptyca", "empty plastic can"))
                        {
                            useFSM.FsmVariables.GetFsmBool("ContainsKilju").Value = false;
                        }

                        if (gameObject.name.ContainsAny("kilju"))
                        {
                            useFSM.FsmVariables.GetFsmBool("ContainsKilju").Value = true;
                        }

                        SaveManager.SaveToItem(id + "ContainsJuice", useFSM.FsmVariables.GetFsmBool("ContainsJuice").Value);
                        SaveManager.SaveToItem(id + "ContainsKilju", useFSM.FsmVariables.GetFsmBool("ContainsKilju").Value);
                        SaveManager.SaveToItem(id + "KiljuAlc", useFSM.FsmVariables.GetFsmFloat("KiljuAlc").Value);
                        SaveManager.SaveToItem(id + "KiljuSweetness", useFSM.FsmVariables.GetFsmFloat("KiljuSweetness").Value);
                        SaveManager.SaveToItem(id + "KiljuVinegar", useFSM.FsmVariables.GetFsmFloat("KiljuVinegar").Value);
                        SaveManager.SaveToItem(id + "KiljuYeast", useFSM.FsmVariables.GetFsmFloat("KiljuYeast").Value);
                        useFSM.enabled = false;
                    }
                }
            }
        }

        bool GrillIsOnFire()
        {
            bool active = grillFlame && grillFlame.activeSelf || grillTrigger && grillTrigger.activeSelf || grillKeepActive;
            if (active)
            {
                grillKeepActive = true;
            }

            if (grillTrigger && grillTrigger.activeSelf)
            {
                grillKeepActive = false;
            }

            return active;
        }

        public void IsObjectOnGrill(bool isObjectOnGrill)
        {
            this.isObjectOnGrill = isObjectOnGrill;
        }

        /// <summary>
        /// Loads transform from save file.
        /// </summary>
        public void LoadTransform()
        {
            // Prevent loading that function twice.
            WasTransformLoaded = true;
            // For now, we only support r20 battery box.
            if (!gameObject.name.Contains("r20 battery box"))
            {
                return;
            }

            // Read object's save transform ID from Use PlayMaker.
            string transformID = gameObject.GetPlayMaker("Use")?.FsmVariables.GetFsmString("UniqueTagTransform").Value;

            Transform loadedTransform;

            // Check if transformID is exists in the save or items file.
            // Load the item's transform from the appropriate save file.
            if (SaveManager.IsItemTagPresent(transformID))
            {
                loadedTransform = SaveManager.ReadItemTranform(transformID);
            }
            else if (SaveManager.IsSaveTagPresent(transformID))
            {
                loadedTransform = SaveManager.ReadTransform(transformID);
            }
            else
            {
                return;
            }

            // Do not load transform, which position is point zero.
            if (loadedTransform.position == Vector3.zero)
            {
                return;
            }

            // Finally load the position.
            transform.position = loadedTransform.position;
            transform.rotation = loadedTransform.rotation;
        }

        public bool WasTransformLoaded { get; private set; }
    }
}
