using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOP
{
    class Traffic
    {
        public List<GameObject> ToggledVehicles;

        public Traffic()
        {
            ToggledVehicles = new List<GameObject>();

            GameObject highwayTraffic = GameObject.Find("TRAFFIC");

            // If no traffic is supposed to be disabled
            if (MopSettings.TrafficLimit == -1)
            {
                ToggledVehicles.Add(highwayTraffic);
                return;
            }

            highwayTraffic = highwayTraffic.transform.Find("VehiclesHighway").gameObject;
            List<GameObject> highwayChilds = new List<GameObject>();
            for (int i = 0; i < highwayTraffic.transform.childCount; i++)
            {
                highwayChilds.Add(highwayTraffic.transform.GetChild(i).gameObject);
            }

            // Get random traffic cars from list
            System.Random rnd = new System.Random();
            int howManyvehicles = Mathf.CeilToInt(highwayChilds.Count * MopSettings.TrafficLimit);
            MSCLoader.ModConsole.Print(howManyvehicles);
            ToggledVehicles.AddRange(highwayChilds.OrderBy(x => rnd.Next()).Take(howManyvehicles));
        }

        public void ToggleActive(GameObject gm, bool enabled)
        {
            if (gm != null || gm.activeSelf == enabled) 
                return;

            gm.SetActive(enabled);
        }
    }
}
