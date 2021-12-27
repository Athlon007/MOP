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

using System.Collections.Generic;

namespace MOP.Helpers
{
    // Stored with the game's save file.
    public class MopSaveFileData
    {
        public string version = "1.0";

        public float rearBumperTightness;
        public List<string> rearBumperBolts;

        public float halfshaft_FLTightness;
        public List<string> halfshaft_FLBolts;

        public float halfshaft_FRTightness;
        public List<string> halfshaft_FRBolts;

        public float wiringBatteryMinusTightness;
        public List<string> wiringBatteryMinusBolts;
    }
}
