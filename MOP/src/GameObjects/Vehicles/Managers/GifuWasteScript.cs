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
    class GifuWasteScript : MonoBehaviour
    {
        PlayMakerFSM thisFsm;

        FsmFloat crime;
        float crimeValue;

        FsmFloat crimeEnvelopeWait;
        float crimeEnvelopeWaitValue;

        FsmFloat mass;
        float massValue;

        FsmFloat waste;
        float wasteValue;

        FsmString crimeTime;
        string crimeTimeValue;

        public GifuWasteScript()
        {
            thisFsm = GetComponent<PlayMakerFSM>();

            crime = thisFsm.FsmVariables.FindFsmFloat("Crime");
            crimeEnvelopeWait = thisFsm.FsmVariables.FindFsmFloat("CrimeEnvelopeWait");
            mass = thisFsm.FsmVariables.FindFsmFloat("Mass");
            waste = thisFsm.FsmVariables.FindFsmFloat("Waste");
            crimeTime = thisFsm.FsmVariables.FindFsmString("CrimeTime");
        }

        void OnDisable()
        {
            crimeValue = crime.Value;
            crimeEnvelopeWaitValue = crimeEnvelopeWait.Value;
            massValue = mass.Value;
            wasteValue = waste.Value;
            crimeTimeValue = crimeTime.Value;
        }

        void OnEnable()
        {
            crime.Value = crimeValue;
            crimeEnvelopeWait.Value = crimeEnvelopeWaitValue;
            mass.Value = massValue;
            waste.Value = wasteValue;
            crimeTime.Value = crimeTimeValue;
        }
    }
}
