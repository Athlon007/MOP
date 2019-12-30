using System.Collections.Generic;
using UnityEngine;

namespace MOP
{
    class WorldObjects
    {
        List<WorldObject> worldObject;

        public WorldObjects()
        {
            worldObject = new List<WorldObject>();
        }

        public void Add(string gameObjectName, int distance = 200, bool rendererOnly = false)
        {
            if (GameObject.Find(gameObjectName) == null)
            {
                MSCLoader.ModConsole.Print("[MOP] Couldn't find " + gameObjectName + ".");
                return;
            }

            worldObject.Add(new WorldObject(gameObjectName, distance, rendererOnly));
        }

        public void Add(string gameObjectName, bool awayFromHouse, bool rendererOnly = false)
        {
            if (GameObject.Find(gameObjectName) == null)
            {
                MSCLoader.ModConsole.Print("[MOP] Couldn't find " + gameObjectName + ".");
                return;
            }

            worldObject.Add(new WorldObject(gameObjectName, awayFromHouse, rendererOnly));
        }

        public WorldObject Get(int i)
        {
            return worldObject[i];
        }

        public int Count => worldObject.Count;
    }
}
