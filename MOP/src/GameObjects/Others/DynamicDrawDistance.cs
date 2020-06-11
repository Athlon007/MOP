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

namespace MOP
{
    class DynamicDrawDistance : MonoBehaviour
    {
        Camera mainCamera;
        Transform player;

        void Start()
        {
            mainCamera = Camera.main;
            player = GameObject.Find("PLAYER").transform;
        }

        void Update()
        {
            if (!MopSettings.DynamicDrawDistance) return;

            float toGoDrawDistance = MopFsmManager.GetDrawDistance();
            if (player.position.y > 20)
                toGoDrawDistance *= 2;
            else if (WorldManager.instance.IsInSector())
                toGoDrawDistance /= 2;

            mainCamera.farClipPlane = Mathf.Lerp(mainCamera.farClipPlane, toGoDrawDistance, Time.deltaTime * 2);
        }
    }
}
