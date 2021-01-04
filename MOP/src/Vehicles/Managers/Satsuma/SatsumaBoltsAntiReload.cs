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

namespace MOP.Vehicles.Managers
{
    /// <summary>
    /// This scripts looksa for the BoltCheck or Use scripts and disables restting of the playmaker scripts.
    /// </summary>
    class SatsumaBoltsAntiReload : MonoBehaviour
    {
        PlayMakerFSM fsm;

        public SatsumaBoltsAntiReload()
        {
            try
            {
                string fsmName = gameObject.ContainsPlayMakerByName("BoltCheck") ? "BoltCheck" : "Use";
                fsm = gameObject.GetPlayMakerByName(fsmName);

                if (fsm == null)
                    return;

                fsm.Fsm.RestartOnEnable = false;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, $"BOLTS_ANTI_LOAD_SCRIPT_ERROR_{gameObject.transform.parent.gameObject.name}/{gameObject.name}");
            }
        }
    }
}
