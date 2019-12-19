using UnityEngine;

namespace MOP
{
    class SatsumaInAreaCheck : MonoBehaviour
    {
        // Attaches to gameobject. This script checks if the Satsuma is in the object's area,
        // and sends the info to it about it.

        GameObject gm;
        BoxCollider collider;

        bool isInspection;
        
        bool inspectionPreventUnload;
        public bool InspectionPreventUnload { get => inspectionPreventUnload; }

        public void Initialize(Vector3 size)
        {
            this.gm = this.gameObject;
            collider = gm.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
            collider.transform.position = gm.transform.position;

            isInspection = gm.name == "INSPECTION";
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.StartsWith("gearbox"))
            {
                Satsuma.instance.IsSatsumaInInspectionArea = true;

                if (isInspection)
                    inspectionPreventUnload = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.StartsWith("gearbox"))
            {
                Satsuma.instance.IsSatsumaInInspectionArea = false;
            }
        }
    }
}
