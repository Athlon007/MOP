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
using UnityEngine;

namespace MOP
{
    class WorldObjectList
    {
        // This script manages the list of WorldObjects.
        // Basically, objects that are static, such as buildings.

        List<WorldObject> worldObjects;
        
        /// <summary>
        /// Returns the length of the list.
        /// </summary>
        public int Count => worldObjects.Count;

        public WorldObjectList()
        {
            worldObjects = new List<WorldObject>();
        }

        /// <summary>
        /// Finds the game object by gameObjectName. If it finds it, adds it to the list.
        /// </summary>
        /// <param name="gameObjectName">Name of the game object.</param>
        /// <param name="distance">Distance after which the object gets toggled.</param>
        /// <param name="rendererOnly">If true, only game object's renderer will get toggled.</param>
        public void Add(string gameObjectName, int distance = 200, bool rendererOnly = false)
        {
            IgnoreRule rule = RuleFiles.instance.IgnoreRules.Find(f => f.ObjectName == gameObjectName);
            if (rule != null)
            {
                if (rule.TotalIgnore)
                    return;

                rendererOnly = true;
            }

            GameObject gm = GameObject.Find(gameObjectName);

            if (gm == null)
            {
                MSCLoader.ModConsole.Print($"[MOP] Couldn't find {gameObjectName}");
                return;
            }

            worldObjects.Add(new WorldObject(gm, distance, rendererOnly));
        }

        /// <summary>
        /// Finds the game object by gameObjectName. If it finds it, adds it to the list. 
        /// In that case, it is being triggered by the distance to the house.
        /// </summary>
        /// <param name="gameObjectName">Name of the game object.</param>
        /// <param name="awayFromHouse">If true, the object will be toggled as soon as player leaves the house area.</param>
        /// <param name="rendererOnly">If true, only game object's renderer will get toggled.</param>
        public void Add(string gameObjectName, bool awayFromHouse, bool rendererOnly = false)
        {
            IgnoreRule rule = RuleFiles.instance.IgnoreRules.Find(f => f.ObjectName == gameObjectName);
            if (rule != null)
            {
                if (rule.TotalIgnore)
                    return;

                rendererOnly = true;
            }

            GameObject gm = GameObject.Find(gameObjectName);
            
            if (gm == null)
            {
                MSCLoader.ModConsole.Print($"[MOP] Couldn't find {gameObjectName}");
                return;
            }

            worldObjects.Add(new WorldObject(gm, awayFromHouse, rendererOnly));
        }

        /// <summary>
        /// Returns the element i from the list.
        /// </summary>
        /// <param name="i">Element number.</param>
        public WorldObject Get(int i)
        {
            return worldObjects[i];
        }
    }
}
