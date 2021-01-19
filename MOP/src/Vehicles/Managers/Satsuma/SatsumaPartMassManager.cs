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

using HutongGames.PlayMaker;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOP.Common;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaPartMassManager : MonoBehaviour
    {
        float objectMass;
        FsmFloat carMass;

        bool isClean;

        void OnEnable()
        {
            if (!isClean)
            {
                isClean = true;

                try
                {
                    PlayMakerFSM removalFSM = gameObject.GetPlayMakerByName("Removal");

                    if (!removalFSM)
                    {
                        throw new System.Exception($"SatsumaPartMassManager: {gameObject.name} - Removal PlayMakerFSM is missing!\n\n" +
                                                   $"Path: {gameObject.GetGameObjectPath()}");
                    }

                    objectMass = removalFSM.FsmVariables.GetFsmFloat("Mass").Value;
                    carMass = PlayMakerGlobals.Instance.Variables.FindFsmFloat("CarMass");
                    if (this.gameObject.name.ContainsAny("strut", "antenna", "marker light"))
                    {
                        FsmState disableTrigger = removalFSM.FindFsmState("Disable trigger");
                        List<FsmStateAction> actionList = disableTrigger.Actions.ToList();
                        actionList.RemoveAt(0);
                        disableTrigger.Actions = actionList.ToArray();

                        FsmState removePart = removalFSM.FindFsmState("Remove part");
                        List<FsmStateAction> rpActionList = removePart.Actions.ToList();
                        rpActionList.RemoveAt(9);
                        removePart.Actions = rpActionList.ToArray();
                    }
                    else
                    {
                        FsmState disableTrigger = removalFSM.FindFsmState("Add mass");
                        List<FsmStateAction> actionList = disableTrigger.Actions.ToList();
                        actionList.RemoveAt(0);
                        disableTrigger.Actions = actionList.ToArray();

                        FsmState removePart = removalFSM.FindFsmState("Remove part");
                        List<FsmStateAction> rpActionList = removePart.Actions.ToList();
                        rpActionList.RemoveAt(7);
                        removePart.Actions = rpActionList.ToArray();
                    }

                    if (this.gameObject.activeSelf)
                    {
                        carMass.Value -= objectMass;
                    }
                }
                catch
                {
                    throw new System.Exception($"SatsumaPartMassManager: There has been a problem with {gameObject.GetGameObjectPath()}");
                }
            }

            if (gameObject.transform.root.gameObject == Satsuma.Instance.gameObject)
            {
                carMass.Value += objectMass;
            }
        }

        void OnDisable()
        {
            if (gameObject.transform.root.gameObject == Satsuma.Instance.gameObject)
            {
                carMass.Value -= objectMass;
            }
        }
    }
}
