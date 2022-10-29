using System.Linq;
using UnityEngine;

using MOP.Common;
using MOP.Rules;
using MOP.Rules.Types;

namespace MOP.LOD
{
    internal class LodObject
    {
        private readonly GameObject lodObject;

        public LodObject(GameObject prototype)
        {
            // Do not create the LOD object, if "no_lod" flag was set for it.
            if (RulesManager.Instance.GetList<NoLod>().FirstOrDefault(r => r.ObjectName == prototype.name) != null)
            {
                return;
            }

            lodObject = (GameObject)Object.Instantiate(prototype, new Vector3(0, -100, 0), Quaternion.Euler(0,0,0));
            lodObject.name = "MOP_Dumb-" + prototype.name;
            lodObject.transform.SetParent(Hypervisor.Instance.DumbObjectParent);

            RemoveLogic();
            lodObject.SetActive(false);
        }

        private void RemoveLogic()
        {
            // Remove FSMs.
            foreach (var fsm in lodObject.GetComponentsInChildren<PlayMakerFSM>(true))
            {
                Object.Destroy(fsm);
            }

            // Remove any mono behaviours.
            foreach (var fsm in lodObject.GetComponentsInChildren<MonoBehaviour>(true))
            {
                Object.Destroy(fsm);
            }

            // Remove colliders.
            foreach (var collider in lodObject.GetComponentsInChildren<Collider>(true))
            {
                Object.Destroy(collider);
            }

            foreach (var joint in lodObject.GetComponentsInChildren<Joint>(true))
            {
                Object.Destroy(joint);
            }

            // Rigidbodies.
            foreach (var rb in lodObject.GetComponentsInChildren<Rigidbody>(true))
            {
                Object.Destroy(rb);
            }

            // Audio sources.
            foreach (var audio in lodObject.GetComponentsInChildren<AudioSource>(true))
            {
                Object.Destroy(audio);
            }

            // Audio sources.
            foreach (var animation in lodObject.GetComponentsInChildren<Animation>(true))
            {
                Object.Destroy(animation);
            }

            // Remove axles and rigidbody objects from the root.
            Axles axle = lodObject.GetComponent<Axles>();
            if (axle)
            {
                Object.Destroy(axle);
            }

            Rigidbody rigidbody = lodObject.GetComponent<Rigidbody>();
            if (rigidbody)
            {
                Object.Destroy(rigidbody);
            }

            DestroyMaskedElements();


            // Remove useleess objects.
            DestroyGameObjects("LOD", "FuelTank", "DeadBody", "CoG", "Hose",
                "Dashboard", "Simulation", "ShitTank", "audio", "StagingWheel",
                "TrafficTrigger", "HookRear", "HookFront", "RadioPivot",
                "IKTarget", "PistonIK", "PlayerTrigger", "CarSimulation",
                "Interior", "Body/car body(xxxxx)/shadow_body", "MiscParts",
                "Electricity", "Wiring", "Wipers", "Colliders", "Chassis",
                "Sounds", "Bottle", "Body/Windshield", "Body/rear_windows",
                "Body/wiper_base", "Body/body masse(xxxxx)", "Body/cowl_parts",
                "Odometer", "HydraulicCylinder", "Automatic", "MESH/panel",
                "MESH/panel 1", "MESH/muscle_Scoop");

            // Rename all children - to not confuse other mods and MOP.
            // Also delete the ones that are useless.
            foreach (Transform t in lodObject.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject == lodObject)
                {
                    continue;
                }

                if (!t.gameObject.activeSelf || (t.gameObject.GetComponent<Renderer>() == null && t.childCount == 0))
                {
                    GameObject.Destroy(t.gameObject);
                    continue;
                }

                t.gameObject.name = $"DUMMY_{t.gameObject.name}";
            }

            DisableShadowCasting();
        }

        /// <summary>
        /// If it's meant to be enabled, it will teleport to the object it inherits from, and enable itself.
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="transform"></param>
        public void ToggleActive(bool enabled, Transform transform)
        {
            if (lodObject == null)
            {
                return;
            }

            if (enabled == lodObject.activeSelf)
            {
                return;
            }
            lodObject.SetActive(enabled);
            if (!enabled)
            {
                return;
            }
            lodObject.transform.position = transform.position;
            lodObject.transform.eulerAngles = transform.eulerAngles;
        }

        private void DestroyGameObjects(params string[] names)
        {
            foreach (string objectName in names)
            {
                Transform t = lodObject.transform.Find(objectName);
                if (t != null)
                {
                    GameObject.Destroy(t.gameObject);
                }
            }
        }

        private void DisableShadowCasting()
        {
            if (MopSettings.Mode >= Common.Enumerations.PerformanceMode.Balanced) return;

            foreach (var mesh in lodObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            foreach (var projector in lodObject.GetComponentsInChildren<Projector>(true))
            {
                Object.Destroy(projector);
            }
        }

        /// <summary>
        /// For Satsuma. Destroy masked crap.
        /// </summary>
        private void DestroyMaskedElements()
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.transform.root == lodObject.transform && g.name.StartsWith("Masked"));
            foreach (GameObject obj in objects)
            {
                GameObject.Destroy(obj);
            }
        }

        /// <summary>
        /// Destroys this dummy car.
        /// </summary>
        public void Destroy()
        {
            if (lodObject != null)
            {
                GameObject.Destroy(lodObject);
            }
        }
    }
}
