using System;
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
