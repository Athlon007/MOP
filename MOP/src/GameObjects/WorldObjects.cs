using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (GameObject.Find(gameObjectName))
            {
                worldObject.Add(new WorldObject(gameObjectName, distance, rendererOnly));
            }
            else
            {
                MSCLoader.ModConsole.Print("[MOP] Couldn't find " + gameObjectName + ".");
            }
        }

        public void Add(string gameObjectName, bool awayFromHouse, bool rendererOnly = false)
        {
            if (GameObject.Find(gameObjectName))
            {
                worldObject.Add(new WorldObject(gameObjectName, awayFromHouse, rendererOnly));
            }
            else
            {
                MSCLoader.ModConsole.Print("[MOP] Couldn't find " + gameObjectName + ".");
            }
        }

        public WorldObject Get(int i)
        {
            return worldObject[i];
        }
    }
}
