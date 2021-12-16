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
