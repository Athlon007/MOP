// Modern Optimization Plugin
// Copyright(C) 2019-2021 Athlon

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
using System.Linq;
using UnityEngine;

namespace MOP.Places
{
    class Yard : Place
    {
        public static Yard Instance;

        readonly string[] blackList =
        {
            "YARD", "Spawn", "VenttiPigHouse", "Capsule", "Target", "Pivot", "skeleton", "bodymesh",
            "COPS", "Trigger", "Cop", "Collider", "thig", "pelvis", "knee", "ankle", "spine",
            "PlayerMailBox", "mailbox", "envelope", "Envelope", "Letter", "Ad", "UNCLE",
            "Uncle", "GifuKeys", "key", "anim", "Pos", "GraveYardSpawn", "HOUSEFIRE",
            "Livingroom", "Fire", "0", "1", "2", "Particle", "smoke", "Flame", "livingroom",
            "Kitchen", "kitchen", "Bathroom", "bathroom", "Bedroom", "bedroom", "Sauna",
            "sauna", "COMPUTER", "SYSTEM", "Computer", "TriggerPlayMode", "Meshes", "386",
            "monitor", "mouse", "cord", "Button", "Sound", "led", "DiskDrive", "Sled",
            "disk", "Power", "power", "Floppy", "LCD", "Screen", "Memory", "Mesh",
            "Dynamics", "Light", "Electric", "KWH", "kwh", "Clock", "Program", "program",
            "Buzz", "switch", "Switch", "Fines", "Colliders", "coll", "Shower", "Water", "Tap",
            "Valve", "Telephone", "Cord", "Socket", "Logic", "Phone", "table", "MAP", "Darts", "Booze",
            "Shit", "Wood", "Grandma", "SAVEGAME", "Shelf", "shelf", "Garage", "Building", "LIVINGROOM",
            "BEDROOM1", "Table", "boybed", "KITCHEN", "Fridge", "bench", "wood", "Pantry", "Glass",
            "closet", "Numbers", "Ring", "log", "washingmachine", "lauteet", "MIDDLEROOM", "BeerCamp",
            "Chair", "TablePlastic", "LOD_middleroom", "hotwaterkeeper", "house_roof",
            "WC", "Hallway", "Entry", "ContactPivot", "DoorRight", "DoorLeft", "GarageDoors", "BatteryCharger",
            "Clamps", "ChargerPivot", "Clamp", "BatteryPivot", "battery_charger", "Wire", "cable", "TriggerCharger",
            "tvtable", "VHS_Screen", "tv_table(Clone)", "scart_con", "Haybale", "Combine", "UncleWalking", "LOD", "house_wall_brick",
            "houseuncle_roof", "houseuncle_walls"
        };

        /// <summary>
        /// Initialize the RepairShop class
        /// </summary>
        public Yard() : base("YARD")
        {
            Instance = this;

            // Fix for broken computer.
            // We're changing it to null.
            if (GameObject.Find("COMPUTER") != null)
                GameObject.Find("COMPUTER").transform.parent = null;

            GameObject garageDoors = GameObject.Find("GarageDoors");
            garageDoors.transform.Find("DoorLeft/Coll").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            garageDoors.transform.Find("DoorRight/Coll").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            foreach (Transform door in transform.GetComponentsInChildren<Transform>().Where(t => t.root == transform && t.gameObject.name.Contains("Door") && t.Find("Pivot") != null).ToArray())
            {
                if (door.Find("Pivot/Handle") != null)
                    door.Find("Pivot/Handle").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }

            GameObjectBlackList.AddRange(blackList);
            DisableableChilds = GetDisableableChilds();

            // Remove fridge mesh from the list of disabled objects
            Transform fridgeMesh = DisableableChilds.Find(w => w.name == "mesh" && w.transform.parent.name == "Fridge");
            DisableableChilds.Remove(fridgeMesh);

            // Disable restarting of FSM in UNCLE object.
            transform.Find("UNCLE").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            Compress();

            transform.Find("Building/SAUNA/Sauna/Kiuas/ButtonPower").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            transform.Find("Building/SAUNA/Sauna/Kiuas/ButtonTime").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            transform.Find("Building/SAUNA/Sauna/Kiuas/StoveTrigger").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            transform.Find("Building/SAUNA/Sauna/Simulation").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            this.LightSources = GetLightSources();
        }
    }
}
