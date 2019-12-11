using System.Linq;
using UnityEngine;

namespace MOP
{
    class Place
    {
        // Place Class
        //
        // It is responsible for loading and unloading important to the game places, it is extended by other classes.
        //
        // NOTE: That script DOES NOT disable the store itself, rather some of its childrens.

        public GameObject Object { get; set; }

        // Objects from that whitelist will not be disabled
        // It is so to prevent from restock script and Teimo's bike routine not working
        internal string[] GameObjectBlackList;

        /// <summary>
        /// List of childs that are allowed to be disabled
        /// </summary>
        internal Transform[] DisableableChilds;

        public Transform transform => Object.transform;

        /// <summary>
        /// Saves what value has been last used, to prevent unnescesary launch of loop.
        /// </summary>
        internal bool lastValue = true;

        /// <summary>
        /// Initialize the Store class
        /// </summary>
        public Place(string placeName)
        {
            Object = GameObject.Find(placeName);
            Object.AddComponent<OcclusionObject>();
        }

        /// <summary>
        /// Enable or disable the place
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleActive(bool enabled)
        {
            if (lastValue == enabled) return;
            lastValue = enabled;

            // Load and unload only the objects that aren't on the whitelist.
            for (int i = 0; i < DisableableChilds.Length; i++)
            {
                DisableableChilds[i].gameObject.SetActive(enabled);
            }
        }

        /// <summary>
        /// Returns all childs of the object.
        /// </summary>
        /// <returns></returns>
        internal Transform[] GetDisableableChilds()
        {
            return Object.transform.GetComponentsInChildren<Transform>(true).Where(trans => !trans.gameObject.name.ContainsAny(GameObjectBlackList)).ToArray();
        }
    }
}
