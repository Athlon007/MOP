﻿// Modern Optimization Plugin
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

using MOP.Common.Enumerations;
using MSCLoader;

namespace MOP.Rules.Types
{
    class ToggleRule : Rule
    {
        public string ObjectName;
        public ToggleModes ToggleMode;

        public ToggleRule(Mod mod, string filename, string ObjectName, ToggleModes ToggleMode) : base(mod, filename)
        {
            this.ObjectName = ObjectName;
            this.ToggleMode = ToggleMode;
        }
        
        public override string ToString()
        {
            return base.ToString() + $" Object: {ObjectName} Mode: {ToggleMode}";
        }
    }
}
