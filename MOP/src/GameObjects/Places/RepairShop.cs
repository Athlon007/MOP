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
        // NOTE: That script DOES NOT disable the store itself, rather some of its childrens.

        // Objects from that blaclklist will not be disabled
        // It is so to prevent from restock script and Teimo's bike routine not working
        string[] blackList = { "REPAIRSHOP", "JunkCar", "sats_burn_masse", "TireOld(Clone)", "Order", "JunkYardJob",
                                "BoozeJob", "Spawn", "SatsumaSpawns", "SeatPivot", "DistanceTarget", "SpawnToRepair",
                                "PartsDistanceTarget", "JunkCar4", "JunkCarSpawns", "Parts", "wheel_regul", "rpm gauge(Clone)",
                                "Hook", "Jobs", "GearRatios", "Fix", "fix", "Job", "Polish", "Wheel", "Fill", "Rollcage",
                                "Adjust", "GearLinkage", "Paintjob", "Windshield", "ToeAdjust", "Brakes", "Lifter", "Audio", "roll",
                                "TireCatcher", "Ropes", "Note", "note" };

        /// <summary>
        /// Initialize the RepairShop class
        /// </summary>
        public RepairShop() : base("REPAIRSHOP")
        {
            GameObjectBlackList = blackList;
            DisableableChilds = GetDisableableChilds();
        }
    }
}
