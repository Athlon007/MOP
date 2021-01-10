using UnityEngine;
using System;

using MOP.Common.Enumerations;

namespace MOP.WorldObjects
{
    class GenericObject
    {
        protected GameObject gameObject;
        public GameObject GameObject => gameObject;

        int distance;
        public int Distance => distance;

        public Transform transform => gameObject.transform;

        DisableOn disableOn;
        public DisableOn DisableOn => disableOn;

        public GenericObject(GameObject gameObject, int distance = 200, DisableOn disableOn = DisableOn.Distance)
        {
            this.gameObject = gameObject;
            this.distance = distance;
            this.disableOn = disableOn;
        }

        public virtual void Toggle(bool enabled)
        {
            throw new NotImplementedException("Not implemented toggle method.");
        }

        public string GetName()
        {
            return gameObject.name;
        }
    }
}
