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

using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using HutongGames.PlayMaker;
using MSCLoader;
using MSCLoader.Helper;

namespace MOP.Vehicles.Managers
{
    class KekmetHandThrottle : HandThrottle
    {
        const float MaxMinimumRPM = 500;

        public KekmetHandThrottle() : base("LOD/Dashboard/Throttle")
        {
            try
            {
                GameObject starter = transform.Find("Simulation/Starter").gameObject;

                FsmHook.FsmInject(starter, "Start engine", Invoke);
                FsmHook.FsmInject(starter, "State 1", CancelInvoke);
                Invoke();

                PlayMakerFSM starterFSM = starter.GetPlayMakerFSM("Starter");

                List<FsmStateAction> startEngineActions = starterFSM.GetState("Start engine").Actions.ToList();
                startEngineActions.RemoveAt(3);
                starterFSM.GetState("Start engine").Actions = startEngineActions.ToArray();

                List<FsmStateAction> state1Actions = starterFSM.GetState("State 1").Actions.ToList();
                state1Actions.RemoveAt(1);
                starterFSM.GetState("State 1").Actions = state1Actions.ToArray();
            }
            catch
            {
                throw new Exception("KekmetHandThrottle: Invokers error.");
            }
        }

        protected override void ThrottleUpdate()
        {
            base.ThrottleUpdate();
            MinRPM = IdleThrottle * 2500;
        }

        float MinRPM
        {
            set
            {
                if (value > MaxMinimumRPM)
                    value = MaxMinimumRPM;
                drivetrain.minRPM = value;
            }
        }
    }
}
