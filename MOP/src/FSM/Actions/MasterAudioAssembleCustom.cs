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
using UnityEngine;

namespace MOP.FSM.Actions
{
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
}
