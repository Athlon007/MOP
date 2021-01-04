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

using System.Collections;
using UnityEngine;
using MOP.Common;

namespace MOP.Vehicles.Managers
{
    class BusRollFix : MonoBehaviour
    {
        // This script fixes bus ending up on it's side by restting it's Z angle to 0.

        void OnEnable()
        {
            if (currentPositionFixRoutine != null)
            {
                StopCoroutine(currentPositionFixRoutine);
                currentPositionFixRoutine = null;
            }

            currentPositionFixRoutine = PositionFixRoutine();
            StartCoroutine(currentPositionFixRoutine);
        }

        IEnumerator currentPositionFixRoutine;
        IEnumerator PositionFixRoutine()
        {
            while (MopSettings.IsModActive)
            {
                yield return new WaitForSeconds(5);
                if (transform.localEulerAngles.z > 20 && transform.localEulerAngles.z < 340)
                {
                    // Bus won't be flipped back, if player's too close.
                    if (Vector3.Distance(Hypervisor.Instance.GetPlayer().position, transform.position) < 300) 
                        continue;

                    Vector3 fixedPosition = transform.localEulerAngles;
                    fixedPosition.z = 0;
                    transform.localEulerAngles = fixedPosition;
                }
            }
        }
    }
}
