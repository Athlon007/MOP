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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOP
{
    class OcclusionObject : OcclusionBase
    {
        List<Renderer> Renderers { get; set; }
        List<Light> Lights { get; set; }
        float PlayerDistance { get; set; }

        void Start()
        {
            if (!enabled || !MopSettings.EnableObjectOcclusion) 
                return; 
            
            Renderers = GetChildRenderers(gameObject);
            Lights = GetChildLights(gameObject);
            
            Renderer thisRenderer = gameObject.GetComponent<Renderer>();
            
            if (thisRenderer != null)
                Renderers.Add(thisRenderer);

            if (Renderers != null || Renderers.Count == 0)
            {
                InvokeRepeating("Toggle", MopSettings.OcclusionSampleDelay, MopSettings.OcclusionSampleDelay);
                StartCoroutine(WaitForHideDelayCalculation());
            }

            IsVisible = true;
        }

        /// <summary>
        /// Before the OcclusionCamera calculates the OcclusionHideDelay, wait for that value, then initialzie the Hide void
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitForHideDelayCalculation()
        {
            while (MopSettings.OcclusionHideDelay == -1)
            {
                yield return new WaitForSeconds(1);
            }

            InvokeRepeating("Hide", MopSettings.OcclusionSampleDelay, MopSettings.OcclusionHideDelay);
        }

        public void Hide()
        {
            if (((DateTime.Now - LastSeen).TotalSeconds > MopSettings.OcclusionHideDelay) && (PlayerDistance > MopSettings.MinOcclusionDistance))
            {
                IsVisible = false;
            }
        }

        List<Light> GetChildLights(GameObject toSearch)
        {
            List<Light> theseLights = toSearch.GetComponentsInChildren<Light>().ToList();
            return theseLights;
        }

        List<Renderer> GetChildRenderers(GameObject toSearch)
        {
            List<Renderer> theseRenderers = new List<Renderer>();
            int ChildCount = toSearch.transform.childCount;

            for (int i = 0; i < ChildCount; i++)
            {
                GameObject thisChild = toSearch.transform.GetChild(i).gameObject;

                // Fix for sleeping pivots in cars, because for whatever reason, they have their own mesh models
                if (thisChild.name.Contains("Sleep")) continue;

                if (thisChild.GetComponent<OcclusionObject>() == null)
                {
                    Renderer thisRenderer = thisChild.GetComponent<Renderer>();
                    
                    if (thisRenderer != null)
                        theseRenderers.Add(thisRenderer);

                    theseRenderers.AddRange(GetChildRenderers(thisChild));
                }
            }

            return theseRenderers;
        }

        public void Toggle()
        {
            PlayerDistance = Vector3.Distance(WorldManager.instance.Player.position, transform.position);
            bool visible = (IsVisible) || (PlayerDistance < MopSettings.MinOcclusionDistance);

            for (int i = 0; i < Renderers.Count; i++)
            {
                // We're assuming that all childs are the same
                if (Renderers[i] == null || Renderers[i].enabled == IsVisible)
                    continue;

                Renderers[i].enabled = visible;
            }

            for (int i = 0; i < Lights.Count; i++)
            {
                Lights[i].enabled = visible;
            }
        }
    }
}
