using UnityEngine;

using MOP.Common.Enumerations;

namespace MOP.WorldObjects
{
    class RendererToggle : GenericObject
    {
        Renderer renderer;

        public RendererToggle(GameObject gameObject, DisableOn disableOn, int distance = 200) : base(gameObject, distance, disableOn)
        {
            renderer = this.gameObject.GetComponent<Renderer>();

            if (renderer == null)
            {
                throw new System.Exception($"[MOP] Couldn't find the renderer of {gameObject.name}");
            }
        }

        public override void Toggle(bool enabled)
        {
            if (!renderer.gameObject.activeSelf || renderer.enabled == enabled)
            {
                return;
            }

            renderer.enabled = enabled;
        }
    }
}
