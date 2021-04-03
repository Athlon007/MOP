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

using MOP.FSM;
using MOP.FSM.Actions;
using MOP.Items;
using MOP.Common;
using MOP.Common.Enumerations;
using MOP.Vehicles.Cases;
using MOP.Vehicles.Managers;
using MOP.Rules;
using MOP.Rules.Types;
using MOP.Managers;

namespace MOP.Vehicles
{
    class Vehicle
    {
        public bool IsActive = true;

        public GameObject gameObject { get; private set; }

        // Values that are being saved or loaded
        protected Vector3 Position { get; set; }
        protected Quaternion Rotation { get; set; }
        VehiclesTypes vehicleType;
        public VehiclesTypes VehicleType { get => vehicleType; }

        // All objects that cannot be unloaded (because it causes problems) land under that object
        protected Transform temporaryParent;
        protected List<PreventToggleOnObject> preventToggleOnObjects;

        // Unity car systems and rigidbody
        public Transform transform => gameObject.transform;
        internal CarDynamics carDynamics;
        internal Axles axles;
        internal Rigidbody rb;
        readonly Drivetrain drivetrain;

        // Prevents MOP from disabling car's physics when the car has rope hooked
        readonly PlayMakerFSM fsmHookFront;
        readonly PlayMakerFSM fsmHookRear;

        // Reference to one of the wheels that checks if the vehicle is on ground
        readonly Wheel wheel;

        // Currently used only by Shitsuma.
        internal Quaternion lastGoodRotation;
        internal Vector3 lastGoodPosition;
        bool lastGoodRotationSaved;
        readonly EventSounds eventSounds;

        /// <summary>
        /// Initialize class
        /// </summary>
        /// <param name="gameObjectName"></param>
        public Vehicle(string gameObjectName)
        {
            // gameObject the object by name
            gameObject = GameObject.Find(gameObjectName);

            // Use Resources.FindObjectsOfTypeAll method, if the vehicle was not found.
            if (gameObject == null)
                gameObject = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == gameObjectName);

            if (gameObject == null)
            {
                ModConsole.Error($"[MOP] Could not find {gameObjectName} vehicle.");
                return;
            }

            // Get the object position and rotation
            Position = gameObject.transform.localPosition;
            Rotation = gameObject.transform.localRotation;

            switch (gameObject.name)
            {
                default:
                    vehicleType = VehiclesTypes.Generic;
                    break;
                case "SATSUMA(557kg, 248)":
                    vehicleType = VehiclesTypes.Satsuma;
                    break;
                case "HAYOSIKO(1500kg, 250)":
                    vehicleType = VehiclesTypes.Hayosiko;
                    break;
                case "JONNEZ ES(Clone)":
                    vehicleType = VehiclesTypes.Jonnez;
                    break;
                case "KEKMET(350-400psi)":
                    vehicleType = VehiclesTypes.Kekmet;
                    break;
                case "RCO_RUSCKO12(270)":
                    vehicleType = VehiclesTypes.Ruscko;
                    break;
                case "FERNDALE(1630kg)":
                    vehicleType = VehiclesTypes.Ferndale;
                    break;
                case "FLATBED":
                    vehicleType = VehiclesTypes.Flatbed;
                    break;
                case "GIFU(750/450psi)":
                    vehicleType = VehiclesTypes.Gifu;
                    break;
                case "BOAT":
                    vehicleType = VehiclesTypes.Boat;
                    break;
                case "COMBINE(350-400psi)":
                    vehicleType = VehiclesTypes.Combine;
                    break;
            }

            // Creates a new gameobject that is names after the original file + '_TEMP' (ex. "SATSUMA(557kg, 248)_TEMP")
            temporaryParent = new GameObject($"{gameObject.name}_TEMP").transform;

            preventToggleOnObjects = new List<PreventToggleOnObject>();

            // This should fix bug that leads to items inside of vehicles to fall through it.
            PlayMakerFSM lodFSM = gameObject.GetPlayMakerByName("LOD");
            if (lodFSM != null)
            {
                lodFSM.Fsm.RestartOnEnable = false;
                FsmState resetState = lodFSM.FindFsmState("Fix Collider");
                if (resetState != null)
                {
                    resetState.Actions = new FsmStateAction[] { new CustomStop() };
                    resetState.SaveActions();
                }
            }

            if (vehicleType == VehiclesTypes.Boat)
                return;

