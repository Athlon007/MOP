using UnityEngine;
using MSCLoader;
using System.Collections;

namespace MOP
{
    class ItemHook : MonoBehaviour
    {
        // This MonoBehaviour hooks to all minor objects.
        // ObjectHook class by Konrad "Athlon" Figura

        public GameObject gm;

        /// <summary>
        /// PlayMakerFSM script of the object
        /// </summary>
        PlayMakerFSM playMakerFSM;

        // Saved rotation and position of the object, before it is disabled
        Vector3 lastPosition;
        Quaternion lastRotation;

        /// <summary>
        /// Rigidbody of this object.
        /// </summary>
        Rigidbody rb;

        /// <summary>
        /// Object's renderer
        /// </summary>
        Renderer renderer;

        public ItemHook()
        {
            gm = this.gameObject;

            // Add self to the MinorObjects.objectHooks list
            Items.instance.Add(this);

            // Get the current rotation and position.
            lastPosition = gm.transform.position;
            lastRotation = gm.transform.rotation;

            // Get object's components
            rb = GetComponent<Rigidbody>();
            playMakerFSM = GetComponent<PlayMakerFSM>();
            renderer = GetComponent<Renderer>();

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
                }
            }

            // Object is plastic can
            if (gm.name.ContainsAny("kilju", "empty plastic can", "juice"))
            {
                FsmHook.FsmInject(this.gameObject, "State 3", RemoveSelf);
            }

            // If the item is a shopping bag, hook the RemoveSelf to "Is garbage" FsmState
            if (gm.name.Contains("shopping bag"))
            {
                FsmHook.FsmInject(this.gameObject, "Is garbage", RemoveSelf);
            }
        }

        // Triggered before the object is destroyed.
        // Removes self from MinorObjects.instance.objectHooks.
        public void RemoveSelf()
        {
            Items.instance.Remove(this);
        }

        /// <summary>
        /// Doesn't toggle object itself, rather the Rigidbody and Renderer.
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleActive(bool enabled)
        {
            try
            {
                if (rb == null || rb.detectCollisions == enabled)
                    return;

                // If enabled, activate the position fixer (for CarryMore mod)
                if (enabled)
                {
                    if (currentPositionFix != null)
                        return;

                    currentPositionFix = PositionFix();
                    StartCoroutine(currentPositionFix);
                }

                rb.detectCollisions = enabled;
                rb.isKinematic = !enabled;

                // If occlusion culling is not enabled, disable object's renderer on distance.
                if (!MopSettings.EnableObjectOcclusion && renderer != null)
                {
                    renderer.enabled = enabled;
                }
            }
            catch (System.Exception ex)
            {
                ErrorHandler.New(ex);
            }
        }

        // Current instance of BeerCasePositionFix
        private IEnumerator currentPositionFix;

        /// <summary>
        /// A fix for when the beer case may sometimes clip through the floor.
        /// If the distance between the saved position and the current position is greater than 2,
        /// teleport the object to the saved position.
        /// </summary>
        /// <returns></returns>
        IEnumerator PositionFix()
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

            currentPositionFix = null;
        }
    }
}
