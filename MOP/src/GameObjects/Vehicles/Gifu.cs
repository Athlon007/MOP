using UnityEngine;

namespace MOP
{
    class Gifu : Vehicle
    {
        // Gifu class - made by Konrad "Athlon" Figura
        // 
        // This class extends the functionality of Vehicle class, which is tailored for Gifu.
        // It fixes the issue with Gifu's beams being turned on after respawn.

        Transform knobs;
        Transform knobsParent;

        /// <summary>
        /// Initialize class
        /// </summary>
        /// <param name="gameObject"></param>
        public Gifu(string gameObjectName) : base(gameObjectName)
        {
            gifuScript = this;

            knobs = gameObject.transform.Find("Dashboard").Find("Knobs");
            knobsParent = knobs.parent;

            Toggle = ToggleActive;
        }

        /// <summary>
        /// Enable or disable car
        /// </summary>
        void ToggleActive(bool enabled)
        {
            if (gameObject == null) return;
            // Don't run the code, if the value is the same
            if (gameObject.activeSelf == enabled) return;

            // If we're disabling a car, set the audio child parent to TemporaryAudioParent, and save the position and rotation.
            // We're doing that BEFORE we disable the object.
            if (!enabled)
            {
                SetParentForChilds(AudioObjects, TemporaryParent);
                
                if (FuelTank != null)
                {
                    SetParentForChild(FuelTank, TemporaryParent);
                }

                SetParentForChild(knobs, TemporaryParent);

                Position = gameObject.transform.localPosition;
                Rotation = gameObject.transform.localRotation;
            }

            gameObject.SetActive(enabled);

            // Uppon enabling the file, set the localPosition and localRotation to the object's transform, and change audio source parents to Object
            // We're doing that AFTER we enable the object.
            if (enabled)
            {
                gameObject.transform.localPosition = Position;
                gameObject.transform.localRotation = Rotation;

                SetParentForChilds(AudioObjects, gameObject);

                if (FuelTank != null)
                {
                    SetParentForChild(FuelTank, gameObject);
                }

                SetParentForChild(knobs, knobsParent.gameObject);
            }
        }
    }
}
