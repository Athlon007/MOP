using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MOP.Helpers
{
    internal class MopPauseMenuHandler : MonoBehaviour
    {
        void OnEnable()
        {
            GC.Collect();
        }

        void OnDisable()
        {
            GC.Collect();
        }
    }
}
