// Modern Optimization Plugin
// Copyright(C) 2019-2021 Athlon

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

using UnityEngine;
using MOP.Common;

namespace MOP.Items.Cases
{
    class ThrowableJunkBehaviour : MonoBehaviour
    {
        // This script is being added to the all the prefabs of bottles/glasses/jugs,
        // and if the MopSettings.RemoveEmptyBeerBottles is enabled,
        // it destroys the object the behaviour is attached to.

        // Alternatively, it also adds ItemBehaviour.

        void Start()
        {
            if (MopSettings.RemoveEmptyBeerBottles)
            {
                Destroy(gameObject);
                return;
            }

            gameObject.AddComponent<ItemBehaviour>();

            this.enabled = false;
        }
    }
}
