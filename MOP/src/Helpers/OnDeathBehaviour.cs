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

namespace MOP.Helpers
{
    class OnDeathBehaviour : MonoBehaviour
    {
        // Executes the Hypervisor.PreSaveGame() void, when the death object gets enabled.
        Action action;
        public void Initialize(Action action) => this.action = action;
        void OnEnable() => action();
    }
}
