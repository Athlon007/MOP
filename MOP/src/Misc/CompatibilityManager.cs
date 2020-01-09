using UnityEngine;
using MSCLoader;

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

        // GAZ 24 Volga
        //
        bool gaz;
        public bool Gaz { get => gaz; }

        // Police Ferndale
        bool policeFerndale;
        public bool PoliceFerndale { get => policeFerndale; }

        // VHS mod
        bool vhsPlayer;
        public bool VhsPlayer { get => vhsPlayer; }

        bool offroadHayosiko;
        public bool OffroadHayosiko { get => offroadHayosiko; }

        public CompatibilityManager()
        {
            instance = this;

            drivableFury = GameObject.Find("FURY(1630kg)") != null;
            if (drivableFury)
            {
                ModConsole.Print("[MOP] Drivable Fury has been found!");
            }

            secondFerndale = GameObject.Find("SECONDFERNDALE(1630kg)") != null;
            if (secondFerndale)
            {
                ModConsole.Print("[MOP] Second Ferndale has been found!");
            }

            gaz = GameObject.Find("GAZ24(1420kg)") != null;
            if (gaz)
            {
                ModConsole.Print("[MOP] GAZ 24 Volga has been found!");
            }

            policeFerndale = GameObject.Find("POLICEFERNDALE(1630kg)") != null;
            if (policeFerndale)
            {
                ModConsole.Print("[MOP] Police Ferndale has been found!");
            }

            vhsPlayer = ModLoader.IsModPresent("VHSPlayer");
            if (vhsPlayer)
            {
                ModConsole.Print("[MOP] VHS Player mod has been found!");
            }

            offroadHayosiko = ModLoader.IsModPresent("OffroadHayosiko");
            if (offroadHayosiko)
            {
                ModConsole.Print("[MOP] Offroad Hayosiko has been found!");
            }

            ModConsole.Print("[MOP] Compatibility Manager done");
        }
    }
}
