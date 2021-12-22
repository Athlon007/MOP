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
using UnityEngine;
using MOP.FSM;

namespace MOP.FSM.Actions
{
    /// <summary>
    /// Replaces the "Disable battery wires" state actions in SATSUMA/Wiring object PlayMaker script called "Status".
    /// The default one for some reasome sometimes decides to permamently disable the battery terminal, breaking the save.
    /// </summary>
    public class CustomBatteryDisable : FsmStateAction
    {
        FsmBool fsmBoolInstalled;
        GameObject batteryTerminalMinus;

        public CustomBatteryDisable()
        {
            fsmBoolInstalled = GameObject.Find("Database/DatabaseWiring/WiringBatteryMinus").GetPlayMaker("Data").FsmVariables.FindFsmBool("Installed");
            batteryTerminalMinus = GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Wiring/Parts/battery_terminal_minus(xxxxx)").gameObject;
        }

        public override void OnEnter()
        { 
            batteryTerminalMinus.SetActive(fsmBoolInstalled.Value);
            Finish();
        }
    }
}
