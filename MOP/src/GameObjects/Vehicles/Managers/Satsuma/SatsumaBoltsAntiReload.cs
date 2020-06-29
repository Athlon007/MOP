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
using System.Collections.Generic;
using UnityEngine;

namespace MOP
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
                {
                    return;
                }

                fsm.Fsm.RestartOnEnable = false;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, $"BOLTS_ANTI_LOAD_SCRIPT_ERROR_{gameObject.transform.parent.gameObject.name}/{gameObject.name}");
            }
        }
    }
}
