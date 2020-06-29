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
using UnityEngine;

namespace MOP
{
    class CustomPlayMakerFixedUpdate : MonoBehaviour
    {
        // This script replaces the PlayMaker's PlayMakerFixedUpdate.

        readonly PlayMakerFSM[] fsms;
        bool isRunning;

        public CustomPlayMakerFixedUpdate()
        {
            Component hoodFixedUpdate = gameObject.GetComponentByName("PlayMakerFixedUpdate");

            if (hoodFixedUpdate == null)
            {
                MSCLoader.ModConsole.Error($"[MOP] No PlayMakerFixedUpdate component found in {gameObject.name}.");
                return;
            }

            Object.Destroy(hoodFixedUpdate);
            fsms = GetComponents<PlayMakerFSM>();
        }

        void FixedUpdate()
        {
            if (!isRunning) return;

            for (int i = 0; i < fsms.Length; i++)
            {
                if (fsms[i].Active && fsms[i].Fsm.HandleFixedUpdate)
                {
                    fsms[i].Fsm.FixedUpdate();
                }
            }
        }

        public void StartFixedUpdate()
        {
            isRunning = true;
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

    public class MasterAudioAssembleCustom : FsmStateAction
    {
        // This script makes so the assemble sound from the engine block is played only if player is close.

        Transform player;
        Transform thisTransform;
        Transform masterAudioTransform;
        AudioSource masterAudioSource;

        public override void OnEnter()
        {
            if (player == null)
            {
                player = GameObject.Find("PLAYER").transform;
                thisTransform = Fsm.GameObject.transform;
                masterAudioTransform = GameObject.Find("MasterAudio/CarBuilding/assemble").transform;
                masterAudioSource = masterAudioTransform.gameObject.GetComponent<AudioSource>();
            }

            if (Vector3.Distance(player.position, thisTransform.position) < 5)
            {
                masterAudioTransform.position = thisTransform.position;
                masterAudioSource.Play();
            }

            Finish();
        }
    }

    /// <summary>
    /// Replaces the "Disable battery wires" state actions in SATSUMA/Wiring object PlayMaker script called "Status".
    /// The default one for some reasome sometimes decides to permamently disable the battery terminal, breaking the save.
    /// </summary>
    public class CustomBatteryDisable : FsmStateAction
    {
        FsmBool fsmBoolInstalled;
        GameObject batteryTerminalMinus;

        public override void OnEnter()
        {
            if (fsmBoolInstalled == null)
            {
                fsmBoolInstalled = GameObject.Find("Database/DatabaseWiring/WiringBatteryMinus").GetPlayMakerByName("Data").FsmVariables.FindFsmBool("Installed");
                batteryTerminalMinus = GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Wiring/Parts/battery_terminal_minus(xxxxx)").gameObject;
            }

            batteryTerminalMinus.SetActive(fsmBoolInstalled.Value);
            Finish();
        }
    }

    /// <summary>
    /// This custom action trigger save game, if the player picks up the phone when it's the "large Suski" call.
    /// </summary>
    public class CustomSuskiLargeFlip : FsmStateAction
    {
        public override void OnEnter()
        {
            WorldManager.instance.DelayedPreSave();
            Finish();
        }
    }
}
