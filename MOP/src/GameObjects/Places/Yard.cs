using UnityEngine;

namespace MOP
{
    class Yard : Place
    {
        // Yard Class
        //
        // Extends Place.cs
        //
        // It's responsbile for loading and unloading the YARD game object (player's house).
        //
        // NOTE: That script DOES NOT disable the yard itself, rather some of its childrens.

        // Objects from that blaclklist will not be disabled
        string[] blackList = { "YARD", "Spawn", "VenttiPigHouse", "Capsule", "Target", "Pivot", "skeleton", "bodymesh",
                                "COPS", "Trigger", "Cop", "Collider", "thig", "pelvis", "knee", "ankle", "spine",
                                "PlayerMailBox", "mailbox", "envelope", "Envelope", "Letter", "Ad", "UNCLE",
                                "Uncle", "GifuKeys", "key", "anim", "Pos", "GraveYardSpawn", "LOD", "HOUSEFIRE",
                                "Livingroom", "Fire", "0", "1", "2", "Particle", "smoke", "Flame", "livingroom",
                                "Kitchen", "kitchen", "Bathroom", "bathroom", "Bedroom", "bedroom", "Sauna",
                                "sauna", "COMPUTER", "SYSTEM", "Computer", "TriggerPlayMode", "Meshes", "386",
                                "monitor", "mouse", "cord", "Button", "Sound", "led", "DiskDrive", "Sled",
                                "disk", "Power", "power", "Floppy", "LCD", "Screen", "Memory", "Mesh",
                                "Dynamics", "Light", "Electric", "KWH", "kwh", "Clock", "TV", "Program", "program",
                                "Buzz", "switch", "Switch", "Fines", "Colliders", "coll", "Shower", "Water", "Tap",
                                "Valve", "Telephone", "Cord", "Socket", "Logic", "Phone", "table", "MAP", "Darts", "Booze",
                                "Shit", "Wood", "Grandma", "SAVEGAME", "Shelf", "shelf", "Garage", "Building", "LIVINGROOM",
                                "BEDROOM1", "Table", "boybed", "KITCHEN", "Fridge", "bench", "wood", "Pantry", "Glass",
                                "closet", "Numbers", "Ring", "log", "washingmachine", "lauteet", "MIDDLEROOM", "BeerCamp",
                                "Chair", "TablePlastic", "LOD_middleroom", "hotwaterkeeper", "house_roof", "BATHROOM", "lamp" };

        /// <summary>
        /// Initialize the RepairShop class
        /// </summary>
        public Yard() : base("YARD")
        {
            // Fix for broken computer.
            // We're changing it to null.
            if (GameObject.Find("COMPUTER") != null)
                GameObject.Find("COMPUTER").transform.parent = null;

            GameObjectBlackList = blackList;
            DisableableChilds = GetDisableableChilds();

            // Remove fridge mesh from the list of disabled objects
            Transform fridgeMesh = DisableableChilds.Find(w => w.name == "mesh" && w.transform.parent.name == "Fridge");
            DisableableChilds.Remove(fridgeMesh);
        }
    }
}
