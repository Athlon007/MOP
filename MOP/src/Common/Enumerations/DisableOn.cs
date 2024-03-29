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

using System;

namespace MOP.Common.Enumerations
{
    [Flags]
    public enum DisableOn
    {
        Distance = 0,
        PlayerInHome = 1,
        PlayerAwayFromHome = 2,
        IgnoreInQualityMode = 4,
        AlwaysUse1xDistance = 8,
        DoNotEnableWhenLeavingHome = 16,
        IgnoreInBalancedAndAbove = 32
    }
}
