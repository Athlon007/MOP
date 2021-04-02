using System.Collections;
using System.Linq;
using UnityEngine;

using MOP.Managers;
using MOP.Common.Enumerations;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaTriggerFixer : MonoBehaviour
    {
        // Basically waits couple of seconds and checks if the pivot attached to the trigger is empty.
        // If yes, enable trigger.

        static SatsumaTriggerFixer instance;
        public static SatsumaTriggerFixer Instance => instance;

        void Start()
        {
            instance = this;

            Transform body = VehicleManager.Instance.GetVehicle(VehiclesTypes.Satsuma).transform.Find("Body");
            foreach (var t in body.GetComponentsInChildren<Transform>().Where(g => g.gameObject.name.StartsWith("trigger_")))
            {
                t.gameObject.AddComponent<SatsumaTrigger>();
            }

            Transform block = GameObject.Find("block(Clone)").transform;
            foreach (var t in block.GetComponentsInChildren<Transform>().Where(g => g.gameObject.name.StartsWith("trigger_")))
            {
                t.gameObject.AddComponent<SatsumaTrigger>();
            }
        }

        internal void Check(SatsumaTrigger trigger)
        {
            StartCoroutine(CheckTriggerChild(trigger));
        }

        IEnumerator CheckTriggerChild(SatsumaTrigger trigger)
        {
            yield return new WaitForSeconds(2);

            if (trigger.Pivot.childCount == 0)
            {
                trigger.gameObject.SetActive(true);
            }
        }
    }

    class SatsumaTrigger : MonoBehaviour
    {
        Transform pivot;
        public Transform Pivot { get => pivot; }

        void Awake()
        {
            pivot = GetComponent<PlayMakerFSM>().FsmVariables.FindFsmGameObject("Parent").Value.transform;

            if (pivot == null)
            {
                MSCLoader.ModConsole.Error($"[MOP] Can't find pivot of \"{gameObject.name}\"!");
                Destroy(this);
            }
        }

        void OnDisable()
        {
            if (pivot != null)
            {
                SatsumaTriggerFixer.Instance.Check(this);
            }
        }
    }
}
