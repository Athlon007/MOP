using UnityEngine;

namespace MOP
{
    class WorldObject
    {
        // Objects class by Konrad "Athlon" Figura

        /// <summary>
        /// Game object that this instance of the class controls.
        /// </summary>
        public GameObject gameObject { get; private set; }

        /// <summary>
        /// How close player has to be to the object, in order to be enabled.
        /// </summary>
        public int Distance { get; private set; }

        /// <summary>
        /// If true, the RenderDistance is ignored. Instead of enabling GameObject by distance,
        /// it will be enabled when player is not in the house area.
        /// </summary>
        public bool AwayFromHouse { get; private set; }

        public Transform transform => gameObject.transform;

        Renderer renderer;

        /// <summary>
        /// Initializes the Objects instance.
        /// </summary>
        /// <param name="gameObjectName">Name of game object that this instance controls.</param>
        /// <param name="renderDistance">From how far should that object be enabled (default 200).</param>
        public WorldObject(string gameObjectName, int distance = 200, bool rendererOnly = false)
        {
            this.gameObject = GameObject.Find(gameObjectName);
            this.Distance = distance;

            if (this.gameObject.GetComponent<OcclusionObject>() == null)
            {
                this.gameObject.AddComponent<OcclusionObject>();
            }

            Renderer ren = this.gameObject.GetComponent<Renderer>();
            if (ren != null)
            {
                renderer = ren;
            }

            // If rendererOnly is truge, the Toggle will be set to ToggleMesh.
            if (rendererOnly)
            {
                Toggle = ToggleMesh;
            }
            else
            {
                Toggle = ToggleActive;
            }
        }

        /// <summary>
        /// Initializes the Objects instance.
        /// </summary>
        /// <param name="gameObjectName">Name of game object that this instance controls.</param>
        /// <param name="awayFromHouse">If true, the object will be enabled, when the player leaves the house area.</param>
        public WorldObject(string gameObjectName, bool awayFromHouse, bool rendererOnly = false)
        {
            this.gameObject = GameObject.Find(gameObjectName);
            this.AwayFromHouse = awayFromHouse;

            // If rendererOnly is truge, the Toggle will be set to ToggleMesh.
            if (rendererOnly)
            {
                Toggle = ToggleMesh;
            }
            else
            {
                Toggle = ToggleActive;
            }
        }

        public delegate void ToggleHandler(bool enabled);
        public ToggleHandler Toggle;

        /// <summary>
        /// Enable or disable the object.
        /// </summary>
        /// <param name="enabled"></param>
        void ToggleActive(bool enabled)
        {
            if (this.gameObject != null && this.gameObject.activeSelf != enabled)
                this.gameObject.SetActive(enabled);
        }

        void ToggleMesh(bool enabled)
        {
            if (renderer.enabled != enabled)
            {
                renderer.enabled = enabled;
            }
        }
    }
}
