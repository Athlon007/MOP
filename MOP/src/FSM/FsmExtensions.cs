// Modern Optimization Plugin
// Copyright(C) 2019-2022 Athlon

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
using MOP.FSM.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOP.FSM
{
    static class PlayMakerExtensions
    {
        public static PlayMakerFSM GetPlayMaker(this GameObject gm, string name)
        {
            return gm?.GetComponents<PlayMakerFSM>()?.FirstOrDefault(fsm => fsm.FsmName == name);
        }

        public static PlayMakerFSM GetPlayMaker(this Transform t, string name)
        {
            return GetPlayMaker(t.gameObject, name);
        }

        public static FsmState GetState(this PlayMakerFSM fsm, string name)
        {
            return fsm.FsmStates.FirstOrDefault(s => s.Name == name);
        }

        public static void MakePickable(this GameObject gm)
        {
            gm.layer = LayerMask.NameToLayer("Parts");
        }

        public static void RemoveAction(this FsmState state, int index)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.RemoveAt(index);
            state.Actions = actions.ToArray();
        }

        public static void FsmInject(GameObject gm, string name, Action action)
        {
            PlayMakerFSM fsm = gm.GetComponent<PlayMakerFSM>();

            if (fsm)
            {
                FsmState state = fsm.GetState(name);
                List<FsmStateAction> actions = state.Actions.ToList();
                CustomStateAction customStateAction = new CustomStateAction(action);
                actions.Add(customStateAction);
                state.Actions = actions.ToArray();
            }
        }

        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            List<FsmStateAction> actions = state.ActiveActions.ToList();
            actions.Add(action);
            state.Actions = actions.ToArray();
        }
    }
}
