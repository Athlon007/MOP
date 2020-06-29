// Modern Optimization Plugin
// Copyright(C) 2019-2020 Athlon

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

namespace MOP
{
    class Sector : MonoBehaviour
    {
        const string ReferenceObjectName = "MOP_PlayerCheck";
        string[] ignoreList;

        public void Initialize(Vector3 size, params string[] ignoreList)
        {
            // Set the layer to Ignore Raycast layer.
            gameObject.layer = 2;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;

            if (ignoreList != null)
                this.ignoreList = ignoreList;

            if (gameObject.name == "MOP_SECTOR_DEBUG")
            {
                collider.size = new Vector3(1, 1, 1);
                gameObject.layer = 0;
            }

            WorldManager.instance.GetWorldObjectList().Add(this.gameObject, 500);
            this.transform.parent = WorldManager.instance.gameObject.transform;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == ReferenceObjectName)
            {
                if (ignoreList != null)
                    Rules.instance.AddSectorRule(ignoreList);

                SectorManager.instance.PlayerInSector = true;
                SectorManager.instance.ToggleActive(false);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == ReferenceObjectName)
            {
                SectorManager.instance.PlayerInSector = false;
                SectorManager.instance.ToggleActive(true);

                if (ignoreList.Length > 0)
                    Rules.instance.ClearSectorRules();
            }
        }
    }

    class SectorManager : MonoBehaviour
    {
        public static SectorManager instance;

        public List<GameObject> DisabledObjects { get; }
        readonly List<GameObject> sectors;

        public bool PlayerInSector;

        int loadedSectors;

        public SectorManager()
        {
            instance = this;

            ModConsole.Print("[MOP] Loading sectors...");

            DisabledObjects = new List<GameObject>
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
                GameObject.Find("MAP/RadioMast"),
                GameObject.Find("MAP/PierHome"),
                GameObject.Find("MAP/MESH/FOLIAGE/LAKE_VEGETATION")
            };

            GameObject lakeSimpleTile = GameObject.Find("MAP/LakeSimple/Tile");
            if (lakeSimpleTile != null)
                DisabledObjects.Add(lakeSimpleTile);

            GameObject lakeNice = GameObject.Find("MAP/LakeNice");
            if (lakeNice != null)
                DisabledObjects.Add(lakeNice);

            sectors = new List<GameObject>();

            // Load rule files
            if (Rules.instance.IgnoreRules.Count > 0)
            {
                List<GameObject> newList = new List<GameObject>();
                foreach (GameObject obj in DisabledObjects)
                {
                    try
                    {
                        if (obj == null)
                            continue;

                        if (!Rules.instance.IgnoreRules.Any(f => f.ObjectName == obj.name))
                            newList.Add(obj);
                    }
                    catch (System.Exception ex)
                    {
                        ExceptionManager.New(ex, "SECTOR_RULES_LOAD_ERROR");
                    }
                }
                DisabledObjects = newList;
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
            CreateNewSector(new Vector3(-165.55f, -3.7f, 1020.7f), new Vector3(5, 4, 3.5f),"LakeNice", "Tile", "BUSHES7", "TREES_SMALL1");
            
            // Driveway sector
            if (Rules.instance.SpecialRules.DrivewaySector)
                CreateNewSector(new Vector3(-18.5f, -0.5062422f, 11.9f), new Vector3(11f, 5, 9.5f), 
                    "PierHome", "TREES_SMALL1", "BUSHES7", "BUSHES3", "BUSHES6", "TREES_MEDIUM3", "YARD", "LakeNice", "Tile"); // Driveway.

            // Generating sectors from rule files.
            if (Rules.instance.NewSectors.Count > 0)
            {
                foreach (NewSector sector in Rules.instance.NewSectors)
                {
                    CreateNewSector(sector.Position, sector.Scale, sector.Rotation, sector.Whitelist);
                }
            }

            ModConsole.Print($"[MOP] Loaded {loadedSectors} sectors");
        }

        void CreateNewSector(Vector3 position, Vector3 size, params string[] ignoreList)
        {
            GameObject newSector;
            if (MopSettings.SectorDebugMode)
            {
                newSector = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newSector.name = "MOP_SECTOR_DEBUG";
                newSector.transform.localScale = size;
                Object.Destroy(newSector.GetComponent<Collider>());
                newSector.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                newSector = new GameObject("MOP_Sector");
            }
            newSector.transform.position = position;
            Sector sectorInfo = newSector.AddComponent<Sector>();

            if (ignoreList.Length == 0)
                ignoreList = new string[0];

            sectorInfo.Initialize(size, ignoreList);
            sectors.Add(newSector);

            if (Rules.instance.SpecialRules.ExperimentalOptimization)
            {
                if (ignoreList.Length > 0 && ignoreList[0] == "PierHome")
                    newSector.AddComponent<SatsumaInGarage>();
            }

            loadedSectors++;
        }

        void CreateNewSector(Vector3 position, Vector3 size, Vector3 rotation, params string[] ignoreList)
        {
            GameObject newSector;
            if (MopSettings.SectorDebugMode)
            {
                newSector = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newSector.name = "MOP_SECTOR_DEBUG";
                newSector.transform.localScale = size; 
                Object.Destroy(newSector.GetComponent<Collider>());
                newSector.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                newSector = new GameObject("MOP_Sector");
            }
            newSector.transform.position = position;
            newSector.transform.localEulerAngles = rotation;
            Sector sectorInfo = newSector.AddComponent<Sector>();

            if (ignoreList.Length == 0)
                ignoreList = new string[0];

            sectorInfo.Initialize(size, ignoreList);
            sectors.Add(newSector);

            loadedSectors++;
        }

        /// <summary>
        /// Destroys all sectors and reeanbles all disabled objects
        /// </summary>
        public void DestroyAllSectors()
        {
            for (int i = 0; i < sectors.Count; i++)
                GameObject.Destroy(sectors[i]);

            PlayerInSector = false;
            for (int i = 0; i < DisabledObjects.Count; i++)
                DisabledObjects[i].SetActive(true);
        }

        public void ToggleActive(bool enabled)
        {
            for (int i = 0; i < SectorManager.instance.DisabledObjects.Count; i++)
            {
                // Safe check if somehow the i gets bigger than array length.
                if (i > SectorManager.instance.DisabledObjects.Count) break;

                GameObject obj = SectorManager.instance.DisabledObjects[i];

                if (obj == null)
                    continue;

                if (Rules.instance.SectorRulesContains(obj.name))
                    continue;

                obj.SetActive(enabled);
            }
        }
    }
}
