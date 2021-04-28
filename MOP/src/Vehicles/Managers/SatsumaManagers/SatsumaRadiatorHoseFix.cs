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

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using MSCLoader.Helper;

using MOP.Common;
using MOP.FSM.Actions;
using MOP.Managers;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaRadiatorHoseFix : MonoBehaviour
    {
        void Start()
        {
            PlayMakerFSM removalFSM = gameObject.GetPlayMakerFSM("Removal");
            GameObject hosePrefab = ItemsManager.Instance.GetRadiatorHose3();
            removalFSM.FsmVariables.FindFsmGameObject("Part").Value = ItemsManager.Instance.GetRadiatorHose3();

            List<FsmStateAction> actions = removalFSM.FindFsmState("Remove part").Actions.ToList();
            CustomCreateObject newHose = new CustomCreateHose(gameObject, hosePrefab);
            actions.Insert(0, newHose);
            removalFSM.FindFsmState("Remove part").Actions = actions.ToArray();
        }
    }
}
