using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOP
{
    class OcclusionBase : MonoBehaviour
    {
        public bool IsVisible { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
