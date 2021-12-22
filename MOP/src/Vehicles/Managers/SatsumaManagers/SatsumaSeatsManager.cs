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

using HutongGames.PlayMaker;
using UnityEngine;

using MOP.FSM;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaSeatsManager : MonoBehaviour
    {
        // So this is a stupid fix.
        // Sets the weight of the seats rigidbody to 1, if seat is not bolted.
        // And sets it to their default value, if the screw tightness level is more than 7.

        Rigidbody rb;
        FsmFloat tightness;
        float defaultMass;
        float lastTightnessValue;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            tightness = gameObject.GetPlayMaker("BoltCheck").FsmVariables.GetFsmFloat("Tightness");

            defaultMass = rb.mass;
            lastTightnessValue = tightness.Value;

            rb.mass = tightness.Value > 7 ? defaultMass : 1;
        }

        void Update()
        {
            if (lastTightnessValue != tightness.Value)
            {
                rb.mass = tightness.Value > 7 ? defaultMass : 1;
                lastTightnessValue = tightness.Value;
            }
        }
    }
}
