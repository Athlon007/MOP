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
using MOP.Common;

namespace MOP.FSM.Actions
{
    class CustomPlayMakerFixedUpdate : MonoBehaviour
    {
        // This script replaces the PlayMaker's PlayMakerFixedUpdate.
        // The purpose of it is to have full control over it, being able to be disabled and enabled when needed.

        readonly PlayMakerFSM[] fsms;
        bool isRunning;

        public CustomPlayMakerFixedUpdate()
        {
            if (!gameObject.name.Contains("wheel"))
            {
                Component hoodFixedUpdate = gameObject.GetComponentByName("PlayMakerFixedUpdate");

                if (hoodFixedUpdate == null)
                {
                    MSCLoader.ModConsole.Error($"[MOP] No PlayMakerFixedUpdate component found in {gameObject.name}.");
                    return;
                }

                Object.Destroy(hoodFixedUpdate);
            }
            fsms = GetComponents<PlayMakerFSM>();
        }

        void FixedUpdate()
        {
            if (!isRunning) return;

            for (int i = 0; i < fsms.Length; i++)
            {
                if (fsms[i].Active && fsms[i].Fsm.HandleFixedUpdate)
                {
                    fsms[i].Fsm.FixedUpdate();
                }
            }
        }

        public void StartFixedUpdate()
        {
            isRunning = true;
        }
    }   
}
