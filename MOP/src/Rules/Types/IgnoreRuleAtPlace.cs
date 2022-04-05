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
    class IgnoreRuleAtPlace : Rule
    {
        public string Place;
        public string ObjectName;

        public IgnoreRuleAtPlace(Mod mod, string filename, string Place, string ObjectName) : base(mod, filename)
        {
            this.Place = Place;
            this.ObjectName = ObjectName;
        }
    }
}
