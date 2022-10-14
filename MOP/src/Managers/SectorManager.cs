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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOP.Common;
using MOP.Common.Enumerations;
using MOP.Rules;
using MOP.Rules.Types;
using MOP.Common.Interfaces;

namespace MOP.Managers
{
    class SectorManager : MonoBehaviour, IManager<Sector>
    {
        static SectorManager instance;
        public static SectorManager Instance { get => instance; }

        List<GameObject> objectsToDisable;
        
        List<Sector> sectors;
        List<Sector> activeSectors;

        readonly string[] qualityModeIgnore = { "RadioMast", "Tile", "LakeNice", "BUSHES", "PierHome", "TREES" };

        const int FramesWaitOnSectorEnter = 15;

        public GameObject PlayerCheck { get; private set; }

        public int Count => sectors.Count;

        public List<Sector> GetAll => sectors;

        public SectorManager()
        {
            instance = this;

            ModConsole.Log("[MOP] Loading sectors...");

            PlayerCheck = new GameObject("MOP_PlayerCheck");
            PlayerCheck.layer = 20; // This layer is ignored by MSC's player "hand" raycasting.
            PlayerCheck.transform.parent = GameObject.Find("PLAYER").transform;
            PlayerCheck.transform.localPosition = Vector3.zero;
            BoxCollider collider = PlayerCheck.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(.1f, 1, .1f);
            Rigidbody rb = PlayerCheck.AddComponent<Rigidbody>();
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

            sectors = new List<Sector>();

            // Load rule files
            if (RulesManager.Instance.GetList<IgnoreRule>().Count > 0)
            {
                List<GameObject> newList = new List<GameObject>();
                foreach (GameObject obj in objectsToDisable)
                {
                    try
                    {
                        if (obj == null)
                            continue;

                        if (!RulesManager.Instance.GetList<IgnoreRule>().Any(f => f.ObjectName == obj.name))
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
            if (MopSettings.Mode == PerformanceMode.Performance)
            {
                CreateNewSector(new Vector3(-16.77627f, -0.5062422f, 1.559867f), new Vector3(5, 5, 9), 1500, "PierHome");
            }
            else
            {
                CreateNewSector(new Vector3(-16.77627f, -0.5062422f, 1.559867f), new Vector3(5, 5, 9), 1500, "PierHome", "Tile", "LakeNice", "TREES_MEDIUM3", "BUSHES3", "BUSHES6", "RadioMast");
            }
            // Teimo
            CreateNewSector(new Vector3(-1547.4f, 4, 1183.35f), new Vector3(9.6f, 5, 5.5f), new Vector3(0, 328, 0), 500,
                "StreetLights", "HUMANS", "TRAFFIC", "NPC_CARS", "PERAJARVI", "TrafficSigns", "StreetLights", "ELEC_POLES", "TREES_SMALL1", "BusStop", "BUSHES3");
            CreateNewSector(new Vector3(-1551.7f, 4, 1185.8f), new Vector3(4.7f, 5, 3.3f), new Vector3(0, 328, 0), 50,
                "StreetLights", "HUMANS", "TRAFFIC", "NPC_CARS", "PERAJARVI", "TrafficSigns", "StreetLights", "ELEC_POLES", "TREES_SMALL1"); // Back.
            // Repair shop
            CreateNewSector(new Vector3(1562.49f, 4.8f, 733.8835f), new Vector3(15, 5, 20), new Vector3(0, 335, 0), 500, 
                "TRAFFIC", "ELEC_POLES", "Buildings", "HUMANS", "TrafficSigns", "StreetLights", "TREES_MEDIUM3", "BUSHES6");
            // Yard Machine Hall
            CreateNewSector(new Vector3(54.7f, -0.5062422f, -73.9f), new Vector3(6, 5, 5.2f), 1500, "YARD", "MachineHall", "BUSHES3", "BUSHES6", "TREES_SMALL1");
            // Home
            CreateNewSector(new Vector3(-7.2f, -0.5062422f, 9.9f), new Vector3(11, 5, 9.5f), 1500, "PierHome", "TREES_SMALL1", "BUSHES7", "Building", "RadioMast"); // Living room, kitchen, bedrooms.
            CreateNewSector(new Vector3(-12.5f, -0.5062422f, 1.2f), new Vector3(3, 5, 8f), 300, "PierHome", "TREES_SMALL1", "Building"); // Sauna, bathroom.
            CreateNewSector(new Vector3(-13.4f, -0.5062422f, 6.4f), new Vector3(1.4f, 5, 1.7f), 100, "PierHome"); // Storage room (kitchen).
            // Jail
            CreateNewSector(new Vector3(-655, 5, -1156), new Vector3(5, 5, 9f), 50);
            // Cottage
            CreateNewSector(new Vector3(-848.2f, -2, 505.6f), new Vector3(5, 3, 5.2f), new Vector3(0, 342, -1.07f), 2200, 
                "BUSHES7", "TREES_SMALL4", "TREES_MEDIUM3", "LakeNice", "TRAFFIC", "Tile");
            // Cabin
            CreateNewSector(new Vector3(-165.55f, -3.7f, 1020.7f), new Vector3(5, 4, 3.5f), 1400, "LakeNice", "Tile", "BUSHES7", "TREES_SMALL1");

            // Driveway sector
            if (MopSettings.Mode == PerformanceMode.Performance)
                CreateNewSector(new Vector3(-18.5f, -0.5062422f, 11.9f), new Vector3(11f, 5, 9.5f), 3000,
                    "TREES_SMALL1", "BUSHES7", "BUSHES3", "BUSHES6", "TREES_MEDIUM3", "YARD", "LakeNice", "Tile", "PierHome");

            // Generating sectors from rule files.
            if (RulesManager.Instance.GetList<NewSector>().Count > 0)
            {
                foreach (NewSector sector in RulesManager.Instance.GetList<NewSector>())
                {
                    try
                    {
                        CreateNewSector(sector.Position, sector.Scale, sector.Rotation, 300, sector.Whitelist);
                    }
                    catch (System.Exception ex)
                    {
                        ExceptionManager.New(ex, false, "CUSTOM_SECTOR_FAIL");
                    }
                }
            }

            ModConsole.Log($"[MOP] Loaded {sectors.Count} sectors");

            if (MopSettings.GenerateToggledItemsListDebug)
            {
                ToggledItemsListGenerator.CreateSectorList(objectsToDisable);
            }
        }

        void CreateNewSector(Vector3 position, Vector3 size, int renderDistance, params string[] ignoreList) 
            => CreateNewSector(position, size, Vector3.zero, renderDistance, ignoreList);
        void CreateNewSector(Vector3 position, Vector3 size, Vector3 rotation, int renderDistance, params string[] ignoreList)
        {
            GameObject newSector = new GameObject("MOP_Sector");
            newSector.transform.position = position;
            newSector.transform.localEulerAngles = rotation;
            Sector sectorInfo = newSector.AddComponent<Sector>();

            if (ignoreList.Length == 0)
                ignoreList = new string[0];

            sectorInfo.Initialize(size, renderDistance, ignoreList);
            Add(sectorInfo);
        }

        public void ToggleActive()
        {
            for (int i = 0; i < objectsToDisable.Count; ++i)
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

                obj.SetActive(!IsPlayerInSector());
            }
        }

        internal void AddActiveSector(Sector sector)
        {
            if (!activeSectors.Contains(sector))
            {
                activeSectors.Add(sector);
            }
        }

        internal void RemoveActiveSector(Sector sector)
        {
            if (activeSectors.Contains(sector))
            {
                activeSectors.Remove(sector);
            }
        }

        public bool SectorRulesContains(string name)
        {
            if (activeSectors.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < activeSectors.Count; i++)
            {
                if (activeSectors[i].IgnoreList.Contains(name))
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

        public int GetCurrentSectorDrawDistance()
        {
            if (activeSectors.Count == 0)
            {
                throw new System.Exception("No sector is active.");
            }

            return activeSectors[0].DrawDistance;
        }

        public bool IsPlayerInSector(Sector sector)
        {
            return activeSectors.Contains(sector);
        }

        public Sector Add(Sector obj)
        {
            sectors.Add(obj);
            return obj;
        }

        public void Remove(Sector obj)
        {
            if (sectors.Contains(obj))
            {
                sectors.Remove(obj);
            }
        }

        public void RemoveAt(int index)
        {
            sectors.RemoveAt(index);
        }

        public int EnabledCount
        {
            get
            {
                return activeSectors.Count;
            }
        }
    }
}
