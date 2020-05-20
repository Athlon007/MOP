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

using System.Collections.Generic;
using UnityEngine;

namespace MOP
{
    class RepairShop : Place
    {
        // RepairShop Class - made by Konrad "Athlon" Figura
        //
        // Extends Place.cs
        //
        // It is responsible for loading and unloading parts of the repair shop, that are safe to be unloaded or loaded again.
        // It gives some performance bennefit, while still letting the shop and Teimo routines running without any issues.
        // Some objects on the WhiteList can be removed, but that needs testing.
        //
        // NOTE: That script DOES NOT disable the repairshop itself, rather some of its childrens.

        readonly string[] blackList = {
            "REPAIRSHOP", "JunkCar", "sats_burn_masse", "TireOld(Clone)", "Order", "JunkYardJob",
            "BoozeJob", "Spawn", "SatsumaSpawns", "SeatPivot", "DistanceTarget", "SpawnToRepair",
            "PartsDistanceTarget", "JunkCarSpawns", "Parts", "wheel_regul", "rpm gauge(Clone)",
            "Hook", "Jobs", "GearRatios", "Fix", "fix", "Job", "Polish", "Wheel", "Fill", "Rollcage",
            "Adjust", "GearLinkage", "Paintjob", "Windshield", "ToeAdjust", "Brakes", "Lifter", "Audio", "roll",
            "TireCatcher", "Ropes", "Note", "note", "inspection_desk 1", "Office", "Furniture",
            "Building", "office_floor", "coll", "wall_base", "JunkYardJob", "PayMoney", "100mk", "GaugeMeshTach",
            "gauge_glass_fbx", "Pivot", "needle", "Bolt", "bolt", "grille", "wheel_steel5", "gear_stick",
            "Platform", "Coll", "Buy", "Product", "Key(Clone)" };

        /// <summary>
        /// Initialize the RepairShop class
        /// </summary>
        public RepairShop() : base("REPAIRSHOP", 250)
        {
            for (int i = 1; transform.Find($"JunkCar{i}") != null; i++)
            {
                Transform junk = transform.Find($"JunkCar{i}");
                if (junk != null)
                    junk.parent = null;
            }

            GameObjectBlackList.AddRange(blackList);
            DisableableChilds = GetDisableableChilds();

            // Fix for Satsuma parts on shelves
            List<Transform> productsMesh = DisableableChilds.FindAll(t => t.name == "mesh" && t.parent.name.Contains("Product"));
            foreach (Transform product in productsMesh)
                DisableableChilds.Remove(product);
        }
    }
}