            // Get the object's child which are responsible for audio
            foreach (Transform audioObject in FindAudioObjects())
            {
                preventToggleOnObjects.Add(new PreventToggleOnObject(audioObject));
            }

            // Fix for fuel level resetting after respawn
            Transform fuelTank = gameObject.transform.Find("FuelTank");
            if (fuelTank != null)
            {
                PlayMakerFSM fuelTankFSM = fuelTank.GetComponent<PlayMakerFSM>();
                if (fuelTankFSM)
                    fuelTankFSM.Fsm.RestartOnEnable = false;
            }

            // If the vehicle is Gifu, find knobs and add them to list of unloadable objects
            switch (vehicleType)
            {
                case VehiclesTypes.Gifu:
                    Transform knobs = gameObject.transform.Find("Dashboard/Knobs");
                    foreach (PlayMakerFSM knobsFSMs in knobs.GetComponentsInChildren<PlayMakerFSM>())
                        knobsFSMs.Fsm.RestartOnEnable = false;

                    // Fix resetting of shit tank.
                    gameObject.transform.Find("ShitTank").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

                    // Odometer fix.
                    transform.Find("Dashboard/Odometer").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

                    // Air pressure fix.
                    transform.Find("Simulation/Airbrakes").GetPlayMakerByName("Air Pressure").Fsm.RestartOnEnable = false;

                    // Hand throttle.
                    gameObject.AddComponent<GifuHandThrottle>();
                    break;
                case VehiclesTypes.Jonnez:
                    gameObject.transform.Find("Kickstand").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

                    // Disable on restart for wheels script.
                    Transform wheelsParent = transform.Find("Wheels");
                    foreach (Transform wheel in wheelsParent.GetComponentsInChildren<Transform>())
                    {
                        if (!wheel.gameObject.name.StartsWith("Moped_wheel")) continue;
                        wheel.gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    }
                    
                    // Tries to fix shaking of the Jonnez.
                    gameObject.transform.Find("LOD/PlayerTrigger").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    break;
                case VehiclesTypes.Kekmet:
                    transform.Find("Dashboard/HourMeter").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

                    // Hand Throttle.
                    gameObject.AddComponent<KekmetHandThrottle>();

                    // Trailer connection.
                    transform.Find("Trailer/Hook").GetPlayMakerByName("Distance").Fsm.RestartOnEnable = false;
                    //transform.Find("Trailer/Remove").GetPlayMakerByName("Use").Fsm.RestartOnEnable = false;
                    break;
                case VehiclesTypes.Flatbed:
                    transform.Find("Bed/LogTrigger").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

                    GameObject trailerLogUnderFloorCheck = new GameObject("MOP_TrailerLogUnderFloorFix");
                    trailerLogUnderFloorCheck.transform.parent = gameObject.transform;
                    trailerLogUnderFloorCheck.AddComponent<TrailerLogUnderFloor>();

                    // Tractor connection.
                    gameObject.GetPlayMakerByName("Detach").Fsm.RestartOnEnable = false;
                    break;

            }

            carDynamics = gameObject.GetComponent<CarDynamics>();
            axles = gameObject.GetComponent<Axles>();
            rb = gameObject.GetComponent<Rigidbody>();

            // Hook HookFront and HookRear
            // Get hooks first
            Transform hookFront = transform.Find(vehicleType == VehiclesTypes.Kekmet ? "Frontloader/ArmPivot/Arm/LoaderPivot/Loader/RopePoint/HookFront" : "HookFront");
            Transform hookRear = transform.Find("HookRear");

            // If hooks exists, attach the RopeHookUp and RopeUnhook to appropriate states
            if (hookFront != null)
            {
                fsmHookFront = hookFront.GetComponent<PlayMakerFSM>();
                fsmHookFront.Fsm.RestartOnEnable = false;
            }

            if (hookRear != null)
            {
                fsmHookRear = hookRear.GetComponent<PlayMakerFSM>();
                fsmHookRear.Fsm.RestartOnEnable = false;
            }

            // Set default toggling method - that is entire vehicle
            Toggle = ToggleActive;
            // If the user selected to toggle vehicle's physics only, it overrided any previous set for Toggle method
            if (RulesManager.Instance.SpecialRules.ToggleAllVehiclesPhysicsOnly)
            {
                Toggle = IgnoreToggle;
            }

