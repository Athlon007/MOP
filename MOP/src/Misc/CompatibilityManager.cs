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
        // https://www.racedepartment.com/downloads/plugin-gaz-24-volga.28653/
        bool gaz;
        public bool Gaz { get => gaz; }

        // Police Ferndale
        // https://www.racedepartment.com/downloads/police-ferndale.30338/
        bool policeFerndale;
        public bool PoliceFerndale { get => policeFerndale; }

        // VHS mod
        bool vhsPlayer;
        public bool VhsPlayer { get => vhsPlayer; }

        // Offroad Hayosiko
        bool offroadHayosiko;
        public bool OffroadHayosiko { get => offroadHayosiko; }

        // JetSky mod
        bool jetSky;
        public bool JetSky { get => jetSky; }

        // Moonshine still mod
        bool moonshinestill;
        public bool Moonshinestill { get => moonshinestill; }

        public CompatibilityManager()
        {
            instance = this;

            drivableFury = ModLoader.IsModPresent("FURY");
            if (drivableFury)
            {
                ModConsole.Print("[MOP] Drivable Fury has been found!");
            }

            secondFerndale = ModLoader.IsModPresent("SecondFerndale");
            if (secondFerndale)
            {
                ModConsole.Print("[MOP] Second Ferndale has been found!");
            }

            gaz = ModLoader.IsModPresent("GAZ24");
            if (gaz)
            {
                ModConsole.Print("[MOP] GAZ 24 Volga has been found!");
            }

            policeFerndale = ModLoader.IsModPresent("Police_Ferndale");
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

            jetSky = ModLoader.IsModPresent("JetSky");
            if (jetSky)
            {
                ModConsole.Print("[MOP] JetSky has been found!");
            }

            moonshinestill = ModLoader.IsModPresent("MSCStill");
            if (moonshinestill)
            {
                ModConsole.Print("[MOP] Moonshine Still has been found!");
            }

            ModConsole.Print("[MOP] Compatibility Manager done");
        }

        static bool IsModPresent(string modID)
        {
            bool isModPresent = ModLoader.IsModPresent(modID);
            if (isModPresent)
                foreach (var mod in ModLoader.LoadedMods)
                    if (mod.ID == modID)
                        ModConsole.Print($"[MOP] {mod.Name} has been found!");

            return isModPresent;
        }
    }
}
