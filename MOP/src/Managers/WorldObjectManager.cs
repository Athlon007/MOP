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

using System.Collections.Generic;
using UnityEngine;

using MOP.Rules;
using MOP.Rules.Types;
using MOP.Common.Enumerations;

namespace MOP.Managers
{
    class WorldObjectManager
    {
        static WorldObjectManager instance;
        public static WorldObjectManager Instance { get => instance; }

        // This script manages the list of WorldObjects.
        // Basically, objects that are static, such as buildings.

        public WorldObject this[int index] => worldObjects[index];

        readonly List<WorldObject> worldObjects;

        /// <summary>
        /// Returns the length of the list.
        /// </summary>
        public int Count => worldObjects.Count;

        public WorldObjectManager()
        {
            instance = this;
            worldObjects = new List<WorldObject>();
        }

        /// <summary>
        /// Finds the game object by gameObjectName. If it finds it, adds it to the list.
        /// </summary>
        /// <param name="gameObjectName">Name of the game object.</param>
        /// <param name="distance">Distance after which the object gets toggled.</param>
        /// <param name="rendererOnly">If true, only game object's renderer will get toggled.</param>
        /// <param name="silent">Skips </param>
        public void Add(string gameObjectName, DisableOn disableOn, int distance = 200, bool rendererOnly = false, bool silent = false)
        {
            GameObject gm = GameObject.Find(gameObjectName);
            if (gm == null)
            {
                if (!silent)
                {
                    throw new System.Exception($"WorldObjectManager: Couldn't find {gameObjectName}.");
                }
            }

            Add(gm, disableOn, distance, rendererOnly);
        }

        /// <summary>
        /// Finds the game object by gameObjectName. If it finds it, adds it to the list.
        /// </summary>
        /// <param name="gameObject">Game object that will be toggled.</param>
        /// <param name="distance">Distance after which the object gets toggled.</param>
        /// <param name="rendererOnly">If true, only game object's renderer will get toggled.</param>
        public void Add(GameObject gameObject, DisableOn disableOn, int distance = 200, bool rendererOnly = false)
        {
            IgnoreRule rule = RulesManager.Instance.IgnoreRules.Find(f => f.ObjectName == gameObject.name);
            if (rule != null)
            {
                if (rule.TotalIgnore)
                    return;

                rendererOnly = true;
            }

            worldObjects.Add(new WorldObject(gameObject, disableOn, distance, rendererOnly));
        }

        public List<WorldObject> GetList()
        {
            return worldObjects;
        }

        public void Remove(WorldObject worldObject)
        {
            worldObjects.Remove(worldObject);
        }
    }
}
