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
using System.Linq;
using UnityEngine;

namespace MOP
{
    class SatsumaBoltsAntiReload : MonoBehaviour
    {
        // HACK: This script forces hood to have it's joint's break force and break torque set to 1000.

        public SatsumaBoltsAntiReload()
        {
            PlayMakerFSM fsm = gameObject.GetPlayMakerByName("BoltCheck");

            if (fsm == null)
            {
                MSCLoader.ModConsole.Print($"[MOP] No FSM BoltCheck for {gameObject.name} found");
                return;
            }
            
            FsmState loadArray = fsm.FindFsmState("Load array");
            List<FsmStateAction> loadArrayActions = loadArray.Actions.ToList();
            loadArrayActions[0] = new CustomNullState();
            loadArray.Actions = loadArrayActions.ToArray();
            loadArray.SaveActions();

            FsmState loadFloat = fsm.FindFsmState("Load float");
            List<FsmStateAction> loadFloatActions = loadFloat.Actions.ToList();
            loadFloatActions[0] = new CustomNullState();
            loadFloat.Actions = loadFloatActions.ToArray();
            loadFloat.SaveActions();
        }
    }

    public class CustomBoltAction : FsmStateAction
    {
        HingeJoint joint;
        public FsmFloat tightness;
        public FsmBool databaseBolted; // db_ThisPart FSMGameObject -> Data fsm -> Bolted FSMBool

        public override void OnEnter()
        {
            joint = Fsm.GameObject.GetComponent<HingeJoint>();
            tightness = Fsm.Variables.GetFsmFloat("Tightness");
            joint.breakForce = tightness.Value * 400;
            joint.breakTorque = tightness.Value * 400;

            Finish();
        }
    }

    public class CustomNullState : FsmStateAction
    {
        public override void OnEnter()
        {
            Finish();
        }
    }
}
