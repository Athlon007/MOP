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
    class ChangeParentRule : Rule
    {
        public string ObjectName { get; private set; }
        public string NewParentName { get; private set; }

        public ChangeParentRule(Mod mod, string filename, string objectName, string newParentName) : base(mod, filename)
        {
            ObjectName = objectName;
            NewParentName = newParentName;
        }

        public override string ToString()
        {
            return base.ToString() + $" ObjectName: {ObjectName} NewParentName: {NewParentName}";
        }
    }
}
