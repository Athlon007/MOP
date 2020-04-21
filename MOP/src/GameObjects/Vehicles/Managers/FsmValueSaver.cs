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

using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;

namespace MOP
{
    class FsmFloatManager
    {
        public FsmFloat StoredFsmFloat;
        public float Value;

        public FsmFloatManager(FsmFloat fsmValue)
        {
            StoredFsmFloat = fsmValue;
            Value = fsmValue.Value;
        }
    }

    class FsmValueSaver : MonoBehaviour
    {
        List<FsmFloatManager> fsmFloats;

        public FsmValueSaver()
        {
            fsmFloats = new List<FsmFloatManager>();
            PlayMakerFSM fsm = GetComponent<PlayMakerFSM>();
            
            foreach (FsmFloat fsmf in fsm.FsmVariables.FloatVariables)
                fsmFloats.Add(new FsmFloatManager(fsmf));
        }

        void OnDisable()
        {
            for (int i = 0; i < fsmFloats.Count; i++)
                fsmFloats[i].Value = fsmFloats[i].StoredFsmFloat.Value;
        }

        void OnEnable()
        {
            for (int i = 0; i < fsmFloats.Count; i++)
                fsmFloats[i].StoredFsmFloat.Value = fsmFloats[i].Value;
        }
    }
}
