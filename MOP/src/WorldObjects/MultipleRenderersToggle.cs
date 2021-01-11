using System.Linq;
using UnityEngine;

using MOP.Common.Enumerations;

namespace MOP.WorldObjects
{
    class MultipleRenderersToggle : GenericObject
    {
        Renderer[] renderers;

        public MultipleRenderersToggle(GameObject gameObject, DisableOn disableOn = DisableOn.Distance, int distance = 200) : base(gameObject, distance, disableOn)
        {
            renderers = gameObject.GetComponentsInChildren<Renderer>(true).ToArray();

            if (renderers.Length == 0)
            {
                throw new System.Exception($"[MOP] (RenderersToggle) Couldn't find the renderers of {gameObject.name}");
            }
        }

        public override void Toggle(bool enabled)
        {
            if (renderers[0].enabled == enabled)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (!renderers[i].gameObject.activeSelf)
                {
                    continue;
                }

                renderers[i].enabled = enabled;
            }
        }
    }
}
