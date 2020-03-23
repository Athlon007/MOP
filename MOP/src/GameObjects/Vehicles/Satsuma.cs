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

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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

        List<Renderer> engineBayRenderers;
        Transform pivotHood;

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
            Transform block = this.gameObject.transform.Find("Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)");
            engineBayRenderers = block.GetComponentsInChildren<Renderer>(true).ToList();
            pivotHood = this.gameObject.transform.Find("Body/pivot_hood");

            // Ignore Rule
            IgnoreRule vehicleRule = RuleFiles.instance.IgnoreRules.Find(v => v.ObjectName == this.gameObject.name);
            if (vehicleRule != null)
            {
                Toggle = ToggleUnityCar;

                if (vehicleRule.TotalIgnore)
                    IsActive = false;
            }
        }

        /// <summary>
        /// Enable or disable car
        /// </summary>
        void ToggleActive(bool enabled)
        {
            // Don't run the code, if the value is the same
            if (gameObject == null || disableableObjects[0].gameObject.activeSelf == enabled) return;

            if (MopFsmManager.IsRepairshopJobOrdered())
            {
                Toggle = ToggleUnityCar;
                return;
            }

            carDynamics.enabled = enabled;
            axles.enabled = enabled;
            rb.isKinematic = !enabled;

            if (MopSettings.SatsumaTogglePhysicsOnly) return;

            for (int i = 0; i < disableableObjects.Length; i++)
            {
                if (disableableObjects[i] == null)
                    continue;

                disableableObjects[i].gameObject.SetActive(enabled);
            }
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

        public void ToggleUnityCarOnly()
        {
            if (MopFsmManager.IsRepairshopJobOrdered())
            {
                Toggle(true);
                Toggle = ToggleUnityCar;
            }
        }

        public void ToggleRenderers(bool enabled)
        {
            if (!IsHoodAttached() && RuleFiles.instance.SpecialRules.SatsumaIgnoreEngineRenders) return;

            for (int i = 0; i< engineBayRenderers.Count; i++)
            {
                engineBayRenderers[i].enabled = enabled;
            }
        }
        
        bool IsHoodAttached()
        {
            return pivotHood.childCount > 0;
        }
    }
}
