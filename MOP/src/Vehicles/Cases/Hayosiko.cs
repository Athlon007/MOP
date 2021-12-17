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

using System;
using System.Collections.Generic;
using UnityEngine;

using MOP.FSM;

namespace MOP.Vehicles.Cases
{
    internal class Hayosiko : Vehicle
    {
        readonly string[] partialDisableItemNames = 
        { 
            "CoG", "FuelTank", "HookFront", "HookRear", "RadioPivot", 
            "Odometer", "LOD", "wheelFL", "wheelFR", "wheelRL", 
            "wheelRR", "StagingWheel", "DriverDoors", "RearDoor", 
            "SideDoor", "body", "GetInPivot", "Colliders", "Starter" 
        };

        readonly GameObject[] partialDisableItems;        

        public Hayosiko(string gameObjectName = "HAYOSIKO(1500kg, 250)") : base(gameObjectName)
        {
            transform.Find("Odometer").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            try
            {
                // Get items for partial disabling.
                List<GameObject> gms = new List<GameObject>();
                foreach (var f in partialDisableItemNames)
                {
                    Transform t = transform.Find(f);
                    if (t)
                    {
                        gms.Add(t.gameObject);
                    }
                }
                partialDisableItems = gms.ToArray();
            }
            catch
            {
                throw new Exception("Couldn't find partial disable items.");
            }

            Toggle = ToggleFull;
        }

        public void ToggleFull(bool enabled)
        {
            if (!FsmManager.PlayerHasHayosikoKey())
            {
                // We are using partial disabling to avoid any issues with MSC scripts relating uncle.
                TogglePartial(enabled);
                return;
            }
            else
            {
                // We are not using partial disabling, if player has permanently obtained Hayosiko, as it is not needed anymore.
                TogglePartial(true);
            }

            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);
            }

            gameObject.SetActive(enabled);

            if (enabled)
            {
                MoveNonDisableableObjects(null);
            }
        }

        public void TogglePartial(bool enabled)
        {
            if (partialDisableItems[0].activeSelf == enabled)
            {
                return;
            }

            for (int i = 0; i < partialDisableItems.Length; i++)
            {
                partialDisableItems[i].SetActive(enabled);
            }
        }
    }
}
