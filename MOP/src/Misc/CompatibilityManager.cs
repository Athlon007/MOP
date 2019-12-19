using UnityEngine;

namespace MOP
{
    class CompatibilityManager
    {
        // This script manages the compatibility between other mods

        public static CompatibilityManager instance;

        // Drivable Fury
        // https://www.racedepartment.com/downloads/drivable-fury.29885/
        bool drivableFury;
        public bool DrivableFury { get => drivableFury; }

        // Second Ferndale
        // https://www.racedepartment.com/downloads/plugin-second-ferndale.28407/
        bool secondFerndale;
        public bool SecondFerndale { get => secondFerndale; }

        public CompatibilityManager()
        {
            instance = this;

            drivableFury = GameObject.Find("FURY(1630kg)") != null;
            if (drivableFury)
            {
                MSCLoader.ModConsole.Print("[MOP] Drivable Fury has been found!");
            }

            secondFerndale = GameObject.Find("SECONDFERNDALE(1630kg)") != null;
            if (secondFerndale)
            {
                MSCLoader.ModConsole.Print("[MOP] Second Ferndale has been found!");
            }
        }
    }
}
