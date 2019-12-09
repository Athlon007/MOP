using UnityEngine;

namespace MOP
{
    class AdoptableObject
    {
        // AdoptableObjects
        //
        // Objects that are needed to be adopted by other GameObjects, when the main object is disabled, in ordrer to prevent bugs.

        public Transform Object;
        public Transform Parent;

        public AdoptableObject(Transform Object)
        {
            this.Object = Object;
            this.Parent = Object.transform.parent;
        }
    }
}
