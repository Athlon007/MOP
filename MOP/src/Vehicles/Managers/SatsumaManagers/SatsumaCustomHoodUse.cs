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

using UnityEngine;

using MOP.FSM;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaCustomHoodUse : MonoBehaviour
    {
        public bool isActive;
        public bool isHoodOpen;

        PlayMakerFSM useFsm;

        void Start()
        {
            MSCLoader.FsmHook.FsmInject(gameObject, "State 2", () => isActive = true);
            MSCLoader.FsmHook.FsmInject(gameObject, "Mouse off", () => isActive = false);
            MSCLoader.FsmHook.FsmInject(gameObject, "Open hood 2", () => isHoodOpen = true);
            MSCLoader.FsmHook.FsmInject(gameObject, "Close hood 2", () => isHoodOpen = false);

            useFsm = gameObject.GetPlayMaker("Use");
        }

        void FixedUpdate()
        {
            if (!IsHoodAttached() || (!isHoodOpen && !isActive)) return;
             
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
            if (transform.parent == null)
                return false;

            return transform.parent.gameObject.name == "pivot_hood";
        }
    }
}
