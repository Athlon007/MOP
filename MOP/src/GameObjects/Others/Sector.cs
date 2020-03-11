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
                for (int i = 0; i < SectorManager.instance.Objects.Count; i++)
                    SectorManager.instance.Objects[i].SetActive(false);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == ReferenceObjectName)
            {
                SectorManager.instance.PlayerInSector = false;
                for (int i = 0; i < SectorManager.instance.Objects.Count; i++)
                    SectorManager.instance.Objects[i].SetActive(true);
            }
        }
    }

    class SectorManager : MonoBehaviour
    {
        public static SectorManager instance;

        public List<GameObject> Objects;

        public bool PlayerInSector;

        public SectorManager()
        {
            instance = this;

            Objects = new List<GameObject>
            {
                GameObject.Find("ELEC_POLES"),
                GameObject.Find("ELEC_POLES_COLL"),
                GameObject.Find("TREES_MEDIUM3"),
                GameObject.Find("TREES_MEDIUM1"),
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
                GameObject.Find("MAP/LakeSimple/Tile")
            };

            CreateNewSector(new Vector3(-16.77627f, -0.5062422f, 1.559867f), new Vector3(5,5,9)); // Garage
            CreateNewSector(new Vector3(-1547, 4, 1183), new Vector3(7, 5, 8)); // Teimo
            CreateNewSector(new Vector3(1563.49f, 4.8f, 731.8835f), new Vector3(15, 5, 17), new Vector3(0, 335, 0)); // Repair shop
        }

        void CreateNewSector(Vector3 position, Vector3 size)
        {
            GameObject newSector = new GameObject("MOP_Sector");
            newSector.transform.position = position;
            Sector sectorInfo = newSector.AddComponent<Sector>();
            sectorInfo.Initialize(size);
        }

        void CreateNewSector(Vector3 position, Vector3 size, Vector3 rotation)
        {
            GameObject newSector = new GameObject("MOP_Sector");
            newSector.transform.position = position;
            newSector.transform.localEulerAngles = rotation;
            Sector sectorInfo = newSector.AddComponent<Sector>();
            sectorInfo.Initialize(size);
        }
    }
}
