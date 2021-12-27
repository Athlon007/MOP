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

using UnityEngine;

namespace MOP.Rules.Types
{
    class NewSector
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public string[] Whitelist;

        public NewSector(Vector3 Position, Vector3 Scale, Vector3 Rotation, string[] Whitelist)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Rotation = Rotation;
            this.Whitelist = Whitelist;
        }
    }
}
