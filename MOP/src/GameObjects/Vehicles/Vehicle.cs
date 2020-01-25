﻿using MSCLoader;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace MOP
{
    class Vehicle
    {
        // Vehicle class - made by Konrad "Athlon" Figura
        // 
        // It fixes known issue with missing vehicles engine sound by (admittedly hacky way) creating new GameObject,
        // that stores GameObjects responsible for playing sounds in vehicle (named "audio", or "SoundSrc").
        // Whenever the vehicle is disabled, all audio objects are changing parent to that temporary object.
        //
        // It also fixes the issue of vehicles going back to the original spawn position, instead of staying in the same place - as they should,
        // by simply saving the Transform.position and Transform.rotation parameters just before disabling the object, and then loading these values,
        // just after loading them.

        public GameObject gameObject { get; set; }

        // Values that are being saved or loaded
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        // All objects that cannot be unloaded (because it causes problems) land under that object
        internal GameObject TemporaryParent;

        // List of non unloadable objects
        internal Transform[] AudioObjects;
        internal Transform FuelTank;    

        // Overwrites the "Component.transform", to prevent eventual mod crashes caused by missuse of Vehicle.transform.
        // Technically, you should use Vehicle.Object.transform (ex. GIFU.Object.Transform), this here just lets you use Vehicle.transform
        // (ex. GIFU.transform).
        public Transform transform => gameObject.transform;

        public Satsuma satsumaScript;

        internal CarDynamics carDynamics;
        internal Axles axles;
        internal Rigidbody rb;

        bool isHayosiko;

        bool flatbedScriptActivated;

        // Prevents MOP from disabling car's physics when the car has rope hooked
        internal bool IsRopeHooked { get; set; }

        /// <summary>
        /// Initialize class
        /// </summary>
        /// <param name="gameObjectName"></param>
        public Vehicle(string gameObjectName)
        {
            // Find the object by name
            gameObject = GameObject.Find(gameObjectName);

            // Dump the object position and rotation
            Position = gameObject.transform.localPosition;
            Rotation = gameObject.transform.localRotation;

            // Creates a new gameobject that is names after the original file + '_TEMP' (ex. "SATSUMA(557kg, 248)_TEMP")
            TemporaryParent = new GameObject(gameObject.name + "_TEMP");

            // Get the object's child which are responsible for audio
            AudioObjects = FindAudioObjects();

            // Fix for fuel level resetting after respawn
            FuelTank = FindFuelTank();

            carDynamics = gameObject.GetComponent<CarDynamics>();
            axles = gameObject.GetComponent<Axles>();
            rb = gameObject.GetComponent<Rigidbody>();

            if (!gameObject.name.ContainsAny("HAYOSIKO", "FURY"))
                gameObject.AddComponent<OcclusionObject>();

            // Hook HookFront and HookRear
            // Get hooks first
            Transform hookFront = transform.Find("HookFront");
            Transform hookRear = transform.Find("HookRear");

            // If hooks exists, attach the RopeHookUp and RopeUnhook to appropriate states
            if (hookFront != null)
            {
                FsmHook.FsmInject(hookFront.gameObject, "Activate cable", RopeHookUp);
                FsmHook.FsmInject(hookFront.gameObject, "Activate cable 2", RopeHookUp);
                FsmHook.FsmInject(hookFront.gameObject, "Remove rope", RopeUnhook);
            }

            if (hookRear != null)
            {
                FsmHook.FsmInject(hookRear.gameObject, "Activate cable", RopeHookUp);
                FsmHook.FsmInject(hookRear.gameObject, "Activate cable 2", RopeHookUp);
                FsmHook.FsmInject(hookRear.gameObject, "Remove rope", RopeUnhook);
            }
            
            // If vehicle is flatbed, hook SwitchToggleMethod to Add scale script
            if (gameObject.name == "FLATBED")
            {
                FsmHook.FsmInject(transform.Find("Bed/LogTrigger").gameObject, "Add scale", FlatbedSwitchToggleMethod);
            }

            // Set default toggling method - that is entire vehicle
            Toggle = ToggleActive;

            isHayosiko = gameObject.name.Contains("HAYOSIKO");

            // Fix for Offroad Hayosiko and HayosikoColorfulGauges
            if (isHayosiko && (CompatibilityManager.instance.OffroadHayosiko || CompatibilityManager.instance.HayosikoColorfulGauges))
            {
                Toggle = ToggleUnityCar;
            }

            // If the vehicle is Fury
            if (gameObject.name == "FURY(1630kg)" || gameObject.name == "POLICEFERNDALE(1630kg)")
            {
                Toggle = ToggleUnityCar;
            }

            // If the user selected to toggle vehicle's physics only, it overrided any previous set for Toggle method
            if (MopSettings.ToggleVehiclePhysicsOnly)
            {
                Toggle = ToggleUnityCar;
            }
        }


        public delegate void ToggleHandler(bool enabled);
        public ToggleHandler Toggle;

        /// <summary>
        /// Enable or disable car
        /// </summary>
        void ToggleActive(bool enabled)
        {
            if (gameObject == null) return;
            // Don't run the code, if the value is the same
            if (gameObject.activeSelf == enabled) return;

            // Fix for when the player doesn't have keys for Hayosiko.
            // Van will NOT be toggled
            if (isHayosiko && MopFsmManager.PlayerHasHayosikoKey() == false)
            {
                ToggleUnityCar(enabled);
                return;
            }

            // If we're disabling a car, set the audio child parent to TemporaryAudioParent, and save the position and rotation.
            // We're doing that BEFORE we disable the object.
            if (!enabled)
            {
                SetParentForChilds(AudioObjects, TemporaryParent);
                if (FuelTank != null)
                {
                    SetParentForChild(FuelTank, TemporaryParent);
                }

                Position = gameObject.transform.localPosition;
                Rotation = gameObject.transform.localRotation;
            }

            gameObject.SetActive(enabled);

            // Uppon enabling the file, set the localPosition and localRotation to the object's transform, and change audio source parents to Object
            // We're doing that AFTER we enable the object.
            if (enabled)
            {
                gameObject.transform.localPosition = Position;
                gameObject.transform.localRotation = Rotation;

                SetParentForChilds(AudioObjects, gameObject);
                if (FuelTank != null)
                {
                    SetParentForChild(FuelTank, gameObject);
                }
            }
        }

        /// <summary>
        /// Toggle car physics only.
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleUnityCar(bool enabled)
        {
            if (gameObject == null || carDynamics.enabled == enabled || (satsumaScript != null && satsumaScript.IsSatsumaInInspectionArea && enabled == false)) return;

            // Prevent disabling car physics if the rope is hooked
            if (IsRopeHooked && gameObject.activeSelf == true) return;

            carDynamics.enabled = enabled;
            axles.enabled = enabled;
            rb.isKinematic = !enabled;
        }

        /// <summary>
        /// Changes audio childs parent
        /// </summary>
        /// <param name="childs"></param>
        /// <param name="newParent"></param>
        internal void SetParentForChilds(Transform[] childs, GameObject newParent)
        {
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i].parent = newParent.transform;
            }
        }

        /// <summary>
        /// Changes parent for single child
        /// </summary>
        /// <param name="child"></param>
        /// <param name="newParent"></param>
        internal void SetParentForChild(Transform child, GameObject newParent)
        {
            child.transform.parent = newParent.transform;
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
        /// Looks for FuelTank object.
        /// Returns null if it hasn't been found.
        /// </summary>
        Transform FindFuelTank()
        {
            if (gameObject.transform.Find("FuelTank") != null)
            {
                return gameObject.transform.Find("FuelTank");
            }

            return null;
        }

        /// <summary>
        /// Toggled uppon firewood being placed on the bed of flatbed.
        /// Switches the toggling method to UnityCar only, so the flatbed won't be unloaded completly.
        /// </summary>
        void FlatbedSwitchToggleMethod()
        {
            if (flatbedScriptActivated) return;
            flatbedScriptActivated = true;

            Toggle = ToggleUnityCar;
        }

        void RopeHookUp()
        {
            IsRopeHooked = true;
        }

        void RopeUnhook()
        {
            IsRopeHooked = false;
        }
    }
}
