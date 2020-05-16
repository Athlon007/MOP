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
    class SatsumaBoltsAntiReload : MonoBehaviour
    {
        string fsmName;

        public SatsumaBoltsAntiReload()
        {
            try
            {
                fsmName = gameObject.ContainsPlayMakerByName("BoltCheck") ? "BoltCheck" : "Use";
                PlayMakerFSM fsm = gameObject.GetPlayMakerByName(fsmName);

                if (fsm == null)
                {
                    return;
                }

                FsmState loadArray = fsm.FindFsmState(fsmName == "Use" ? "Load" : "Load array");
                List<FsmStateAction> loadArrayActions = new List<FsmStateAction> { new CustomNullState() };
                loadArray.Actions = loadArrayActions.ToArray();
                loadArray.SaveActions();

                if (fsmName == "Use") return;

                FsmState loadFloat = fsm.FindFsmState("Load float");
                List<FsmStateAction> loadFloatActions = new List<FsmStateAction> { new CustomNullState() };
                loadFloat.Actions = loadFloatActions.ToArray();
                loadFloat.SaveActions();

                if (fsm.FindFsmState("Load float 2") != null)
                {
                    FsmState loadFloatX = fsm.FindFsmState("Load float 2");
                    loadFloatX.Actions = new FsmStateAction[] { new CustomNullState() };
                    loadFloatX.SaveActions();
                }
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, $"BOLTS_ANTI_LOAD_SCRIPT_ERROR_{gameObject.transform.parent.gameObject.name}/{gameObject.name}");
            }
        }
    }

    public class CustomNullState : FsmStateAction
    {
        public override void OnEnter()
        {
            Finish();
        }
    }

    public class CustomStopAction : FsmStateAction
    {
        public override void OnEnter()
        {
            Fsm.Event("FINISHED");
            Finish();
        }
    }
}
