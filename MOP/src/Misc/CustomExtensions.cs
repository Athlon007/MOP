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

using System.Collections.Generic;

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
    }
}
