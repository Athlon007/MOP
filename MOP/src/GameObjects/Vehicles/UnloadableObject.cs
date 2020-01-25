using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MOP
{
    class UnloadableObject
    {
        public Transform ObjectTransform;
        public Transform Parent;

        public UnloadableObject(Transform objectTransform)
        {
            ObjectTransform = objectTransform;
            Parent = objectTransform.parent;
        }
    }
}
