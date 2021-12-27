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

using UnityEngine;

using MOP.Common.Enumerations;

namespace MOP.WorldObjects
{
    abstract class GenericObject
    {
        protected GameObject gameObject;
        public GameObject GameObject => gameObject;

        readonly int distance;
        public int Distance => distance;

        public Transform transform => gameObject.transform;

        readonly DisableOn disableOn;
        public DisableOn DisableOn => disableOn;

        public GenericObject(GameObject gameObject, int distance = 200, DisableOn disableOn = DisableOn.Distance)
        {
            this.gameObject = gameObject;
            this.distance = distance;
            this.disableOn = disableOn;
        }

        public abstract void Toggle(bool enabled);

        public string GetName()
        {
            return gameObject.name;
        }
    }
}
