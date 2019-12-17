using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MSCLoader;

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

            System.Random rnd = new System.Random();
            ToggledVehicles.AddRange(highwayChilds.OrderBy(x => rnd.Next()).Take(MopSettings.TrafficLimit));
        }

        public void Toggle(GameObject gm, bool enabled)
        {
            if (gm != null || gm.activeSelf == enabled) return;

            gm.SetActive(enabled);
        }
    }
}
