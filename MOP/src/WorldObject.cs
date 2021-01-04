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

using MSCLoader;
using UnityEngine;

using MOP.Common;
using MOP.Common.Enumerations;

namespace MOP
{
    class WorldObject
    {
        /// <summary>
        /// Game object that this instance of the class controls.
        /// </summary>
        public GameObject gameObject { get; private set; }

        /// <summary>
        /// Situations in which objects are getting disabled.
        /// </summary>
        public DisableOn DisableOn { get; private set; }

        /// <summary>
        /// How close player has to be to the object, in order to be enabled.
        /// </summary>
        public int Distance { get; private set; }

        public Transform transform => gameObject.transform;
        
        Renderer renderer;

        /// <summary>
        /// Initializes the Objects instance.
        /// </summary>
        /// <param name="gameObject">game object that this instance controls.</param>
        /// <param name="distance">From how far should that object be enabled (default is 200).</param>
        /// <param name="toggleRendererOnly">If true, only the renderer of that object will be toggled.</param>
        public WorldObject(GameObject gameObject, DisableOn disableOn, int distance = 200, bool toggleRendererOnly = false)
        {
            this.gameObject = gameObject;
            this.Distance = distance;
            this.DisableOn = disableOn;

            renderer = this.gameObject.GetComponent<Renderer>();

            // If rendererOnly is true, the Toggle will be set to ToggleMesh.
            if (toggleRendererOnly)
            {
                if (renderer == null)
                {
                    ModConsole.Error($"[MOP] Couldn't set the Toggle for {this.gameObject.name} because renderer hasn't been found.");
                    return;
                }

                Toggle = ToggleRenderer;
            }
            else
            {
                Toggle = ToggleActive;
            }
        }

        public delegate void ToggleHandler(bool enabled);
        public ToggleHandler Toggle;

        /// <summary>
        /// Enable or disable the object.
        /// </summary>
        /// <param name="enabled"></param>
        void ToggleActive(bool enabled)
        {
            if (DisableOn.HasFlag(DisableOn.IgnoreInQualityMode) && MopSettings.Mode == PerformanceMode.Quality) enabled = true;

            if (this.gameObject != null && this.gameObject.activeSelf != enabled)
                this.gameObject.SetActive(enabled);
        }

        /// <summary>
        /// Enable or disable object's renderer
        /// </summary>
        /// <param name="enabled"></param>
        void ToggleRenderer(bool enabled)
        {
            if (renderer.enabled != enabled)
            {
                renderer.enabled = enabled;
            }
        }
    }
}
