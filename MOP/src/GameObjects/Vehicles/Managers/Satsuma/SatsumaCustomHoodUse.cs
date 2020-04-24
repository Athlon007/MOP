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

using MSCLoader;
using UnityEngine;

namespace MOP
{
    class SatsumaCustomHoodUse : MonoBehaviour
    {
        //HingeJoint joint;

        public bool isActive;
        public bool isHoodOpen;

        PlayMakerFSM useFsm;

        void Start()
        {
            //joint = GetComponent<HingeJoint>();
            FsmHook.FsmInject(gameObject, "State 2", OnClose);
            FsmHook.FsmInject(gameObject, "Mouse off", OnCloseAbandon);
            FsmHook.FsmInject(gameObject, "Open hood 2", HoodOpen);
            FsmHook.FsmInject(gameObject, "Close hood 2", HoodClose);

            useFsm = gameObject.GetPlayMakerByName("Use");
        }

        void Update()
        {
            if (!IsHoodAttached()) return;
            if (!isHoodOpen) return;
             
            if (isActive && transform.localRotation.x > 350)
            {
                useFsm.SendEvent("CLOSE");
            }
        }

        void OnClose()
        {
            isActive = true;
        }

        void OnCloseAbandon()
        {
            isActive = false;
        }

        void HoodOpen()
        {
            isHoodOpen = true;
        }

        void HoodClose()
        {
            isHoodOpen = false;
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
