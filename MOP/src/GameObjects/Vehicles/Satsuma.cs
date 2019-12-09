using UnityEngine;
using System.Linq;

namespace MOP
{
    class Satsuma : Vehicle
    {
        // Satsuma class - made by Konrad "Athlon" Figura
        // 
        // This class extends the functionality of Vehicle class, which is tailored for Gifu.
        // It fixes the issue with Gifu's beams being turned on after respawn.

        Transform[] disableableObjects;
        string[] whiteList;

        /// <summary>
        /// Initialize class
        /// </summary>
        /// <param name="gameObject"></param>
        public Satsuma(string gameObject) : base(gameObject)
        {
            satsumaScript = this;

            whiteList = Properties.Resources.whitelist_satsuma.Replace("\n", "").Split(',');
            disableableObjects = GetDisableableChilds();
        }

        /// <summary>
        /// Enable or disable car
        /// </summary>
        public new void ToggleActive(bool enabled)
        {
            // Don't run the code, if the value is the same
            if (gm == null || disableableObjects[0].gameObject.activeSelf == enabled) return;

            carDynamics.enabled = enabled;
            axles.enabled = enabled;
            rb.isKinematic = !enabled;

            for (int i = 0; i < disableableObjects.Length; i++)
            {
                disableableObjects[i].gameObject.SetActive(enabled);
            }
        } 

        /// <summary>
        /// Get list of disableable childs.
        /// It looks for objects that contian the name from the whiteList
        /// </summary>
        /// <returns></returns>
        internal Transform[] GetDisableableChilds()
        {
            return gm.GetComponentsInChildren<Transform>(true)
                .Where(trans => trans.gameObject.name.ContainsAny(whiteList)).ToArray();
        }
    }
}
