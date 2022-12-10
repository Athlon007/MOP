// Modern Optimization Plugin
// Copyright(C) 2019-2022 Athlon

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
using System.Linq;
using System;

using MOP.Rules;
using MOP.Rules.Types;
using MOP.Common.Enumerations;
using MOP.WorldObjects;
using MOP.Common.Interfaces;

namespace MOP.Managers
{
    class WorldObjectManager : IManager<GenericObject>
    {
        // This script manages the list of WorldObjects.
        // Basically, objects that are static, such as buildings.

        static WorldObjectManager instance;
        public static WorldObjectManager Instance { get => instance; }
        
        public GenericObject this[int index] => worldObjects[index];

        readonly List<GenericObject> worldObjects;

        /// <summary>
        /// Returns the length of the list.
        /// </summary>
        public int Count => worldObjects.Count;

        public WorldObjectManager()
        {
            instance = this;
            worldObjects = new List<GenericObject>();
        }

        /// <summary>
        /// Finds the game object by gameObjectName. If it finds it, adds it to the list.
        /// </summary>
        /// <param name="gameObjectName">Name of the game object.</param>
        /// <param name="distance">Distance after which the object gets toggled.</param>
        /// <param name="rendererOnly">If true, only game object's renderer will get toggled.</param>
        /// <param name="silent">Skips the error message.</param>
        public GenericObject Add(string gameObjectName, DisableOn disableOn, int distance = 200, ToggleModes toggleMode = ToggleModes.Simple, bool silent = false)
        {
            IgnoreRule rule = RulesManager.Instance.GetList<IgnoreRule>().Find(f => f.ObjectName == gameObjectName);
            if (rule != null)
            {
                if (rule.TotalIgnore)
                    return null;

                toggleMode = ToggleModes.Renderer;
            }

            GameObject gm = GameObject.Find(gameObjectName);

            if (gm == null)
            {
                gm = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == gameObjectName);
            }

            if (!gm)
            {
                if (!silent)
                {
                    throw new Exception($"WorldObjectManager: Couldn't find gameObjectName: \"{gameObjectName}\".");
                }

                return null;
            }

            return Add(gm, disableOn, distance, toggleMode);
        }

        /// <summary>
        /// Adds the gameObject to the list.
        /// </summary>
        /// <param name="gameObject">Game object that will be toggled.</param>
        /// <param name="distance">Distance after which the object gets toggled.</param>
        /// <param name="rendererOnly">If true, only game object's renderer will get toggled.</param>
        public GenericObject Add(GameObject gameObject, DisableOn disableOn, int distance = 200, ToggleModes toggleMode = ToggleModes.Simple)
        {
            if (!gameObject)
            {
                throw new NullReferenceException($"WorldObjectManager: gameObject is null.");
            }

            IgnoreRule rule = RulesManager.Instance.GetList<IgnoreRule>().Find(f => f.ObjectName == gameObject.name);
            if (rule != null)
            {
                if (rule.TotalIgnore)
                    return null;

                toggleMode = ToggleModes.Renderer;
            }

            GenericObject obj;

            switch (toggleMode)
            {
                default:
                    throw new NotImplementedException($"Toggle mode: {toggleMode} is not supported by WorldObjectManager!");
                case ToggleModes.Simple:
                    obj = new SimpleObjectToggle(gameObject, disableOn, distance);
                    break;
                case ToggleModes.Renderer:
                    obj = new RendererToggle(gameObject, disableOn, distance);
                    break;
                case ToggleModes.MultipleRenderers:
                    obj = new MultipleRenderersToggle(gameObject, disableOn, distance);
                    break;
            }

            return Add(obj);
        }

        public GenericObject Add(GenericObject generic)
        {
            worldObjects.Add(generic);
            return generic;
        }

        public List<GenericObject> GetAll => worldObjects;

        public void Remove(GenericObject worldObject)
        {
            worldObjects.Remove(worldObject);
        }

        public GameObject Get(string name)
        {
            return worldObjects.FirstOrDefault(g => g.GetName() == name)?.GameObject;
        }

        public void RemoveAt(int index)
        {
            worldObjects.RemoveAt(index);
        }

        public int EnabledCount
        {
            get
            {
                int enabled = 0;
                foreach (GenericObject obj in GetAll)
                {
                    if (obj.GameObject.activeSelf)
                    {
                        enabled++;
                    }
                }
                return enabled;
            }
        }
    }
}
