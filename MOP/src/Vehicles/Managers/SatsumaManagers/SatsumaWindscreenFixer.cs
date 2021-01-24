using UnityEngine;

namespace MOP.Vehicles.Managers.SatsumaManagers
{
    class SatsumaWindscreenFixer : MonoBehaviour
    {
        bool fixWindscreen;
        PlayMakerFSM fsm;

        void Awake()
        {
            fsm = GetComponent<PlayMakerFSM>();
        }

        void OnEnable()
        {
            if (fixWindscreen)
            {
                fsm.SendEvent("REPAIR");
                fixWindscreen = false;
            }
        }

        internal void FixWindscreen()
        {
            fixWindscreen = true;
        }
    }
}
