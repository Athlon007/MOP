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

using UnityEngine;

using MOP.Common.Enumerations;

namespace MOP.WorldObjects
{
    class RendererToggle : GenericObject
    {
        Renderer renderer;

        public RendererToggle(GameObject gameObject, DisableOn disableOn, int distance = 200) : base(gameObject, distance, disableOn)
        {
            renderer = this.gameObject.GetComponent<Renderer>();

            if (renderer == null)
            {
                throw new System.Exception($"[MOP] Couldn't find the renderer of {gameObject.name}");
            }
        }

        public override void Toggle(bool enabled)
        {
            if (!renderer.gameObject.activeSelf || renderer.enabled == enabled)
            {
                return;
            }

            renderer.enabled = enabled;
        }
    }
}