            // Get all HingeJoints and add HingeManager to them
            // Ignore for Satsuma or cars that use ToggleUnityCar method (and force for Hayosiko - no matter what)
            if (vehicleType != VehiclesTypes.Satsuma && Toggle != ToggleUnityCar || vehicleType == VehiclesTypes.Hayosiko)
            {
                HingeJoint[] joints = gameObject.transform.GetComponentsInChildren<HingeJoint>();
                foreach (HingeJoint joint in joints)
                    joint.gameObject.AddComponent<HingeManager>();
            }

            // Get one of the wheels.
            wheel = axles.allWheels[0];
            drivetrain = gameObject.GetComponent<Drivetrain>();

            // Ignore Rules.
            IgnoreRule vehicleRule = RulesManager.Instance.IgnoreRules.Find(v => v.ObjectName == this.gameObject.name);
            if (vehicleRule != null)
            {
                Toggle = IgnoreToggle;

                if (vehicleRule.TotalIgnore)
                    IsActive = false;
            }

            // Prevent Toggle On Object Rule.
            IgnoreRuleAtPlace[] preventToggleOnObjectRule = RulesManager.Instance.IgnoreRulesAtPlaces
                .Where(v => v.Place == this.gameObject.name).ToArray();
            if (preventToggleOnObjectRule.Length > 0)
            {
                foreach (var p in preventToggleOnObjectRule)
                {
                    Transform t = transform.FindRecursive(p.ObjectName);
                    if (t == null)
                    {
                        ModConsole.Error($"[MOP] Couldn't find {p.ObjectName} in {p.Place}.");
                        continue;
                    }

                    preventToggleOnObjects.Add(new PreventToggleOnObject(t));
                }
            }

            eventSounds = gameObject.GetComponent<EventSounds>();
        }

        public delegate void ToggleHandler(bool enabled);
        public ToggleHandler Toggle;

        /// <summary>
        /// Enable or disable car
        /// </summary>
        internal void ToggleActive(bool enabled)
        {
            if (gameObject == null || gameObject.activeSelf == enabled || !IsActive) return;

            // If we're disabling a car, set the audio child parent to TemporaryAudioParent, and save the position and rotation.
            // We're doing that BEFORE we disable the object.
            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);

