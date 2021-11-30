﻿// Modern Optimization Plugin
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

using UnityEngine.Events;

namespace MOP.Helpers
{
    struct SaveBugs
    {
        public string BugName;
        public string Description;
        public UnityAction Fix;

        public static SaveBugs New(string bugName, string description, UnityAction fix)
        {
            SaveBugs bug = new SaveBugs
            {
                BugName = bugName,
                Description = description,
                Fix = fix
            };
            return bug;
        }
    }
}