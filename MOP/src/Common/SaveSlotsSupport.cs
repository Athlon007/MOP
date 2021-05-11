using MOP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MOP.Common
{
    class SaveSlotsSupport : MonoBehaviour
    {
        void OnDisable()
        {
            SaveManager.VerifySave();
        }
    }
}
