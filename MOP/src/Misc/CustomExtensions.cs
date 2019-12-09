using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Returns the distance between point A and B.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float Distance(this Transform player, Transform target)
        {
            //Gets Distance.
            return Vector3.Distance(player.position, target.position);
        }
    }
}
