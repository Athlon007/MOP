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

using HutongGames.PlayMaker;
using UnityEngine;

namespace MOP
{
    class SatsumaSeatsManager : MonoBehaviour
    {
        readonly Rigidbody rb;
        readonly FsmFloat tightness;
        readonly float defaultMass;
        float lastTightnessValue;

        public SatsumaSeatsManager()
        {
            rb = GetComponent<Rigidbody>();
            Component[] fsms = GetComponents(typeof(PlayMakerFSM));            
            foreach (PlayMakerFSM f in fsms)
                if (f.FsmVariables.GetFsmFloat("Tightness") != null)
                    tightness = f.FsmVariables.GetFsmFloat("Tightness");

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
