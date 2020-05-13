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

using HutongGames.PlayMaker;
using MSCLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOP
{
    class SatsumaTrunk : MonoBehaviour
    {
        bool isDisabled;

        List<GameObject> trunkContent;
        Rigidbody rb;
        float currentMass;

        FsmBool storageOpen;
        Transform rearSeatPivot;

        public SatsumaTrunk()
        {
            gameObject.layer = 2;
        }

        public void Initialize(Vector3 position, Vector3 scale, GameObject triggerObject)
        {            
            gameObject.transform.parent = GameObject.Find("SATSUMA(557kg, 248)").transform;
            gameObject.transform.localPosition = position;
            gameObject.transform.localEulerAngles = Vector3.zero;
            gameObject.transform.localScale = scale;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = true;
            rb.mass = 0;
            
            trunkContent = new List<GameObject>();

            storageOpen = triggerObject.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Open");
            FsmHook.FsmInject(triggerObject, "Mouse off", OnBootAction);

            rearSeatPivot = GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Interior/pivot_seat_rear");
        }

        public void Initialize()
        {
            StartCoroutine(DelayedInitialization());
        }

        IEnumerator DelayedInitialization()
        {
            yield return new WaitForSeconds(3);
            OnBootAction();
        }

        void OnTriggerEnter(Collider other)
        {
            if (isDisabled) return;

            if (other.gameObject.GetComponent<ItemHook>() != null && !trunkContent.Contains(other.gameObject))
            {
                /*
                if (afterFirstLoad && !storageOpen.Value)
                {
                    Vector3 newItemPosition = other.gameObject.transform.position;
                    newItemPosition.y += 1;
                    other.gameObject.transform.position = newItemPosition;
                    other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    return;
                }
                */

                trunkContent.Add(other.gameObject);
                currentMass += other.gameObject.GetComponent<ItemHook>().GetMass();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (isDisabled) return;

            if (other.gameObject.GetComponent<ItemHook>() != null && trunkContent.Contains(other.gameObject))
            {
                trunkContent.Remove(other.gameObject);
                currentMass -= other.gameObject.GetComponent<ItemHook>().GetMass();
            }
        }

        void OnBootAction()
        {
            for (int i = 0; i < trunkContent.Count; i++)
            {
                trunkContent[i].GetComponent<ItemHook>().IsInHood = !storageOpen.Value;
                trunkContent[i].SetActive(storageOpen.Value);
                trunkContent[i].transform.parent = storageOpen.Value ? null : gameObject.transform;

                trunkContent[i].transform.localScale = new Vector3(1, 1, 1);
            }

            rb.mass = storageOpen.Value ? 0 : currentMass;
        }

        bool IsRearSeatAttached()
        {
            return rearSeatPivot.childCount > 0;
        }

        public void OnSaveStop()
        {
            isDisabled = true;

            foreach (GameObject gm in trunkContent)
            {
                gm.GetComponent<ItemHook>().IsInHood = false;
                gm.SetActive(true);
            }

            trunkContent = new List<GameObject>();
        }
    }
}
