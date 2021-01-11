using UnityEngine;

using MOP.Common;
using MOP.Common.Enumerations;

namespace MOP.WorldObjects
{
    class SimpleObjectToggle : GenericObject
    {
        public SimpleObjectToggle(GameObject gameObject, DisableOn disableOn = DisableOn.Distance, int distance = 200) : base(gameObject, distance, disableOn)
        {
            return;
        }

        public override void Toggle(bool enabled)
        {
            if (DisableOn.HasFlag(DisableOn.IgnoreInQualityMode) && MopSettings.Mode == PerformanceMode.Quality)
            {
                enabled = true;
            }

            if (this.gameObject != null && this.gameObject.activeSelf != enabled)
            {
                this.gameObject.SetActive(enabled);
            }
        }
    }
}
