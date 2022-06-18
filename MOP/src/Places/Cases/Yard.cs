// Modern Optimization Plugin
// Copyright(C) 2019-2022 Athlon

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

using System;
using HutongGames.PlayMaker;
using System.Linq;
using UnityEngine;

using MOP.FSM;
using MOP.Common;

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
            "houseuncle_roof", "houseuncle_walls", "fuse holder(Clone)", "SAUNA"
        };

        const float ChillDistance = .45f;
        readonly Transform chillPoint;
        readonly FsmBool fridgeRunning;
        
        GameObject sauna;
        GameObject saumaSimulation;
        readonly FsmFloat saunaStoveHeat;
        const float StoveOnSimulationPoint = 35; // Stove heat after which we will simulate stove overheating


        /// <summary>
        /// Initialize the RepairShop class
        /// </summary>
        public Yard() : base("YARD")
        {
            Instance = this;

            // Fix for broken computer.
            // We're changing it to null.
            RemoveComputerFromHome();
            FixGarageDoors();

            try
            {
                foreach (Transform door in transform.GetComponentsInChildren<Transform>().Where(t => t.root == transform && t.gameObject.name.Contains("Door") && t.Find("Pivot") != null).ToArray())
                {
                    if (door.Find("Pivot/Handle") != null)
                        door.Find("Pivot/Handle").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "FRIDGE_DOORHANDLE_ERROR");
            }

            GameObjectBlackList.AddRange(blackList);
            DisableableChilds = GetDisableableChilds();

            // Remove fridge mesh from the list of disabled objects
            Transform fridgeMesh = DisableableChilds.Find(w => w.name == "mesh" && w.transform.parent.name == "Fridge");
            DisableableChilds.Remove(fridgeMesh);

            // Disable restarting of FSM in UNCLE object.
            transform.Find("UNCLE").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            Compress();

            FixSaunaSimulation();

            chillPoint = transform.Find("Building/KITCHEN/Fridge/FridgePoint/ChillArea");
            try
            {
                GameObject fridgePoint = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "FridgePoint");
                if (fridgePoint)
                {
                    fridgeRunning = fridgePoint.GetPlayMaker("Chilling")?.FsmVariables.GetFsmBool("Kitchen");
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "FRIDGE_RUNNING_FAILURE");
            }

            // Get sauna simulation.
            try
            {
                sauna = transform.Find("Building/SAUNA")?.gameObject;
                if (sauna != null)
                {
                    saumaSimulation = sauna.transform.Find("Sauna/Simulation")?.gameObject;
                    if (saumaSimulation != null)
                    {
                        saunaStoveHeat = saumaSimulation.GetPlayMaker("Time").FsmVariables.GetFsmFloat("StoveHeat");
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SAUNA_STOVE_SIMULATION_FAILURE");
            }

            LightSources = GetLightSources();
        }

        private static void FixGarageDoors()
        {
            GameObject garageDoors = GameObject.Find("GarageDoors");
            if (garageDoors)
            {
                garageDoors.transform.Find("DoorLeft/Coll").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                garageDoors.transform.Find("DoorRight/Coll").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
        }

        private static void RemoveComputerFromHome()
        {
            GameObject computer = GameObject.Find("COMPUTER");
            if (computer != null)
            {
                computer.transform.parent = null;
            }
        }

        public bool IsItemInFridge(GameObject item)
        {
            if (fridgeRunning == null)
                return false;

            if (!fridgeRunning.Value)
                return false;

            return Vector3.Distance(item.transform.position, chillPoint.position) < ChillDistance;
        }

        private void FixSaunaSimulation()
        {
            if (transform.Find("Building/SAUNA") != null)
            {
                transform.Find("Building/SAUNA/Sauna/Kiuas/ButtonPower").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                transform.Find("Building/SAUNA/Sauna/Kiuas/ButtonTime").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                transform.Find("Building/SAUNA/Sauna/Kiuas/StoveTrigger").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                transform.Find("Building/SAUNA/Sauna/Simulation").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
        }

        public override void ToggleActive(bool enabled)
        {
            // Don't execute the code, if the enabled value is the same as the activity status.
            if (isActive == enabled)
                return;

            isActive = enabled;

            // Load and unload only the objects that aren't on the whitelist.
            for (int i = 0; i < DisableableChilds.Count; i++)
            {
                // If the object is missing, skip and continue.
                if (DisableableChilds[i] == null)
                    continue;

                DisableableChilds[i].gameObject.SetActive(enabled);
            }

            if (PlayMakers.Count > 0)
            {
                for (int i = 0; i < PlayMakers.Count; i++)
                {
                    if (PlayMakers[i] == null)
                    {
                        continue;
                    }

                    PlayMakers[i].enabled = enabled;
                }
            }

            if (LightSources.Count > 0)
            {
                if (FsmManager.ShadowsHouse)
                {
                    for (int i = 0; i < LightSources.Count; ++i)
                    {
                        LightSources[i].shadows = enabled ? LightShadows.Hard : LightShadows.None;
                    }
                }
            }

            if (sauna != null)
            {
                if (saunaStoveHeat.Value > StoveOnSimulationPoint)
                {
                    sauna.SetActive(true);
                    saumaSimulation.SetActive(true);
                }
                else
                {
                    sauna.SetActive(enabled);
                    saumaSimulation.SetActive(enabled);
                }
            }
        }
    }
}
