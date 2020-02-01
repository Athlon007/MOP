using MSCLoader;
using UnityEngine;

namespace MOP
{
    class CompatibilityManager
    {
        // This script manages the compatibility between other mods

        public static CompatibilityManager instance;

        // Drivable Fury
        // https://www.racedepartment.com/downloads/drivable-fury.29885/
        public bool DrivableFury { get; private set; }

        // Second Ferndale
        // https://www.racedepartment.com/downloads/plugin-second-ferndale.28407/
        public bool SecondFerndale { get; private set; }

        // GAZ 24 Volga
        // https://www.racedepartment.com/downloads/plugin-gaz-24-volga.28653/
        public bool Gaz { get; private set; }

        // Police Ferndale
        // https://www.racedepartment.com/downloads/police-ferndale.30338/
        public bool PoliceFerndale { get; private set; }

        // VHS mod
        public bool VhsPlayer { get; private set; }

        // Offroad Hayosiko
        public bool OffroadHayosiko { get; private set; }

        // JetSky mod
        public bool JetSky { get; private set; }

        // Moonshine still mod
        public bool Moonshinestill { get; private set; }

        // HayosikoColorfulGauges
        public bool HayosikoColorfulGauges { get; private set; }

        // CD Player Enhanced
        //public bool CDPlayerEnhanced { get; private set; }

        // CarryMore
        public bool CarryMore { get; private set; }
        public readonly Vector3 CarryMoreTempPosition = new Vector3(0.0f, -1000.0f, 0.0f);

        public CompatibilityManager()
        {
            instance = this;

            DrivableFury = IsModPresent("FURY");
            SecondFerndale = IsModPresent("SecondFerndale");
            Gaz = IsModPresent("GAZ24");
            PoliceFerndale = IsModPresent("Police_Ferndale");
            VhsPlayer = IsModPresent("VHSPlayer");
            OffroadHayosiko = IsModPresent("OffroadHayosiko");
            JetSky = IsModPresent("JetSky");
            Moonshinestill = IsModPresent("MSCStill");
            HayosikoColorfulGauges = IsModPresent("HayosikoColorfulGauges");
            //CDPlayerEnhanced = IsModPresent("CDPlayer");
            CarryMore = IsModPresent("CarryMore");

            ModConsole.Print("[MOP] Compatibility Manager done");
        }

        /// <summary>
        /// Checks if mod is present by modID using ModLoader.IsModPresent.
        /// </summary>
        /// <param name="modID"></param>
        /// <returns></returns>
        bool IsModPresent(string modID)
        {
            bool isModPresent = ModLoader.IsModPresent(modID);

            if (isModPresent)
            {
                string modName = modID;

                foreach (var mod in ModLoader.LoadedMods)
                    if (mod.ID == modID)
                        modName = mod.Name;

                ModConsole.Print($"[MOP] {modName} has been found!");
            }

            return isModPresent;
        }
    }
}
