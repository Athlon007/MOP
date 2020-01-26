using System.Collections;
using System.Linq;
using UnityEngine;
using MSCLoader;

namespace MOP
{
    class CashRegisterHook : MonoBehaviour
    {
        // This MonoBehaviour hooks to CashRegister GameObject
        // CashRegisterHook class by Konrad "Athlon" Figura

        IEnumerator currentRoutine;

        public CashRegisterHook()
        {
            FsmHook.FsmInject(this.gameObject, "Purchase", TriggerMinorObjectRefresh);
            TriggerMinorObjectRefresh();
        }

        /// <summary>
        /// Starts the PurchaseCoroutine
        /// </summary>
        public void TriggerMinorObjectRefresh()
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            currentRoutine = PurchaseCoroutine();
            StartCoroutine(currentRoutine);
        }

        /// <summary>
        /// Injects the newly bought store items.
        /// </summary>
        /// <returns></returns>
        IEnumerator PurchaseCoroutine()
        {
            // Wait for few seconds to let all objects to spawn, and then inject the objects.
            yield return new WaitForSeconds(2);
            // Find shopping bags in the list
            GameObject[] items = FindObjectsOfType<GameObject>()
                .Where(gm => gm.name.ContainsAny(Items.instance.blackList)
                && gm.name.ContainsAny("(itemx)", "(Clone)"))
                .ToArray();

            if (items.Length > 0)
            {
                int half = items.Length / 2;
                for (int i = 0; i < items.Length; i++)
                {
                    if (i == half)
                        yield return null;

                    // Object already ObjectHook attached? Ignore it.
                    if (items[i].GetComponent<ItemHook>() != null)
                        continue;

                    items[i].AddComponent<ItemHook>();

                    // Hook the TriggerMinorObjectRefresh to Confirm and Spawn all actions
                    if (items[i].name.Contains("shopping bag"))
                    {
                        FsmHook.FsmInject(items[i], "Confirm", TriggerMinorObjectRefresh);
                        FsmHook.FsmInject(items[i], "Spawn all", TriggerMinorObjectRefresh);
                    }
                }
            }
            currentRoutine = null;
        }
    }
}
