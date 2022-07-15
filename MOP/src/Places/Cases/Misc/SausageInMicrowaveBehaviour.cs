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

namespace MOP.Places.Cases.Misc
{
    class SausageInMicrowaveBehaviour : MonoBehaviour
    {
        private const float RotateSpeed = 0.5f;
        
        private float timer;
        private const float WaitTime = 4f;

        private void FixedUpdate()
        {
            if (timer < WaitTime)
            {
                timer += Time.deltaTime;
                return;
            }

            Vector3 euler = transform.localEulerAngles;
            euler.z += RotateSpeed;
            transform.localEulerAngles = euler;
        }

        private void OnEnable()
        {
            timer = 0;
        }
    }
}
