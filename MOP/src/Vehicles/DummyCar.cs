using MOP.Common;
using System.Linq;
using UnityEngine;

namespace MOP.Vehicles
{
    internal class DummyCar
    {
        private readonly GameObject dumbCar;

        public DummyCar(GameObject vehicle)
        {
            dumbCar = GameObject.Instantiate(vehicle);
            dumbCar.name = "MOP_Dumb" + vehicle.name;
            dumbCar.transform.position = new Vector3(0, -100, 0);

            CleanDumbCarOfCrap();
            dumbCar.SetActive(false);
        }

        private void CleanDumbCarOfCrap()
        {
            // Remove FSMs.
            foreach (var fsm in dumbCar.GetComponentsInChildren<PlayMakerFSM>(true))
            {
                Object.Destroy(fsm);
            }

            // Remove any mono behaviours.
            foreach (var fsm in dumbCar.GetComponentsInChildren<MonoBehaviour>(true))
            {
                Object.Destroy(fsm);
            }

            // Remove colliders.
            foreach (var collider in dumbCar.GetComponentsInChildren<Collider>(true))
            {
                Object.Destroy(collider);
            }

            foreach (var joint in dumbCar.GetComponentsInChildren<Joint>(true))
            {
                Object.Destroy(joint);
            }

            // Rigidbodies.
            foreach (var rb in dumbCar.GetComponentsInChildren<Rigidbody>(true))
            {
                Object.Destroy(rb);
            }

            // Audio sources.
            foreach (var audio in dumbCar.GetComponentsInChildren<AudioSource>(true))
            {
                Object.Destroy(audio);
            }

            // Audio sources.
            foreach (var animation in dumbCar.GetComponentsInChildren<Animation>(true))
            {
                Object.Destroy(animation);
            }

            // Remove axles and rigidbody objects from the root.
            Axles axle = dumbCar.GetComponent<Axles>();
            if (axle)
            {
                Object.Destroy(axle);
            }

            Rigidbody rigidbody = dumbCar.GetComponent<Rigidbody>();
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
                "Sounds");

            // Rename all children - to not confuse other mods and MOP.
            // Also delete the ones that are useless.
            foreach (Transform t in dumbCar.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject == dumbCar)
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

        public void ToggleActive(bool enabled, Transform transform)
        {
            dumbCar.SetActive(enabled);
            dumbCar.transform.position = transform.position;
            dumbCar.transform.eulerAngles = transform.eulerAngles;
        }

        private void DestroyGameObjects(params string[] names)
        {
            foreach (string objectName in names)
            {
                Transform t = dumbCar.transform.Find(objectName);
                if (t != null)
                {
                    GameObject.Destroy(t.gameObject);
                }
            }
        }

        private void DisableShadowCasting()
        {
            if (MopSettings.Mode == Common.Enumerations.PerformanceMode.Quality) return;

            foreach (var mesh in dumbCar.GetComponentsInChildren<MeshRenderer>(true))
            {
                mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        /// <summary>
        /// For Satsuma. Destroy masked crap.
        /// </summary>
        private void DestroyMaskedElements()
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.transform.root == dumbCar.transform && g.name.StartsWith("Masked"));
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
            if (dumbCar != null)
            {
                GameObject.Destroy(dumbCar);
            }
        }
    }
}
