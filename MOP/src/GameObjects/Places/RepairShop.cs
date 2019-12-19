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

        string[] blackList = { "REPAIRSHOP", "JunkCar", "sats_burn_masse", "TireOld(Clone)", "Order", "JunkYardJob",
                                "BoozeJob", "Spawn", "SatsumaSpawns", "SeatPivot", "DistanceTarget", "SpawnToRepair",
                                "PartsDistanceTarget", "JunkCar4", "JunkCarSpawns", "Parts", "wheel_regul", "rpm gauge(Clone)",
                                "Hook", "Jobs", "GearRatios", "Fix", "fix", "Job", "Polish", "Wheel", "Fill", "Rollcage",
                                "Adjust", "GearLinkage", "Paintjob", "Windshield", "ToeAdjust", "Brakes", "Lifter", "Audio", "roll",
                                "TireCatcher", "Ropes", "Note", "note", "inspection_desk 1", "LOD", "Office", "Furniture",
                                "Building", "office_floor", "coll", "wall_base" };

        /// <summary>
        /// Initialize the RepairShop class
        /// </summary>
        public RepairShop() : base("REPAIRSHOP")
        {
            List<string> blackListList = new List<string>();
            blackListList.AddRange(blackList);

            // Compatibility fix for Fury Mod
            if (CompatibilityManager.instance.DrivableFury)
            {
                blackListList.Add("Vehicle");
            }

            GameObjectBlackList = blackListList.ToArray();
            DisableableChilds = GetDisableableChilds();
        }
    }
}
