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

using System.Collections.Generic;
using UnityEngine;

namespace MOP
{
    class Sector : MonoBehaviour
    {
        const string ReferenceObjectName = "PLAYER";

        public void Initialize(Vector3 size)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == ReferenceObjectName)
            {
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
            }
        }
    }

    class SectorManager : MonoBehaviour
    {
        public static SectorManager instance;

        List<GameObject> disabledObjects;
        public List<GameObject> DisabledObjects { get => disabledObjects; }

        public bool PlayerInSector;

        List<GameObject> sectors;

        public SectorManager()
        {
            instance = this;

            disabledObjects = new List<GameObject>
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
                GameObject.Find("MAP/MESH/FOLIAGE/LAKE_VEGETATION"),
                GameObject.Find("BusStop"),
                GameObject.Find("BusStop 1"),
                GameObject.Find("MachineHall"),
                GameObject.Find("YARD/UNCLE/Shed"),
                GameObject.Find("YARD/UNCLE/Greenhouse"),
                GameObject.Find("YARD/UNCLE/LOD"),
                GameObject.Find("YARD/UNCLE/Home"),
                GameObject.Find("YARD/UNCLE/Building"),
                GameObject.Find("MAP/RadioMast"),
                GameObject.Find("MAP/LakeSimple/Tile"),
                GameObject.Find("MAP/Bottom"),
                GameObject.Find("MAP/PierHome")
            };

            sectors = new List<GameObject>();

            // Load rule files
            List<GameObject> newList = new List<GameObject>();
            foreach (GameObject obj in disabledObjects)
            {
                IgnoreRule rule = RuleFiles.instance.IgnoreRules.Find(f => f.ObjectName == obj.name);
                if (rule == null)
                    newList.Add(obj);
            }

            disabledObjects = newList;

            CreateNewSector(new Vector3(-16.77627f, -0.5062422f, 1.559867f), new Vector3(5,5,9)); // Garage
            CreateNewSector(new Vector3(-1548, 4, 1184), new Vector3(8, 5, 4.5f), new Vector3(0, 328, 0)); // Teimo
            CreateNewSector(new Vector3(1562.49f, 4.8f, 733.8835f), new Vector3(15, 5, 20), new Vector3(0, 335, 0)); // Repair shop
            // CreateNewSector(new Vector3(-7.2f, -0.5062422f, 9.559867f), new Vector3(11, 5, 9)); // Yard
        }

        void CreateNewSector(Vector3 position, Vector3 size)
        {
            GameObject newSector = new GameObject("MOP_Sector");
            //GameObject newSector = GameObject.CreatePrimitive(PrimitiveType.Cube); // DEBUG
            //newSector.name = "MOP_SECTOR_DEBUG"; //DEBUG
            //newSector.transform.localScale = size; // DEBUG
            //Object.Destroy(newSector.GetComponent<Collider>()); // DEBUG
            newSector.transform.position = position;
            Sector sectorInfo = newSector.AddComponent<Sector>();
            sectorInfo.Initialize(size);
            sectors.Add(newSector);
        }

        void CreateNewSector(Vector3 position, Vector3 size, Vector3 rotation)
        {
            GameObject newSector = new GameObject("MOP_Sector");
            //GameObject newSector = GameObject.CreatePrimitive(PrimitiveType.Cube); // DEBUG
            //newSector.name = "MOP_SECTOR"; //DEBUG
            //newSector.transform.localScale = size; // DEBUG
            //Object.Destroy(newSector.GetComponent<Collider>()); // DEBUG
            newSector.transform.position = position;
            newSector.transform.localEulerAngles = rotation;
            Sector sectorInfo = newSector.AddComponent<Sector>();
            sectorInfo.Initialize(size);
            sectors.Add(newSector);
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
                SectorManager.instance.DisabledObjects[i].SetActive(enabled);
        }
    }
}
