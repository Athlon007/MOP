using System.Collections.Generic;
using UnityEngine;

namespace MOP
{
    class WorldObjects
    {
        // This script manages the list of WorldObjects.
        // Basically, objects that are static, such as buildings.

        List<WorldObject> worldObject;

        public WorldObjects()
        {
            worldObject = new List<WorldObject>();
        }

        /// <summary>
        /// Finds the game object by gameObjectName. If it finds it, adds it to the list.
        /// </summary>
        /// <param name="gameObjectName">Name of the game object.</param>
        /// <param name="distance">Distance after which the object gets toggled.</param>
        /// <param name="rendererOnly">If true, only game object's renderer will get toggled.</param>
        public void Add(string gameObjectName, int distance = 200, bool rendererOnly = false)
        {
            GameObject gm = GameObject.Find(gameObjectName);

            if (gm == null)
            {
                MSCLoader.ModConsole.Print("[MOP] Couldn't find " + gameObjectName);
                return;
            }

            worldObject.Add(new WorldObject(gm, distance, rendererOnly));
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
            GameObject gm = GameObject.Find(gameObjectName);
            
            if (gm == null)
            {
                MSCLoader.ModConsole.Print("[MOP] Couldn't find " + gameObjectName);
                return;
            }

            worldObject.Add(new WorldObject(gm, awayFromHouse, rendererOnly));
        }

        /// <summary>
        /// Returns the element i from the list.
        /// </summary>
        /// <param name="i">Element number.</param>
        public WorldObject Get(int i)
        {
            return worldObject[i];
        }

        /// <summary>
        /// Returns the length of the list.
        /// </summary>
        public int Count => worldObject.Count;
    }
}
