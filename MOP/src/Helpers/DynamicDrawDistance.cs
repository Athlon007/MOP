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

using UnityEngine;
using HutongGames.PlayMaker;

using MOP.Common;
using MOP.FSM;
using System.Collections;

namespace MOP.Helpers
{
    class DynamicDrawDistance : MonoBehaviour
    {
        Camera mainCamera;
        Transform player;

        FsmString typer;

        void Start()
        {
            mainCamera = Camera.main;
            player = GameObject.Find("PLAYER").transform;

            typer = GameObject.Find("COMPUTER").transform.Find("SYSTEM/POS/Command").GetPlayMakerByName("Typer").FsmVariables.GetFsmString("Old");
        }

        void Update()
        {
            if (FsmManager.PlayerComputer && Input.GetKeyDown(KeyCode.Return) && typer != null)
            {
                StartCoroutine(Computer());
            }

            if (!MopSettings.DynamicDrawDistance) return;

            float toGoDrawDistance = FsmManager.GetDrawDistance();
            if (player.position.y > 20)
                toGoDrawDistance *= 2;
            else if (Hypervisor.Instance.IsInSector())
                toGoDrawDistance /= 2;

            mainCamera.farClipPlane = Mathf.Lerp(mainCamera.farClipPlane, toGoDrawDistance, Time.deltaTime * .5f);
        }

        IEnumerator Computer()
        {
            yield return null;
            if (string.IsNullOrEmpty(typer.Value)) yield break;
            string[] arr = typer.Value.Split('\n');
                MSCLoader.ModConsole.Print(arr[arr.Length - 3]);
           
                MSCLoader.ModConsole.Print(arr[arr.Length - 2]);
            if (arr[arr.Length - 3].Contains("est rulz"))
            {
                arr[arr.Length - 2] = "    |\\__/|\n" +
                                      "   /     \\\n" +
                                      "  /_.~ ~,_\\\n" +
                                      "     \\@/ \n";

                typer.Value = string.Join("\n", arr);
            }
        }
    }
}