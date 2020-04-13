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

        Transform[] disableableObjects;
        string[] whiteList;

        public bool IsSatsumaInInspectionArea { get; set; }

        List<Renderer> renderers;
        bool renderersToggled;

        List<Renderer> engineBayRenderers;
        Transform pivotHood;

        bool currentStatus = true;

        bool preventDespawnDuringThisSession;

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
            Transform body = this.gameObject.transform.Find("Body");
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

            // Adding components to normal and bucket seats.
            GameObject.Find("seat driver(Clone)").AddComponent<SatsumaSeatsManager>();
            GameObject.Find("seat passenger(Clone)").AddComponent<SatsumaSeatsManager>();

            GameObject bucketDriver = GameObject.Find("bucket seat driver(Clone)");
            GameObject bucketPassanger = GameObject.Find("bucket seat passenger(Clone)");
            if (bucketDriver == null)
            {
                bucketDriver = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "bucket seat driver(Clone)" && g.transform.parent.gameObject.name == "Parts");
                bucketPassanger = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "bucket seat passenger(Clone)" && g.transform.parent.gameObject.name == "Parts");
            }
            bucketDriver.AddComponent<SatsumaSeatsManager>();
            bucketPassanger.AddComponent<SatsumaSeatsManager>();
        }

        /// <summary>
        /// Enable or disable car
        /// </summary>
        new void ToggleActive(bool enabled)
        {
            // Don't run the code, if the value is the same
            if (gameObject == null || currentStatus == enabled) return;

            currentStatus = enabled;

            if (!enabled)
                lastGoodRotation = transform.rotation;

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
            if (renderersToggled) return;

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

            renderersToggled = enabled;
        }

        /// <summary>
        /// Yeah, we're literally forcing this fucker to stay rotated at the last good rotation.
        /// For some fucking reason it keeps spinning and shit.
        /// </summary>
        public void ForceFuckingRotation()
        {
            if (!carDynamics.enabled)
                transform.rotation = lastGoodRotation;
        }
    }
}
