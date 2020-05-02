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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOP
{
    static class CustomExtensions
    {
        /// <summary>
        /// Checks if string contains any of the values provided in lookFor.
        /// </summary>
        /// <param name="lookIn">String in which we should look for</param>
        /// <param name="lookFor">Values that we want to look for in lookIn</param>
        /// <returns></returns>
        public static bool ContainsAny(this string lookIn, params string[] lookFor)
        {
            for (int i = 0; i < lookFor.Length; i++)
            {
                // Value found? Return true.
                if (lookIn.Contains(lookFor[i]))
                {
                    return true;
                }
            }

            // Nothing has been found? Return false.
            return false;
        }

        /// <summary>
        /// Checks if string contains any of the values provided in lookFor.
        /// </summary>
        /// <param name="lookIn">String in which we should look for</param>
        /// <param name="lookFor">Array that we want to look for in lookIn</param>
        /// <returns></returns>
        public static bool ContainsAny(this string lookIn, List<string> lookFor)
        {
            for (int i = 0; i < lookFor.Count; i++)
            {
                // Value found? Return true.
                if (lookIn.Contains(lookFor[i]))
                {
                    return true;
                }
            }

            // Nothing has been found? Return false.
            return false;
        }

        public static bool EqualsAny(this string lookIn, params string[] lookFor)
        {
            for (int i = 0; i < lookFor.Length; i++)
            {
                // Value found? Return true.
                if (lookIn == lookFor[i])
                {
                    return true;
                }
            }

            // Nothing has been found? Return false.
            return false;
        }

        static string[] rainbow = new string[] { "red", "orange", "yellow", "green", "blue", "purple" };
        public static string Rainbowmize(this string input)
        {
            char[] inputArray = input.ToCharArray();
            string output = "";
            int colorNumber = 0;
            for (int i = 0; i < inputArray.Length; i++)
            {
                if (colorNumber >= rainbow.Length)
                    colorNumber = 0;

                string color = rainbow[colorNumber];
                output += $"<color={color}>{inputArray[i]}</color>";
                colorNumber++;
            }

            return output;
        }

        /// <summary>
        /// Returns PlayMaker script by it's name.
        /// </summary>
        public static PlayMakerFSM GetPlayMakerByName(this GameObject gm, string name)
        {
            try
            {
                return gm.GetComponents<PlayMakerFSM>().First(f => f.FsmName == name);
            }
            catch 
            {
                MSCLoader.ModConsole.Error($"[MOP] No PlayMakerFSM {name} for {gm.transform.parent.gameObject.name}/{gm.name} found!");
                return null;
            }
        }

        /// <summary>
        /// Returns the FsmState by it's name.
        /// </summary>
        public static FsmState FindFsmState(this PlayMakerFSM fsm, string name)
        {
            try
            {
                return fsm.FsmStates.First(state => state.Name == name);
            }
            catch
            {
                //MSCLoader.ModConsole.Error($"[MOP] No FsmState {name} in {fsm.gameObject.name} found!");
                return null;
            }
        }
        
        /// <summary>
        /// Returns true, if the PlayMaker script contains the state of provided name.
        /// </summary>
        public static bool ContainsPlayMakerByName(this GameObject gm, string name)
        {
            try
            {
                return gm.GetComponents<PlayMakerFSM>().First(f => f.FsmName == name) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Looks through the components of game object and returns the one matching it's name.
        /// </summary>
        public static Component GetComponentByName(this GameObject gm, string name)
        {
            var list = gm.GetComponents(typeof(Component));
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].GetType().Name == name)
                    return list[i];
            }

            return null;
        }
    }
}
