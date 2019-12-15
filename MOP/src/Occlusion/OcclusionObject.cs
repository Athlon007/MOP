using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOP
{
    class OcclusionObject : OcclusionBase
    {
        List<Renderer> renderers { get; set; }
        List<Light> lights { get; set; }
        float PlayerDistance { get; set; }

        void Start()
        {
            if (!enabled || !MopSettings.EnableObjectOcclusion) 
                return; 
            
            renderers = GetChildRenderers(gameObject);
            lights = GetChildLights(gameObject);
            
            Renderer thisRenderer = gameObject.GetComponent<Renderer>();
            if (thisRenderer != null)
                renderers.Add(thisRenderer);

            if (renderers != null || renderers.Count == 0)
            {
                InvokeRepeating("Toggle", MopSettings.OcclusionSampleDelay, MopSettings.OcclusionSampleDelay);
                InvokeRepeating("Hide", MopSettings.OcclusionSampleDelay, MopSettings.OcclusionHideDelay);
            }

            isVisible = true;
        }

        public void Hide()
        {
            if (((DateTime.Now - lastSeen).TotalSeconds > MopSettings.OcclusionHideDelay) && (PlayerDistance > MopSettings.MinOcclusionDistance))
            {
                isVisible = false;
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

                // Fix for sleeping pivots in cars
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
            PlayerDistance = Vector3.Distance(WorldManager.instance.player.position, transform.position);
            bool visible = (isVisible) || (PlayerDistance < MopSettings.MinOcclusionDistance);

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i].enabled == isVisible)
                    return;

                renderers[i].enabled = visible;
            }

            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].enabled = visible;
            }
        }
    }
}
