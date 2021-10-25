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
using MSCLoader;
using MSCLoader.Helper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace MOP.Vehicles.Managers
{
    class HandThrottle : MonoBehaviour
    {
        // This class overwrites the hand throttle systems found in Kekmet and Gifu.

        protected Drivetrain drivetrain;
        protected AxisCarController axisCarController;
        protected FsmFloat handThrottleValue;

        const float ThrottleMax = 1;

        public HandThrottle(string throttlePath)
        {
            try
            {
                drivetrain = gameObject.GetComponent<Drivetrain>();
                axisCarController = gameObject.GetComponent<AxisCarController>();
            }
            catch
            {
                throw new Exception("HandThrottle: Components not found.");
            }

            try
            {
                Transform handThrottle = transform.Find(throttlePath);
                handThrottleValue = handThrottle.GetPlayMakerFSM("Use").FsmVariables.GetFsmFloat("Throttle");
                handThrottle.GetPlayMakerFSM("Throttle").enabled = false;
            }
            catch
            {
                throw new Exception("HandThrottle: FSM tweaks issue.");
            }
        }

        protected void Invoke()
        {
            InvokeRepeating("ThrottleUpdate", 0, 0.000015f);
        }

        protected virtual void ThrottleUpdate()
        {
            IdleThrottle = handThrottleValue.Value;
            Throttle = axisCarController.throttle + IdleThrottle;
        }

        protected float Throttle
        {
            get => drivetrain.throttle;
            set
            {
                if (value > ThrottleMax)
                    value = ThrottleMax;

                drivetrain.throttle = value;
            }
        }

        protected float IdleThrottle
        {
            get => drivetrain.idlethrottle;
            set => drivetrain.idlethrottle = value;
        }
    }
    
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

                List<FsmStateAction> s = starterFSM.GetState("Start engine").Actions.ToList();
                s.RemoveAt(3);
                starterFSM.GetState("Start engine").Actions = s.ToArray();

                List<FsmStateAction> s2 = starterFSM.GetState("State 1").Actions.ToList();
                s2.RemoveAt(1);
                starterFSM.GetState("State 1").Actions = s2.ToArray();
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
