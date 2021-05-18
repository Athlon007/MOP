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
using System.Linq;
using UnityEngine;

namespace MOP.Common
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

        /// <summary>
        /// Checks if string contains any of the values provided in lookFor.
        /// </summary>
        /// <param name="lookIn">String in which we should look for</param>
        /// <param name="lookFor">Values that we want to look for in lookIn</param>
        /// <returns></returns>
        public static bool ContainsAny(this string[] lookIn, params string[] lookFor)
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

        readonly static string[] rainbow = new string[] { "red", "orange", "yellow", "green", "blue", "purple" };
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

        public static Transform FindRecursive(this Transform obj, string name)
        {
            foreach (Transform g in obj.GetComponentsInChildren<Transform>())
            {
                if (g.name == name)
                    return g;
            }

            return null;
        }

        /// <summary>
        /// A FX 3.5 way to mimic the FX4 "HasFlag" method.
        /// </summary>
        /// <param name="variable">The tested enum.</param>
        /// <param name="value">The value to test.</param>
        /// <returns>True if the flag is set. Otherwise false.</returns>
        public static bool HasFlag(this Enum variable, Enum value)
        {
            // check if from the same type.
            if (variable.GetType() != value.GetType())
            {
                throw new ArgumentException("The checked flag is not from the same type as the checked variable.");
            }

            Convert.ToUInt64(value);
            ulong num = Convert.ToUInt64(value);
            ulong num2 = Convert.ToUInt64(variable);

            return (num2 & num) == num;
        }

        /// <summary>
        /// Returns the path of the game object.
        /// </summary>
        public static string Path(this Transform transform)
        {
            if (transform == null)
            {
                return "null";
            }

            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        /// <summary>
        /// Returns the path of the game object.
        /// </summary>
        public static string Path(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                return "null";
            }

            return Path(gameObject.transform);
        }
    }
}
