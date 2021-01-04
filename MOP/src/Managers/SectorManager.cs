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

using MSCLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOP.Common;
using MOP.Common.Enumerations;
using MOP.Rules;
using MOP.Rules.Types;

namespace MOP.Managers
{
    class SectorManager : MonoBehaviour
    {
        static SectorManager instance;
        public static SectorManager Instance { get => instance; }

        List<GameObject> objectsToDisable;
        
        List<GameObject> sectors;
        List<Sector> activeSectors;

        readonly string[] qualityModeIgnore = { "RadioMast", "Tile", "LakeNice", "BUSHES", "PierHome", "TREES" };

        public SectorManager()
        {
            instance = this;

            ModConsole.Print("[MOP] Loading sectors...");

            GameObject colliderCheck = new GameObject("MOP_PlayerCheck");
            colliderCheck.layer = 20;
            colliderCheck.transform.parent = GameObject.Find("PLAYER").transform;
            colliderCheck.transform.localPosition = Vector3.zero;
            BoxCollider collider = colliderCheck.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(.1f, 1, .1f);
            Rigidbody rb = colliderCheck.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            activeSectors = new List<Sector>();

            objectsToDisable = new List<GameObject>
            {
                GameObject.Find("ELEC_POLES"),
                GameObject.Find("ELEC_POLES_COLL"),
                GameObject.Find("TREES_MEDIUM3"),
                GameObject.Find("TREES_SMALL1"),
                GameObject.Find("TREES1_COLL"),
                GameObject.Find("TREES2_COLL"),
                GameObject.Find("TREES3_COLL"),
                GameObject.Find("BUSHES3"),
                GameObject.Find("BUSHES6"),
                GameObject.Find("BUSHES7"),
                GameObject.Find("BusStop"),
                GameObject.Find("BusStop 1"),
                GameObject.Find("MachineHall"),
                GameObject.Find("YARD/UNCLE/Shed"),
                GameObject.Find("YARD/UNCLE/Greenhouse"),
                GameObject.Find("YARD/UNCLE/LOD"),
                GameObject.Find("YARD/UNCLE/Home"),
                GameObject.Find("YARD/UNCLE/Building"),
                GameObject.Find("MAP/PierHome"),
                GameObject.Find("MAP/MESH/FOLIAGE/LAKE_VEGETATION"),
                GameObject.Find("MAP/RadioMast")
            };

            GameObject lakeSimpleTile = GameObject.Find("MAP/LakeSimple/Tile");
            if (lakeSimpleTile != null)
                objectsToDisable.Add(lakeSimpleTile);

            GameObject lakeNice = GameObject.Find("MAP/LakeNice");
            if (lakeNice != null)
                objectsToDisable.Add(lakeNice);

            sectors = new List<GameObject>();

            // Load rule files
            if (RulesManager.Instance.IgnoreRules.Count > 0)
            {
                List<GameObject> newList = new List<GameObject>();
                foreach (GameObject obj in objectsToDisable)
                {
                    try
                    {
                        if (obj == null)
                            continue;

                        if (!RulesManager.Instance.IgnoreRules.Any(f => f.ObjectName == obj.name))
                            newList.Add(obj);
                    }
                    catch (System.Exception ex)
                    {
                        ExceptionManager.New(ex, false, "SECTOR_RULES_LOAD_ERROR");
                    }
                }
                objectsToDisable = newList;
            }

            // Garage
            CreateNewSector(new Vector3(-16.77627f, -0.5062422f, 1.559867f), new Vector3(5, 5, 9), "PierHome");
            // Teimo
            CreateNewSector(new Vector3(-1547.3f, 4, 1183.35f), new Vector3(9.6f, 5, 5.5f), new Vector3(0, 328, 0),
                "StreetLights", "HUMANS", "TRAFFIC", "NPC_CARS", "PERAJARVI", "TrafficSigns", "StreetLights", "ELEC_POLES", "TREES_SMALL1");
            CreateNewSector(new Vector3(-1551.7f, 4, 1185.8f), new Vector3(4.6f, 5, 2.5f), new Vector3(0, 328, 0),
                "StreetLights", "HUMANS", "TRAFFIC", "NPC_CARS", "PERAJARVI", "TrafficSigns", "StreetLights", "ELEC_POLES", "TREES_SMALL1");
            // Repair shop
            CreateNewSector(new Vector3(1562.49f, 4.8f, 733.8835f), new Vector3(15, 5, 20), new Vector3(0, 335, 0), "TRAFFIC", "ELEC_POLES", "Buildings",
                "HUMANS", "TrafficSigns", "StreetLights");
            // Yard Machine Hall
            CreateNewSector(new Vector3(54.7f, -0.5062422f, -73.9f), new Vector3(6, 5, 5.2f), "YARD", "MachineHall", "BUSHES3", "BUSHES6", "TREES_SMALL1");
            // Home
            CreateNewSector(new Vector3(-7.2f, -0.5062422f, 9.9f), new Vector3(11, 5, 9.5f), "PierHome", "TREES_SMALL1", "BUSHES7", "Building"); // Living room, kitchen, bedrooms.
            CreateNewSector(new Vector3(-12.5f, -0.5062422f, 1), new Vector3(3, 5, 7.7f), "PierHome", "TREES_SMALL1", "BUSHES7", "Building"); // Sauna, bathroom.
            CreateNewSector(new Vector3(-13.5f, -0.5062422f, 6.4f), new Vector3(1.3f, 5, 1.7f), "PierHome", "TREES_SMALL1", "BUSHES7", "Building"); // Storage room (kitchen).
            // Jail
            CreateNewSector(new Vector3(-655, 5, -1156), new Vector3(5, 5, 9f));
            // Cottage
            CreateNewSector(new Vector3(-848.2f, -2, 505.6f), new Vector3(5, 3, 5.2f), new Vector3(0, 342, -1.07f), "BUSHES7", "TREES_SMALL4", "TREES_MEDIUM3", "LakeNice", "TRAFFIC", "Tile");
            // Cabin
            CreateNewSector(new Vector3(-165.55f, -3.7f, 1020.7f), new Vector3(5, 4, 3.5f), "LakeNice", "Tile", "BUSHES7", "TREES_SMALL1");

            // Driveway sector
            //if (RulesManager.Instance.SpecialRules.DrivewaySector)
            if (MopSettings.Mode == PerformanceMode.Performance)
                CreateNewSector(new Vector3(-18.5f, -0.5062422f, 11.9f), new Vector3(11f, 5, 9.5f),
                    "TREES_SMALL1", "BUSHES7", "BUSHES3", "BUSHES6", "TREES_MEDIUM3", "YARD", "LakeNice", "Tile", "PierHome");

            // Generating sectors from rule files.
            if (RulesManager.Instance.NewSectors.Count > 0)
            {
                foreach (NewSector sector in RulesManager.Instance.NewSectors)
                {
                    CreateNewSector(sector.Position, sector.Scale, sector.Rotation, sector.Whitelist);
                }
            }

            ModConsole.Print($"[MOP] Loaded {sectors.Count} sectors");

            if (MopSettings.GenerateToggledItemsListDebug)
            {
                if (System.IO.File.Exists("sector.txt"))
                    System.IO.File.Delete("sector.txt");
                string sector = "";
                foreach (var w in objectsToDisable)
                {
                    sector += w.name + ", ";
                }
                System.IO.File.WriteAllText("sector.txt", sector);
                System.Diagnostics.Process.Start("sector.txt");
            }
        }

