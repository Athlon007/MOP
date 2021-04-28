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

using MSCLoader;
using MSCLoader.Helper;
using UnityEngine;

using MOP.Common;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaCustomHoodUse : MonoBehaviour
    {
        public bool isActive;
        public bool isHoodOpen;

        PlayMakerFSM useFsm;

        void Start()
        {
            FsmHook.FsmInject(gameObject, "State 2", () => isActive = true);
            FsmHook.FsmInject(gameObject, "Mouse off", () => isActive = false);
            FsmHook.FsmInject(gameObject, "Open hood 2", () => isHoodOpen = true);
            FsmHook.FsmInject(gameObject, "Close hood 2", () => isHoodOpen = false);

            useFsm = gameObject.GetPlayMakerFSM("Use");
        }

        void FixedUpdate()
        {
            if (!IsHoodAttached()) return;
            if (!isHoodOpen && !isActive) return;
             
            if (transform.localEulerAngles.x > 340)
            { 
                useFsm.SendEvent("CLOSE");
            }
        }

        void OnEnable()
        {
            isHoodOpen = false;
            isActive = false;
        }

        bool IsHoodAttached()
        {
            if (gameObject.transform.parent == null)
                return false;

            return gameObject.transform.parent.gameObject.name == "pivot_hood";
        }
    }
}
