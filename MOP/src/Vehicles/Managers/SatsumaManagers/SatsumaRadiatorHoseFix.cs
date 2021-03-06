﻿// Modern Optimization Plugin
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

using MOP.Common;
using MOP.FSM.Actions;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaRadiatorHoseFix : MonoBehaviour
    {
        void Start()
        {
            PlayMakerFSM removalFSM = gameObject.GetPlayMakerByName("Removal");
            GameObject hosePrefab = GameObject.Find("CARPARTS").transform.Find("PartsCar/radiator hose3(Clone)").gameObject;
            removalFSM.FsmVariables.FindFsmGameObject("Part").Value = GameObject.Find("CARPARTS").transform.Find("PartsCar/radiator hose3(Clone)").gameObject;

            List<FsmStateAction> actions = removalFSM.FindFsmState("Remove part").Actions.ToList();
            CustomCreateObject newHose = new CustomCreateObject(gameObject, hosePrefab);
            actions.Add(newHose);
            removalFSM.FindFsmState("Remove part").Actions = actions.ToArray();
        }
    }
}
