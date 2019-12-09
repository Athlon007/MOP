using UnityEngine;
using MSCLoader;
using System.Collections;

namespace MOP
{
    class ObjectHook : MonoBehaviour
    {
        // This MonoBehaviour hooks to all minor objects.
        // ObjectHook class by Konrad "Athlon" Figura

        public GameObject gm => this.gameObject;

        /// <summary>
        /// PlayMakerFSM script of the object
        /// </summary>
        PlayMakerFSM playMakerFSM;

        // Saved rotation and position of the object, before it is disabled
        Vector3 position;
        Quaternion rotation;

        /// <summary>
        /// Rigidbody of this object.
        /// </summary>
        Rigidbody rb;

        /// <summary>
        /// Stores the current state of an object.
        /// </summary>
        bool currentState;

        /// <summary>
        /// Special fixes are applied for beer cases
        /// </summary>
        bool isBeerCase;

        public ObjectHook()
        {
            // Add self to the MinorObjects.objectHooks list
            Items.instance.Add(this);

            // Get the current rotation and position.
            position = gm.transform.position;
            rotation = gm.transform.rotation;

            // Get PlayMakerFSM
            playMakerFSM = gm.GetComponent<PlayMakerFSM>();

            // Get rigidbody
            rb = GetComponent<Rigidbody>();

            // From PlayMakerFSM, find states that contain one of the names that relate to destroying object,
            // and inject RemoveSelf void.
            foreach (var st in playMakerFSM.FsmStates)
            {
                switch (st.Name)
                {
                    case "Destroy self":
                        FsmHook.FsmInject(this.gameObject, "Destroy self", RemoveSelf);
                        break;
                    case "Destroy":
                        FsmHook.FsmInject(this.gameObject, "Destroy", RemoveSelf);
                        break;
                    case "Destroy 2":
                        FsmHook.FsmInject(this.gameObject, "Destroy 2", RemoveSelf);
                        break;
                    // These two are supposed to be attached to shopping bags. Currently doesn't work
                    // TODO: Fix this
                    /*
                    case "Confirm":
                        FsmHook.FsmInject(this.gameObject, "Confirm", RemoveSelf);
                        break;
                    case "Spawn all":
                        FsmHook.FsmInject(this.gameObject, "Spawn all", RemoveSelf);
                        break;
                    */
                }
            }

            isBeerCase = gm.name.Contains("beer case");
        }

        // Triggered before the object is destroyed.
        // Removes self from MinorObjects.instance.objectHooks.
        public void RemoveSelf()
        {
            Items.instance.Remove(this);
        }

        public void ToggleActive(bool enabled)
        {
            if (gm == null || this.currentState == enabled)
                return;

            currentState = enabled;

            // Beer cases are treated differently.
            // Instead of disabling entire object, change it's rigibdody values, to prevent them from landing under vehicles.
            // It fixes the problems with bottles in beer cases disappearing.
            if (isBeerCase)
            {
                // Fix for uncle's beer case
                if (rb == null)
                    return;

                rb.detectCollisions = enabled;
                rb.isKinematic = !enabled;

                // Fix for Carry More mod
                // Beer cases for some reason tend to have detectCollisions disabled uppon reenabling.
                // This code launches the BeerCasePositionFix() coroutine, in order to fix that, if the object is reenabled
                if (enabled)
                {
                    if (currentBeerCasePositionFix != null)
                        return;

                    currentBeerCasePositionFix = BeerCasePositionFix();
                    StartCoroutine(currentBeerCasePositionFix);
                }

                return;
            }

            // Fix for Carry More mod.
            // Carry More mod enables the Rigidbody.isKinematic value, when the object is added to the inventory.
            if (rb != null && rb.isKinematic)
                return;

            // Uppon disabling, save position and rotation
            if (!enabled)
            {
                position = gm.transform.position;
                rotation = gm.transform.rotation;
            }

            gm.SetActive(enabled);

            // Uppon reenabling, load position from saved value
            if (enabled)
            {
                // Only teleport object to the last known position, 
                // if the differenc between current and last known position is larger than 2 units.
                if (Vector3.Distance(gm.transform.position, position) < 2)
                    return;

                gm.transform.position = position;
                gm.transform.rotation = rotation;
            }
        }

        // Current instance of BeerCasePositionFix
        private IEnumerator currentBeerCasePositionFix;

        /// <summary>
        /// A fix for when the beer case may sometimes clip through the floor.
        /// If the distance between the saved position and the current position is greater than 2,
        /// teleport the object to the saved position.
        /// </summary>
        /// <returns></returns>
        IEnumerator BeerCasePositionFix()
        {
            Vector3 pos = gm.transform.position;
            Quaternion rot = gm.transform.rotation;

            yield return new WaitForSeconds(1);

            if (Vector3.Distance(pos, gm.transform.position) > 2)
            {
                pos.y += 3; // add 3 units to the Y value, to prevent the item from appearing in the same place again.
                gm.transform.position = pos;
                gm.transform.rotation = rot;
            }

            currentBeerCasePositionFix = null;
        }
    }
}
