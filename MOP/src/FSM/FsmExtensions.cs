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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;
using MOP.FSM.Actions;

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

        /// <summary>
        /// Gets FsmState of PlayMakerFSM by name.
        /// </summary>
        /// <param name="fsm">FSM object to take State from.</param>
        /// <param name="name">State's name.</param>
        /// <returns></returns>
        public static FsmState GetState(this PlayMakerFSM fsm, string name)
        {
            return fsm.FsmStates.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// Makes the object pickable.
        /// </summary>
        /// <param name="gm">Game object to make pickable.</param>
        public static void MakePickable(this GameObject gm)
        {
            gm.layer = LayerMask.NameToLayer("Parts");
        }

        /// <summary>
        /// Removes action at the selected index.
        /// </summary>
        /// <param name="state">State to remove action from.</param>
        /// <param name="index">Index at which the action will be removed,</param>
        public static void RemoveAction(this FsmState state, int index)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.RemoveAt(index);
            state.Actions = actions.ToArray();
        }

        /// <summary>
        /// A replacement for MSCLoader's FsmHook.FsmInject.
        /// </summary>
        /// <param name="gm"></param>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void FsmInject(this GameObject gm, string name, Action action)
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

        /// <summary>
        /// Adds a new action.
        /// </summary>
        /// <param name="state">State where the actions will be inserted.</param>
        /// <param name="action"></param>
        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.Add(action);
            state.Actions = actions.ToArray();
        }

        /// <summary>
        /// Inserts action into provided index.
        /// </summary>
        /// <param name="state">State to insert action into.</param>
        /// <param name="index">Index at which to insert action.</param>
        /// <param name="action">Action to insert.</param>
        public static void InsertAction(this FsmState state, int index, FsmStateAction action)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.Insert(index, action);
            state.Actions = actions.ToArray();
        }

        /// <summary>
        /// Removes every action in the state.
        /// </summary>
        /// <param name="state">State to remove actions from.</param>
        public static void ClearActions(this FsmState state)
        {
            FsmStateAction[] actions = new FsmStateAction[] { new CustomStop() };
            state.Actions = actions.ToArray();
            state.SaveActions();
        }
    }
}
