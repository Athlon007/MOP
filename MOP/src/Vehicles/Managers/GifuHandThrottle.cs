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
using System;

namespace MOP.Vehicles.Managers
{
    class GifuHandThrottle : HandThrottle
    {
        readonly GameObject key;

        bool isInvoked;

        public GifuHandThrottle() : base("LOD/Dashboard/ButtonHandThrottle")
        {
            try
            {
                key = transform.Find("LOD/Dashboard/KeyHole/Keys/Key").gameObject;
                handThrottleValue.Value = 0.13f;
            }
            catch
            {
                throw new Exception("GifuHandThrottle: Key not found.");
            }
        }

        void Update()
        {
            if (isInvoked && !key.activeSelf)
            {
                isInvoked = false;
                CancelInvoke();
            }
            else if (!isInvoked && key.activeSelf)
            {
                isInvoked = true;
                Invoke();
            }
        }
    }
}