                Position = gameObject.transform.localPosition;
                Rotation = gameObject.transform.localRotation;
            }

            gameObject.SetActive(enabled);

            // Uppon enabling the object, set the localPosition and localRotation to the object's transform, and change audio source parents to Object
            // We're doing that AFTER we enable the object.
            if (enabled)
            {
                MoveNonDisableableObjects(null);
            }
        }

        /// <summary>
        /// Toggle car physics only.
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleUnityCar(bool enabled)
        {
            if ((gameObject == null) || vehicleType == VehiclesTypes.Boat || !IsActive)
                return;

            if (rb.isKinematic == !enabled && carDynamics.enabled == enabled)
                return;

            // Don't toggle physics, unless car's on ground
            if ((IsMoving() || !IsOnGround()) && !enabled)
                return;

            // If player is sitting in this specific vehicle, **NEVER** disable it.
            if (IsPlayerInThisCar())
                enabled = true;

            // If the car is a Satsuma, and Satsuma is in inspection area, don't disable the car.
            if (!enabled && vehicleType == VehiclesTypes.Satsuma && Satsuma.Instance.IsSatsumaInInspectionArea)
                enabled = true;

            // Prevent disabling car physics if the rope is hooked
            if (!enabled && gameObject.activeSelf == true && IsRopeHooked())
                enabled = true;

            carDynamics.enabled = enabled;
            axles.enabled = enabled;
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;

            // We're completly freezing Satsuma, so it won't flip (hopefully...).
            if (vehicleType == VehiclesTypes.Satsuma)
            {
                if (!enabled && !lastGoodRotationSaved)
                {
                    lastGoodRotationSaved = true;
                    lastGoodRotation = transform.localRotation;
                    lastGoodPosition = transform.localPosition;
                }

                if (enabled)
                {
                    lastGoodRotationSaved = false;
                }

                rb.constraints = enabled ? RigidbodyConstraints.None : RigidbodyConstraints.FreezePosition;
            }
        }

        public void ForceToggleUnityCar(bool enabled)
        {
            if ((gameObject == null) || vehicleType == VehiclesTypes.Boat || (carDynamics.enabled == enabled) || !IsActive)
                return;

            // If player is sitting in this specific vehicle, **NEVER** disable it.
            if (IsPlayerInThisCar())
                enabled = true;

            carDynamics.enabled = enabled;
            axles.enabled = enabled;
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;

            // We're completly freezing Satsuma, so it won't flip (hopefully...).
            if (vehicleType == VehiclesTypes.Satsuma)
            {
                if (!enabled && !lastGoodRotationSaved)
                {
                    lastGoodRotationSaved = true;
                    lastGoodRotation = transform.localRotation;
                    lastGoodPosition = transform.localPosition;
                }

                if (enabled)
                {
                    lastGoodRotationSaved = false;
                }

                rb.constraints = enabled ? RigidbodyConstraints.None : RigidbodyConstraints.FreezePosition;
            }
        }

        /// <summary>
        /// This is an empty void for when the toggling is meant to be ignored.
        /// </summary>
        /// <param name="enabled"></param>
        internal void IgnoreToggle(bool enabled)
        {
            return;
        }

        /// <summary>
        /// Retrieves the child audio objects from parent object.
        /// Basically looks for files with "audio" and "SoundSrc" name in it
        /// </summary>
        /// <returns></returns>
        Transform[] FindAudioObjects()
        {
            Transform[] childs = gameObject.transform.GetComponentsInChildren<Transform>();
            return childs.Where(obj => obj.gameObject.name.Contains("audio") || obj.gameObject.name.Contains("SoundSrc")).ToArray();
        }

        /// <summary>
        /// Checks PlayMaker of front and rear hooks and returns "true", if in any of the hooks value "Attached" is true.
        /// </summary>
        /// <returns></returns>
        bool IsRopeHooked()
        {
            bool isFrontHookAttached = fsmHookFront ? fsmHookFront.FsmVariables.GetFsmBool("Attached").Value : false;
            bool isRearHookAttached = fsmHookRear ? fsmHookRear.FsmVariables.GetFsmBool("Attached").Value : false;
            return (isFrontHookAttached || isRearHookAttached);
        }

        /// <summary>
        /// Checks one of the wheels' onGroundDown value
        ///
        /// WORKAROUND FOR JONNEZ:
        /// Because onGroundDown for Jonnez doesn't work the same way as for others, it will check if the Jonnnez's engine torque.
        /// </summary>
        /// <returns></returns>
        internal bool IsOnGround()
        {
            switch (vehicleType)
            {
                case VehiclesTypes.Jonnez:
                    return drivetrain.torque == 0;
                case VehiclesTypes.Satsuma:
                    if (!wheel.enabled)
                        return drivetrain.torque == 0;
                    break;
            }

            return wheel.onGroundDown;
        }

        /// <summary>
        /// Returns true, if the vehicle is moving.
        /// </summary>
        /// <returns></returns>
        internal bool IsMoving()
        {
            return rb.velocity.magnitude > 0.1f;
        }

        /// <summary>
        /// Disable the EventSounds component.
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleEventSounds(bool enabled)
        {
            eventSounds.disableSounds = !enabled;
        }

        /// <summary>
        /// Freezes the car completely by adding ItemFreezer class.
        /// </summary>
        public void Freeze()
        {
            gameObject.AddComponent<ItemFreezer>();
        }

        /// <summary>
        /// Returns true, if the root of Player object is this object.
        /// </summary>
        /// <returns></returns>
        protected bool IsPlayerInThisCar()
        {
            return Hypervisor.Instance.GetPlayer().transform.root == gameObject.transform;
        }

        /// <summary>
        /// Creates a hit collider around the car and checks if object of which root is NPC_CARS or TRAFFIC is in this zone.
        /// </summary>
        /// <returns></returns>
        public bool IsTrafficCarInArea()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.transform.root.gameObject.name.EqualsAny("NPC_CARS", "TRAFFIC"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Moves objects that can never be disabled to the parent variable.
        /// If parent is set to null, the object will be moved to its original parent.
        /// </summary>
        /// <param name="parent"></param>
        protected void MoveNonDisableableObjects(Transform parent)
        {
            for (int i = 0; i < preventToggleOnObjects.Count; i++)
            {
                if (preventToggleOnObjects[i].ObjectTransform == null)
                    continue;

                preventToggleOnObjects[i].ObjectTransform.parent = parent == null ? preventToggleOnObjects[i].OriginalParent : parent;
            }
        }
    }
}
