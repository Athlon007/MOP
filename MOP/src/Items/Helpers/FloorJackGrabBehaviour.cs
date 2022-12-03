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

using UnityEngine;
using MOP.Vehicles.Cases;

namespace MOP.Items.Helpers
{
    internal class FloorJackGrabBehaviour : MonoBehaviour
    {
        private Rigidbody rb;
        private const float MinimalDistanceToSatsuma = 2.5f;

        private bool isActionApplied;
        private BoxCollider lifterCollider;
        private Vector3 lifterDefaultSize;
        private const float NewLifterY = 0.01f;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            lifterCollider = transform.Find("Lifter/lift").GetComponent<BoxCollider>();
            lifterDefaultSize = lifterCollider.size;
        }

        private void Update()
        {
            if (IsGrabbed() && IsCloseToSatsuma())
            {
                OnMove();
            }
            else
            {
                OnMoveStop();
            }
        }

        private bool IsGrabbed()
        {
            return rb.velocity.magnitude > 0;
        }

        private bool IsCloseToSatsuma()
        {
            return Vector3.Distance(Satsuma.Instance.transform.position, this.transform.position) < MinimalDistanceToSatsuma;
        }

        private void OnMove()
        {
            if (!isActionApplied)
            {
                isActionApplied = true;
                Vector3 small = lifterDefaultSize;
                small.y = NewLifterY;
                lifterCollider.size = small;
                
            }
        }

        private void OnMoveStop()
        {
            if (isActionApplied)
            {
                isActionApplied = false;
                lifterCollider.size = lifterDefaultSize;
            }
        }
    }
}