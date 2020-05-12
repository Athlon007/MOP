// Modern Optimization Plugin
// Copyright(C) 2019-2020 Athlon

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

namespace MOP
{
    class IgnoreRule
    {
        public string ObjectName;
        public bool TotalIgnore;

        public IgnoreRule(string ObjectName, bool TotalIgnore)
        {
            this.ObjectName = ObjectName;
            this.TotalIgnore = TotalIgnore;
        }
    }

    class IgnoreRuleAtPlace
    {
        public string Place;
        public string ObjectName;

        public IgnoreRuleAtPlace(string Place, string ObjectName)
        {
            this.Place = Place;
            this.ObjectName = ObjectName;
        }
    }

    class PreventToggleOnObjectRule
    {
        public string MainObject;
        public string ObjectName;

        public PreventToggleOnObjectRule(string MainObject, string ObjectName)
        {
            this.MainObject = MainObject;
            this.ObjectName = ObjectName;
        }
    }

    // This class is intended for special flags used in specific cases.
    class SpecialRules
    {
        public bool SatsumaIgnoreRenderers;
        public bool DontDestroyEmptyBeerBottles;
        
        // Custom rule files.
        public bool IgnoreModVehicles;
        public bool ToggleAllVehiclesPhysicsOnly;
        public bool DrivewaySector;
        public bool ExperimentalSatsumaTrunk;
        public bool ExperimentalOptimization;
    }
}