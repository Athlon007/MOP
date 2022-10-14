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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOP.Common;
using MOP.Common.Enumerations;
using MOP.FSM;
using MOP.Items;
using MOP.Rules;
using MOP.Rules.Types;
using MOP.Vehicles.Managers;

namespace MOP.Vehicles
{
    class Vehicle
    {
        public bool IsActive = true;

        public GameObject gameObject { get; private set; }

        // Values that are being saved or loaded
        protected Vector3 Position { get; set; }
        protected Quaternion Rotation { get; set; }
        protected VehiclesTypes vehicleType;
        public VehiclesTypes VehicleType { get => vehicleType; }

        // All objects that cannot be unloaded (because it causes problems) land under that object
        protected Transform temporaryParent;
        protected List<PreventToggleOnObject> preventToggleOnObjects = new List<PreventToggleOnObject>();

        // Unity car systems and rigidbody
        public Transform transform => gameObject.transform;
        protected Rigidbody rb;
        protected CarDynamics carDynamics;
        protected Axles axles;
        protected Drivetrain drivetrain;
        protected Wheel wheel;

        // Prevents MOP from disabling car's physics when the car has rope hooked
        protected PlayMakerFSM fsmHookFront, fsmHookRear;

        // Currently used only by Shitsuma.
        private EventSounds eventSounds;

        // Colliders and its position
        protected Transform colliders;
        protected Vector3 colliderPosition;

        protected DummyCar dummyCar;

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
            {
                gameObject = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == gameObjectName);
            }

            if (gameObject == null)
            {
                ModConsole.LogError($"[MOP] Could not find {gameObjectName} vehicle.");
                return;
            }

            // Get the object position and rotation
            Position = gameObject.transform.localPosition;
            Rotation = gameObject.transform.localRotation;

            vehicleType = VehiclesTypes.Generic;

            // Creates a new gameobject that is names after the original file + '_TEMP' (ex. "SATSUMA(557kg, 248)_TEMP")
            temporaryParent = new GameObject($"MOP_{gameObject.name}").transform;

            // This should fix bug that leads to items inside of vehicles to fall through it.
            PlayMakerFSM lodFSM = gameObject.GetPlayMaker("LOD");
            if (lodFSM != null)
            {
                lodFSM.Fsm.RestartOnEnable = false;
                if (lodFSM.FsmStates.FirstOrDefault(s => s.Name == "Fix Collider") != null)
                {
                    lodFSM.GetState("Fix Collider").ClearActions();
                }
            }

            LoadColliders();
            LoadCarElements();
            LoadRules();

