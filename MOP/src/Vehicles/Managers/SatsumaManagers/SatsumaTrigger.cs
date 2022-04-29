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

using System;
using UnityEngine;

using MOP.Common;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaTrigger : MonoBehaviour
    {
        Transform pivot;
        public Transform Pivot { get => pivot; }

        void Awake()
        {
            try
            {
                PlayMakerFSM fsm = GetComponent<PlayMakerFSM>();
                if (fsm == null)
                {
                    Destroy(this);
                    return;
                }

                GameObject gameObjectPivot = fsm.FsmVariables.GetFsmGameObject("Parent").Value;

                if (gameObjectPivot == null)
                {
                    Destroy(this);
                    return;
                }

                pivot = gameObjectPivot.transform;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, gameObject.Path());
            }
        }

        void OnDisable()
        {
            if (pivot != null)
            {
                SatsumaTriggerFixer.Instance.Check(this);
            }
        }
    }
}
