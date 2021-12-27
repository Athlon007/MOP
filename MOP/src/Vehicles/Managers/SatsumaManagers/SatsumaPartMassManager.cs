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
using HutongGames.PlayMaker;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOP.FSM;
using MOP.Common;
using MOP.Vehicles.Cases;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaPartMassManager : MonoBehaviour
    {
        float objectMass;
        FsmFloat carMass;

        bool isClean;

        void Awake()
        {
            if (transform.root != transform && !transform.root.gameObject.name.EqualsAny("SATSUMA(557kg, 248)", "CARPARTS"))
            {
                Destroy(this);
            }
        }

        void OnEnable()
        {
            if (!isClean)
            {
                isClean = true;

                try
                {
                    PlayMakerFSM removalFSM = gameObject.GetPlayMaker("Removal");

                    if (!removalFSM)
                    {
                        throw new Exception($"SatsumaPartMassManager: {gameObject.name} - Removal PlayMakerFSM is missing!\n\n" +
                                                   $"Path: {gameObject.Path()}");
                    }

                    objectMass = removalFSM.FsmVariables.GetFsmFloat("Mass").Value;
                    carMass = PlayMakerGlobals.Instance.Variables.FindFsmFloat("CarMass");
                    if (this.gameObject.name.ContainsAny("strut", "antenna", "marker light"))
                    {
                        FsmState disableTrigger = removalFSM.GetState("Disable trigger");
                        if (disableTrigger == null)
                        {
                            throw new Exception($"SatsumaPartMassManager: {gameObject.name} - disableTrigger is null or empty.");
                        }

                        List<FsmStateAction> actionList = disableTrigger.Actions.ToList();
                        if (actionList == null)
                        {
                            throw new Exception($"SatsumaPartMassManager: {gameObject.name} - actionList is null or empty.");
                        }
                        actionList.RemoveAt(0);
                        disableTrigger.Actions = actionList.ToArray();

                        FsmState removePart = removalFSM.GetState("Remove part");
                        if (removePart == null)
                        {
                            throw new Exception($"SatsumaPartMassManager: {gameObject.name} - removePart is null or empty.");
                        }
                        List<FsmStateAction> rpActionList = removePart.Actions.ToList();
                        if (rpActionList == null)
                        {
                            throw new Exception($"SatsumaPartMassManager: {gameObject.name} - rpActionList is null or empty.");
                        }
                        rpActionList.RemoveAt(9);
                        removePart.Actions = rpActionList.ToArray();
                    }
                    else
                    {
                        FsmState disableTrigger = removalFSM.GetState("Add mass");
                        if (disableTrigger == null)
                        {
                            throw new Exception($"SatsumaPartMassManager: {gameObject.name} - disableTrigger is null or empty.");
                        }
                        List<FsmStateAction> actionList = disableTrigger.Actions.ToList();
                        if (actionList == null)
                        {
                            throw new Exception($"SatsumaPartMassManager: {gameObject.name} - actionList is null or empty.");
                        }
                        actionList.RemoveAt(0);
                        disableTrigger.Actions = actionList.ToArray();

                        FsmState removePart = removalFSM.GetState("Remove part");
                        if (removePart == null)
                        {
                            throw new Exception($"SatsumaPartMassManager: {gameObject.name} - removePart(2) is null or empty.");
                        }
                        List<FsmStateAction> rpActionList = removePart.Actions.ToList();
                        if (rpActionList == null)
                        {
                            throw new Exception($"SatsumaPartMassManager: {gameObject.name} - rpActionList(2) is null or empty.");
                        }
                        rpActionList.RemoveAt(7);
                        removePart.Actions = rpActionList.ToArray();
                    }

                    if (this.gameObject.activeSelf)
                    {
                        carMass.Value -= objectMass;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, true, "There was an error with SatsumaPartMassManager: " + gameObject.Path());
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