            eventSounds = gameObject.GetComponent<EventSounds>();
        }

        protected virtual void LoadCarElements()
        {
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

            carDynamics = gameObject.GetComponent<CarDynamics>();
            axles = gameObject.GetComponent<Axles>();
            rb = gameObject.GetComponent<Rigidbody>();

            // Hook HookFront and HookRear
            // Get hooks first
            DisableHooksResetting();

            // Set default toggling method - that is entire vehicle
            Toggle = ToggleActive;
            // If the user selected to toggle vehicle's physics only, it overrided any previous set for Toggle method
            if (RulesManager.Instance.SpecialRules.ToggleAllVehiclesPhysicsOnly)
            {
                Toggle = IgnoreToggle;
            }

            // Get all HingeJoints and add HingeManager to them
            ApplyHingeManager();

            // Get one of the wheels.
            wheel = axles.allWheels[0];
            drivetrain = gameObject.GetComponent<Drivetrain>();

            dummyCar = new DummyCar(this.gameObject);
        }

        /// <summary>
        /// Loads rule files.
        /// </summary>
        private void LoadRules()
        {
            // Ignore Rules.
            IgnoreRule vehicleRule = RulesManager.Instance.GetList<IgnoreRule>().Find(v => v.ObjectName == this.gameObject.name);
            if (vehicleRule != null)
            {
                Toggle = IgnoreToggle;

                if (vehicleRule.TotalIgnore)
                    IsActive = false;
            }

            // Prevent Toggle On Object Rule.
            IgnoreRuleAtPlace[] preventToggleOnObjectRule = RulesManager.Instance.GetList<IgnoreRuleAtPlace>()
                .Where(v => v.Place == this.gameObject.name).ToArray();
            if (preventToggleOnObjectRule.Length > 0)
            {
                foreach (var p in preventToggleOnObjectRule)
                {
                    Transform t = transform.FindRecursive(p.ObjectName);
                    if (t == null)
                    {
                        ModConsole.LogError($"[MOP] Couldn't find {p.ObjectName} in {p.Place}.");
                        continue;
                    }

                    preventToggleOnObjects.Add(new PreventToggleOnObject(t));
                }
            }
        }

        /// <summary>
        /// Applies hinge managers to cars, if they are not using ToggleUnityCar method of disabling/enabling them,
        /// or if they are Hayosiko.
        /// </summary>
        protected virtual void ApplyHingeManager()
        {
            if (Toggle != ToggleUnityCar || vehicleType == VehiclesTypes.Hayosiko)
            {
                HingeJoint[] joints = gameObject.transform.GetComponentsInChildren<HingeJoint>();
                foreach (HingeJoint joint in joints)
                    joint.gameObject.AddComponent<HingeManager>();
            }
        }

        public delegate void ToggleHandler(bool enabled);
        public ToggleHandler Toggle;

        /// <summary>
        /// Enable or disable car
        /// </summary>
        internal virtual void ToggleActive(bool enabled)
        {
            if (gameObject == null || gameObject.activeSelf == enabled || !IsActive) return;

            // If we're disabling a car, set the audio child parent to TemporaryAudioParent, and save the position and rotation.
            // We're doing that BEFORE we disable the object.
            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);

                Position = transform.localPosition;
                Rotation = transform.localRotation;

                if (colliders)
                {
                    colliders.parent = temporaryParent;
                }
            }

            gameObject.SetActive(enabled);

            // Uppon enabling the object, set the localPosition and localRotation to the object's transform, and change audio source parents to Object
            // We're doing that AFTER we enable the object.
            if (enabled)
            {
                MoveNonDisableableObjects(null);

                if (colliders)
                {
                    colliders.parent = transform;
                    colliders.localPosition = colliderPosition;
                }
            }
        }

        /// <summary>
        /// Toggle car physics only.
        /// </summary>
        /// <param name="enabled"></param>
        public virtual void ToggleUnityCar(bool enabled)
        {
            if ((gameObject == null) || !IsActive)
                return;

            if (rb.isKinematic == !enabled && carDynamics.enabled == enabled && rb.useGravity)
                return;

            // Don't toggle physics, unless car's on ground
            if ((IsMoving() || !IsOnGround()) && !enabled)
                return;

            // If player is sitting in this specific vehicle, **NEVER** disable it.
            if (IsPlayerInThisCar())
                enabled = true;

            // Prevent disabling car physics if the rope is hooked
            if (!enabled && gameObject.activeSelf == true && IsRopeHooked())
                enabled = true;

            carDynamics.enabled = enabled;
            axles.enabled = enabled;
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;
        }

        public virtual void ForceToggleUnityCar(bool enabled)
        {
            if ((gameObject == null) || (carDynamics.enabled == enabled) || !IsActive)
                return;

            // If player is sitting in this specific vehicle, **NEVER** disable it.
            if (IsPlayerInThisCar())
                enabled = true;

            carDynamics.enabled = enabled;
            axles.enabled = enabled;
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;
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
        protected bool IsRopeHooked()
        {
            bool isFrontHookAttached = fsmHookFront ? fsmHookFront.FsmVariables.GetFsmBool("Attached").Value : false;
            bool isRearHookAttached = fsmHookRear ? fsmHookRear.FsmVariables.GetFsmBool("Attached").Value : false;
            return (isFrontHookAttached || isRearHookAttached);
        }

        /// <summary>
        /// Checks one of the wheels' onGroundDown value
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOnGround()
        {
            return wheel.onGroundDown;
        }

        /// <summary>
        /// Returns true, if the vehicle is moving.
        /// </summary>
        /// <returns></returns>
        internal bool IsMoving()
        {
            return rb.velocity.magnitude > 0.5f;
        }

        /// <summary>
        /// Disable the EventSounds component.
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleEventSounds(bool enabled)
        {
            if (eventSounds.disableSounds == !enabled) return;
            if (!enabled && rb.velocity.magnitude > 1) enabled = true;
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
            // Don't execute if player is driving this vehicle.
            if (IsPlayerInThisCar()) return false;
            if (rb.useGravity) return false;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 7, 17);

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.transform.root != null && hitCollider.transform.root.gameObject.name.EqualsAny(Hypervisor.Instance.TrafficVehicleRoots))
                {
                    return true;
                }
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
               PreventToggleOnObject p = preventToggleOnObjects[i];
                if (p.ObjectTransform == null)
                    continue;

                p.ObjectTransform.parent = parent ?? p.OriginalParent;
            }
        }

        protected virtual void DisableHooksResetting()
        {
            // Hook HookFront and HookRear
            // Get hooks first
            Transform hookFront = transform.Find("HookFront");
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
        }

        protected virtual void LoadColliders()
        {
            colliders = transform.Find("Colliders");

            if (colliders == null)
            {
                ModConsole.Log($"[MOP] Could not locate the colliders of vehicle {gameObject.name}");
                return;
            }

            colliderPosition = colliders.localPosition;
        }

        /// <summary>
        /// Sets the dummy LOD car on or off.
        /// </summary>
        public void ToggleDummyCar(bool enabled)
        {
            dummyCar?.ToggleActive(enabled, transform);
        }
    }
}
