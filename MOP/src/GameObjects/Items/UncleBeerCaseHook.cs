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

using MSCLoader;
using System.Collections;
using UnityEngine;

namespace MOP
{
    class UncleBeerCaseHook : MonoBehaviour
    {
        // This script hooks to unkkel alkohol beer case

        void Start()
        {
            if (GetComponent<PlayMakerFSM>() == null)
                return;

            FsmHook.FsmInject(this.gameObject, "Remove bottle", DestroyBeerBottles);
            FsmHook.FsmInject(this.gameObject, "Remove bottle", HookBottles);  
        }

        /// <summary>
        /// Used when this item is a beer case.
        /// Starts the coroutine that initializes Items.DestroyEmptyBottles after 7 seconds.
        /// </summary>
        void DestroyBeerBottles()
        {
            // If the setting is not enabled, return.
            if (!MopSettings.RemoveEmptyBeerBottles)
                return;

            // If Bottle Recycling mod is present, prevent destroying beer bottles
            if (Rules.instance.SpecialRules.DontDestroyEmptyBeerBottles)
            {
                ModConsole.Print("<color=yellow>[MOP] Beer bottles won't be destroyed, because one or more mods prevent it. " +
                    "Disable 'Destroy empty beer bottles' in the MOP settings.</color>");
                return;
            }

            if (currentBottleDestroyerRoutine != null)
            {
                StopCoroutine(currentBottleDestroyerRoutine);
            }

            currentBottleDestroyerRoutine = BottleDestroyerRoutine();
            StartCoroutine(currentBottleDestroyerRoutine);
        }

        IEnumerator currentBottleDestroyerRoutine;
        IEnumerator BottleDestroyerRoutine()
        {
            yield return new WaitForSeconds(7);
            Items.instance.DestroyBeerBottles();
        }

        /// <summary>
        /// Used when this item is a beer case.
        /// Starts the coroutine that initializes Items.HookEmptyBeerBottles after 7 seconds.
        /// </summary>
        void HookBottles()
        {
            if (currentBottleHooker != null)
            {
                StopCoroutine(currentBottleHooker);
            }

            currentBottleHooker = BottleHooker();
            StartCoroutine(currentBottleHooker);
        }

        IEnumerator currentBottleHooker;
        IEnumerator BottleHooker()
        {
            yield return new WaitForSeconds(7);
            Items.instance.HookEmptyBeerBottles();
        }
    }
}
