// Modern Optimization Plugin
// Copyright(C) 2019-2020 Athlon

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

using UnityEngine;
using MSCLoader;
using System.Collections;

namespace MOP
{
    class ItemHook : MonoBehaviour
    {
        // This MonoBehaviour hooks to all items from shop and other interactable ones. (Such as sausages, or beer cases)
        // ObjectHook class by Konrad "Athlon" Figura

        /// <summary>
        /// PlayMakerFSM script of the object
        /// </summary>
        PlayMakerFSM playMakerFSM;

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
            // Add self to the MinorObjects.objectHooks list
            Items.instance.Add(this);

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

            // If the item is a shopping bag, hook the RemoveSelf to "Is garbage" FsmState
            if (gameObject.name.Contains("shopping bag"))
            {
                FsmHook.FsmInject(this.gameObject, "Is garbage", RemoveSelf);
            }

            // If the item is beer case, hook the DestroyBeerBottles void uppon removing a bottle.
            if (gameObject.name.StartsWith("beer case"))
            {
                FsmHook.FsmInject(this.gameObject, "Remove bottle", DestroyBeerBottles);
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

                // Check if item is in CarryMore inventory.
                // If so, ignore that item.
                if (CompatibilityManager.instance.CarryMore && Vector3.Distance(transform.position, CompatibilityManager.instance.CarryMoreTempPosition) < 1) 
                    return;

                // CD Player Enhanced mod
                if (CompatibilityManager.instance.CDPlayerEnhanced)
                {
                    // Prevent CDs to clip through CD Case
                    if (this.gameObject.name == "cd(itemy)" && this.transform.parent != null && this.transform.parent.name == "PivotCD")
                        return;

                    // Prevent CDs from clipping through the Radio
                    if (this.gameObject.name == "cd(itemy)" && this.transform.parent != null && this.transform.parent.name == "cd_sled_pivot")
                        return;

                    // Prevents CD cases from clipping through the CD rack
                    if (this.gameObject.name.StartsWith("cd case") && this.transform.parent != null && this.transform.parent.name.StartsWith("cd_trigger"))
                        return;
                }

                rb.detectCollisions = enabled;
                rb.isKinematic = !enabled;

                // If occlusion culling is not enabled, disable object's renderer on distance.
                if (renderer != null)
                {
                    renderer.enabled = enabled;
                }
            }
            catch (System.Exception ex)
            {
                ErrorHandler.New(ex);
            }
        }

        /// <summary>
        /// Used when this item is a beer case.
        /// Starts the coroutine that initializes Items.DestroyEmptyBottles after 7 seconds.
        /// </summary>
        void DestroyBeerBottles()
        {
            // If the setting is not enabled, return.
            if (!MopSettings.RemoveEmptyBeerBottles)
                return;

            if (currentBeerBottleRoutine != null)
            {
                StopCoroutine(currentBeerBottleRoutine);
            }

            currentBeerBottleRoutine = BeerBottlesRoutine();
            StartCoroutine(currentBeerBottleRoutine);
        }

        IEnumerator currentBeerBottleRoutine;
        IEnumerator BeerBottlesRoutine()
        {
            yield return new WaitForSeconds(7);
            Items.instance.DestroyBeerBottles();
        }
    }
}
