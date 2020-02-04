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

using MSCLoader;
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
        Transform temporaryParent;

        // List of non unloadable objects
        List<UnloadableObject> unloadableObjects;

        // Overwrites the "Component.transform", to prevent eventual mod crashes caused by missuse of Vehicle.transform.
        // Technically, you should use Vehicle.Object.transform (ex. GIFU.Object.Transform), this here just lets you use Vehicle.transform
        // (ex. GIFU.transform).
        public Transform transform => gameObject.transform;

        // Loaded only for Satsuma
        public Satsuma satsumaScript;

        // Unity car systems and rigidbody
        internal CarDynamics carDynamics;
        internal Axles axles;
        internal Rigidbody rb;

        // Applies extra fixes, if is set to true.
        readonly bool isHayosiko;

        // Applies for Flatbed only, if true the flatbed unloading will not happen
        bool flatbedUnloadPreventionActivated;

        // Prevents MOP from disabling car's physics when the car has rope hooked
        internal bool IsRopeHooked { get; set; }

        /// <summary>
        /// Initialize class
        /// </summary>
        /// <param name="gameObjectName"></param>
        public Vehicle(string gameObjectName)
        {
            // gameObject the object by name
            gameObject = GameObject.Find(gameObjectName);

            // Get the object position and rotation
            Position = gameObject.transform.localPosition;
            Rotation = gameObject.transform.localRotation;

            // Creates a new gameobject that is names after the original file + '_TEMP' (ex. "SATSUMA(557kg, 248)_TEMP")
            temporaryParent = new GameObject(gameObject.name + "_TEMP").transform;

            unloadableObjects = new List<UnloadableObject>();

            // Get the object's child which are responsible for audio
            foreach (Transform audioObject in FindAudioObjects())
            {
                unloadableObjects.Add(new UnloadableObject(audioObject));
            }

            // Fix for fuel level resetting after respawn
            Transform fuelTank = gameObject.transform.Find("FuelTank");
            if (fuelTank != null)
            {
                unloadableObjects.Add(new UnloadableObject(fuelTank));
            }

            // If the vehicle is Gifu, find knobs and add them to list of unloadableObjects
            if (gameObject.name == "GIFU(750/450psi)")
            {
                unloadableObjects.Add(new UnloadableObject(gameObject.transform.Find("Dashboard").Find("Knobs")));
            }

            carDynamics = gameObject.GetComponent<CarDynamics>();
            axles = gameObject.GetComponent<Axles>();
            rb = gameObject.GetComponent<Rigidbody>();

            if (!gameObject.name.ContainsAny("HAYOSIKO", "FURY"))
            {
                gameObject.AddComponent<OcclusionObject>();
            }

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
                for (int i = 0; i < unloadableObjects.Count; i++)
                    unloadableObjects[i].ObjectTransform.parent = temporaryParent;

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

                for (int i = 0; i < unloadableObjects.Count; i++)
                    unloadableObjects[i].ObjectTransform.parent = unloadableObjects[i].Parent;
            }
        }

        /// <summary>
        /// Toggle car physics only.
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleUnityCar(bool enabled)
        {
            if ((gameObject == null) || (carDynamics.enabled == enabled) || (satsumaScript != null && satsumaScript.IsSatsumaInInspectionArea && enabled == false)) 
                return;

            // Prevent disabling car physics if the rope is hooked
            if (IsRopeHooked && gameObject.activeSelf == true) return;

            carDynamics.enabled = enabled;
            axles.enabled = enabled;
            rb.isKinematic = !enabled;
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
        /// Toggled uppon firewood being placed on the bed of flatbed.
        /// Switches the toggling method to UnityCar only, so the flatbed won't be unloaded completly.
        /// </summary>
        void FlatbedSwitchToggleMethod()
        {
            if (flatbedUnloadPreventionActivated) return;
            flatbedUnloadPreventionActivated = true;

            Toggle = ToggleUnityCar;
        }

        /// <summary>
        /// Hooks up to all car hooks.
        /// Called when player attaches the rope.
        /// </summary>
        void RopeHookUp()
        {
            IsRopeHooked = true;
        }

        /// <summary>
        /// Hooks up to all car hooks.
        /// Called when player dismounts the rope.
        /// </summary>
        void RopeUnhook()
        {
            IsRopeHooked = false;
        }
    }
}
