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
using System;
using MSCLoader.Helper;

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
}
