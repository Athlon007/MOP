using UnityEngine;

namespace MOP
{
    class Gifu : Vehicle
    {
        // Gifu class - made by Konrad "Athlon" Figura
        // 
        // This class extends the functionality of Vehicle class, which is tailored for Gifu.
        // It fixes the issue with Gifu's beams being turned on after respawn.

        Transform BeaconSwitchParent;
        Transform BeaconSwitch;

        Transform BeaconsParent;
        Transform Beacons;
        Transform WorkLightsSwitch;

        /// <summary>
        /// Initialize class
        /// </summary>
        /// <param name="gameObject"></param>
        public Gifu(string gameObject) : base(gameObject)
        {
            gifuScript = this;

            BeaconSwitchParent = gm.transform.Find("Dashboard").Find("Knobs");
            BeaconsParent = gm.transform.Find("LOD");

            BeaconSwitch = BeaconSwitchParent.transform.Find("KnobBeacon");
            Beacons = BeaconsParent.transform.Find("Beacon");
            WorkLightsSwitch = BeaconSwitchParent.transform.Find("KnobWorkLights");
        }

        /// <summary>
        /// Enable or disable car
        /// </summary>
        public new void ToggleActive(bool enabled)
        {
            if (gm == null) return;
            // Don't run the code, if the value is the same
            if (gm.activeSelf == enabled) return;

            // If we're disabling a car, set the audio child parent to TemporaryAudioParent, and save the position and rotation.
            // We're doing that BEFORE we disable the object.
            if (!enabled)
            {
                SetParentForChilds(AudioObjects, TemporaryParent);
                if (FuelTank != null)
                {
                    SetParentForChild(FuelTank, TemporaryParent);
                }
                SetParentForChild(BeaconSwitch, TemporaryParent);
                SetParentForChild(Beacons, TemporaryParent);
                SetParentForChild(WorkLightsSwitch, TemporaryParent);

                Position = gm.transform.localPosition;
                Rotation = gm.transform.localRotation;
            }

            gm.SetActive(enabled);

            // Uppon enabling the file, set the localPosition and localRotation to the object's transform, and change audio source parents to Object
            // We're doing that AFTER we enable the object.
            if (enabled)
            {
                gm.transform.localPosition = Position;
                gm.transform.localRotation = Rotation;

                SetParentForChilds(AudioObjects, gm);
                if (FuelTank != null)
                {
                    SetParentForChild(FuelTank, gm);
                }
                SetParentForChild(BeaconSwitch, BeaconSwitchParent.gameObject);
                SetParentForChild(Beacons, BeaconsParent.gameObject);
                SetParentForChild(WorkLightsSwitch, BeaconSwitchParent.gameObject);
            }
        }
    }
}