        void CreateNewSector(Vector3 position, Vector3 size, params string[] ignoreList)
        {
            CreateNewSector(position, size, Vector3.zero, ignoreList);
        }

        void CreateNewSector(Vector3 position, Vector3 size, Vector3 rotation, params string[] ignoreList)
        {
            GameObject newSector = new GameObject("MOP_Sector");
            newSector.transform.position = position;
            newSector.transform.localEulerAngles = rotation;
            Sector sectorInfo = newSector.AddComponent<Sector>();

            if (ignoreList.Length == 0)
                ignoreList = new string[0];

            sectorInfo.Initialize(size, ignoreList);
            sectors.Add(newSector);
        }

        public void ToggleActive(bool enabled)
        {
            for (int i = 0; i < objectsToDisable.Count; i++)
            {
                // Safe check if somehow the i gets bigger than array length.
                if (i > objectsToDisable.Count) break;

                GameObject obj = objectsToDisable[i];

                if (obj == null)
                    continue;

                if (MopSettings.Mode == PerformanceMode.Quality && obj.name.ContainsAny(qualityModeIgnore))
                    continue;

                if (SectorRulesContains(obj.name))
                {
                    obj.SetActive(true);
                    continue;
                }

                obj.SetActive(enabled);
            }
        }

        internal void AddActiveSector(Sector sector)
        {
            activeSectors.Add(sector);
        }

        internal void RemoveActiveSector(Sector sector)
        {
            activeSectors.Remove(sector);
        }

        public bool SectorRulesContains(string name)
        {
            if (activeSectors.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < activeSectors.Count; i++)
            {
                if (activeSectors[i].GetIgnoreList().Contains(name))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsPlayerInSector()
        {
            return activeSectors.Count > 0;
        }
    }
}
