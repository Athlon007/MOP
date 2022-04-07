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

using MSCLoader;

namespace MOP.Rules.Types
{
    abstract class Rule
    {
        public Mod Mod { get; private set; }
        public string Filename { get; private set; }

        public Rule(Mod mod, string filename)
        {
            Mod = mod;
            Filename = filename;
        }

        public override string ToString()
        {
#if PRO
            return $"{Filename}" + (!Mod.Enabled ? " (Disabled)" : "");
#else
            return $"{Filename}" + (Mod.isDisabled ? " (Disabled)" : "");
#endif
        }
    }
}
