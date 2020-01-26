using UnityEngine;

namespace MOP
{
    class UnloadableObject
    {
        // This script manages the objects that aren't meant to be unloaded, because it may result in weird glitches in-game.

        public Transform ObjectTransform;

        /// <summary>
        /// Default object's parent
        /// </summary>
        public Transform Parent;

        public UnloadableObject(Transform objectTransform)
        {
            ObjectTransform = objectTransform;
            Parent = objectTransform.parent;
        }
    }
}
