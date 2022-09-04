﻿// Modern Optimization Plugin
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

using System;
using System.Collections;
using UnityEngine;

using MOP.Common;
using MOP.Common.Enumerations;
using MOP.FSM;
using MOP.Managers;

namespace MOP.Helpers
{
    class DynamicDrawDistance : MonoBehaviour
    {
        private Camera mainCamera;
        private Transform player;

        const int MaxDrawDistance = 5000; // This is how far player can see anyway.
        const int FarRenderDistanceY = 20; // Player height after which the render distance is maximized.

        void Start()
        {
            mainCamera = Camera.main;
            player = GameObject.Find("PLAYER").transform;

            StartCoroutine(UpdateDrawDistance());
        }

        IEnumerator UpdateDrawDistance()
        {
            while (MopSettings.IsModActive)
            {
                yield return new WaitForSeconds(0.5f);

                try
                {
                    if (mainCamera == null)
                    {
                        mainCamera = Camera.main;
                    }

                    if (!MOP.DynamicDrawDistance.GetValue())
                    {
                        continue;
                    }

                    float targetDistance = CalculateDrawDistance();

                    if (mainCamera == null)
                    {
                        throw new NullReferenceException("Main camera is null.");
                    }

                    // Finally, we set the render distance.
                    mainCamera.farClipPlane = targetDistance;
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "DRAW_DISTANCE_ERROR");
                    ResetFarClip();
                    break;
                }
            }
        }

        /// <summary>
        /// Decides if DDD can use far rendering.<br>
        /// Performance mode MUST be set to Balanced or higher, and player Y position must be higher than FarRenderDistanceY.
        /// </summary>
        /// <returns></returns>
        private bool IsFarRenderDistanceApplicable()
        {
            return MopSettings.Mode >= PerformanceMode.Balanced && player.position.y > FarRenderDistanceY;
        }

        private float CalculateDrawDistance()
        {
            float targetDistance = FsmManager.GetDrawDistance();
            if (IsFarRenderDistanceApplicable())
            {
                // Player is at high elevation (ski jump hill)?
                // Set render distance to far.
                targetDistance = MaxDrawDistance;
            }
            else if (SectorManager.Instance.IsPlayerInSector())
            {
                // Player in sector?
                // Set the render distance to sector's prefered distance..
                targetDistance = SectorManager.Instance.GetCurrentSectorDrawDistance();
            }

            if (targetDistance > FsmManager.GetDrawDistance() && !IsFarRenderDistanceApplicable())
            {
                // Is the target distance HIGHER than the player's prefered render distance,
                // AND far render distance IS NOT applicable?
                // Set the render distance to the player's prefered render distance.
                targetDistance = FsmManager.GetDrawDistance();
            }

            return targetDistance;
        }

        private void ResetFarClip()
        {
            try
            {
                if (Camera.main == null)
                {
                    throw new Exception("Main Camera does not exist.");
                }

                Camera.main.farClipPlane = FsmManager.GetDrawDistance();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "RESET_FAR_CLIP_ERROR");
            }
        }
    }
}