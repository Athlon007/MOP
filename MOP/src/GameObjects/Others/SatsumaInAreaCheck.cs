using UnityEngine;

namespace MOP
{
    class SatsumaInAreaCheck : MonoBehaviour
    {
        // Attaches to gameobject. This script checks if the Satsuma is in the object's area,
        // and sends the info to it about it.

        BoxCollider collider;

        bool isInspection;
        
        bool inspectionPreventUnload;
        public bool InspectionPreventUnload { get => inspectionPreventUnload; }

        const string REFERENCE_ITEM = "gearbox";

        public void Initialize(Vector3 size)
        {
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
            collider.transform.position = transform.position;

            isInspection = gameObject.name == "INSPECTION";
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.StartsWith(REFERENCE_ITEM))
            {
                Satsuma.instance.IsSatsumaInInspectionArea = true;

                if (isInspection)
                    inspectionPreventUnload = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.StartsWith(REFERENCE_ITEM))
            {
                Satsuma.instance.IsSatsumaInInspectionArea = false;
            }
        }
    }
}
