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

using System.Collections;
using System.Linq;
using UnityEngine;

namespace MOP
{
    class GameFixes : MonoBehaviour
    {
        // This class fixes shit done by ToplessGun.
        // Also some caused by MOP itself...

        public static GameFixes Instance;

        public bool HoodFixDone { get; private set; }
        public bool RearBumperFixDone { get; private set; }

        public GameFixes()
        {
            Instance = this;
        }

        /// <summary>
        /// Fixes hood popping out of the car on game load.
        /// </summary>
        public void HoodFix(Transform hoodPivot, Transform batteryPivot, Transform batteryTrigger)
        {
            StartCoroutine(HoodFixCoroutine(hoodPivot, batteryPivot, batteryTrigger));
        }

        IEnumerator HoodFixCoroutine(Transform hoodPivot, Transform batteryPivot, Transform batteryTrigger)
        {
            yield return new WaitForSeconds(2);

            // Hood
            Transform hood = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "hood(Clone)").transform;
            CustomPlayMakerFixedUpdate hoodFixedUpdate = hood.gameObject.AddComponent<CustomPlayMakerFixedUpdate>();

            // Fiber Hood
            GameObject fiberHood = Resources.FindObjectsOfTypeAll<GameObject>()
                .First(obj => obj.name == "fiberglass hood(Clone)"
                && obj.GetComponent<PlayMakerFSM>() != null
                && obj.GetComponent<MeshCollider>() != null);

            int retries = 0;
            if (MopFsmManager.IsStockHoodBolted() && hood.parent != hoodPivot)
            {
                hood.gameObject.GetComponent<ItemHook>().enabled = false;
                hood.gameObject.SetActive(true);

                while (hood.parent != hoodPivot)
                {
                    // Satsuma got disabled while trying to fix the hood.
                    // Attempt to fix it later.
                    if (!hoodPivot.gameObject.activeSelf)
                    {
                        Satsuma.instance.AfterFirstEnable = false;
                        yield break;
                    }

                    MopFsmManager.ForceHoodAssemble();
                    yield return null;

                    // If 10 retries failed, quit the loop.
                    retries++;
                    if (retries == 10)
                    {
                        break;
                    }
                }
            }

            hoodFixedUpdate.StartFixedUpdate();

            if (fiberHood != null && MopFsmManager.IsFiberHoodBolted() && fiberHood.transform.parent != hoodPivot)
            {
                retries = 0;
                while (fiberHood.transform.parent != hoodPivot)
                {
                    // Satsuma got disabled while trying to fix the hood.
                    // Attempt to fix it later.
                    if (!hoodPivot.gameObject.activeSelf)
                    {
                        Satsuma.instance.AfterFirstEnable = false;
                        yield break;
                    }

                    MopFsmManager.ForceHoodAssemble();
                    yield return null;

                    // If 10 retries failed, quit the loop.
                    retries++;
                    if (retries == 60)
                    {
                        break;
                    }
                }
            }

            hood.gameObject.AddComponent<SatsumaBoltsAntiReload>();
            fiberHood.gameObject.AddComponent<SatsumaBoltsAntiReload>();

            // Adds delayed initialization for hood hinge.
            if (hood.gameObject.GetComponent<DelayedHingeManager>() == null)
                hood.gameObject.AddComponent<DelayedHingeManager>();

            // Fix for hood not being able to be closed.
            if (hood.gameObject.GetComponent<SatsumaCustomHoodUse>() == null)
                hood.gameObject.AddComponent<SatsumaCustomHoodUse>();

            // Fix for battery popping out.
            if (MopFsmManager.IsBatteryInstalled() && batteryPivot.parent == null)
            {
                batteryTrigger.gameObject.SetActive(true);
                batteryTrigger.gameObject.GetComponent<PlayMakerFSM>().SendEvent("ASSEMBLE");
            }

            HoodFixDone = true;
            hood.gameObject.GetComponent<ItemHook>().enabled = true;
        }

        public void KekmetTrailerAttach()
        {
            Vehicle flatbed = WorldManager.instance.GetFlatbed();
            StartCoroutine(KekmetTrailerAttachCoroutine(flatbed));
        }

        IEnumerator KekmetTrailerAttachCoroutine(Vehicle flatbed)
        {
            while (!flatbed.gameObject.activeSelf)
            {
                yield return new WaitForSeconds(1f);
            }

            flatbed.transform.rotation = flatbed.Rotation;
            flatbed.transform.position = flatbed.Position;

            for (int i = 0; i < 10; i++)
            {
                PlayMakerFSM.BroadcastEvent("TRAILERATTACH");
                yield return new WaitForSeconds(.1f);
            }
        }

        public void RearBumperFix(GameObject triggerBumper, GameObject bumper)
        {
            StartCoroutine(RearBumperCoroutine(triggerBumper, bumper));
        }

        IEnumerator RearBumperCoroutine(GameObject triggerBumper, GameObject bumper)
        {
            yield return new WaitForSeconds(2);
            triggerBumper.GetComponent<PlayMakerFSM>().SendEvent("ASSEMBLE");

            bumper.GetComponent<ItemHook>().enabled = false;
            
            int childsNumber = bumper.transform.Find("Bolts").childCount;
            for (int i = 0; i < childsNumber; i++)
            {
                PlayMakerFSM fsm = bumper.transform.Find("Bolts").GetChild(i).gameObject.GetComponent<PlayMakerFSM>();
                fsm.FsmVariables.GetFsmInt("Stage").Value = 7;
                yield return null;
                fsm.SendEvent("ASSEMBLE");
                yield return null;
                fsm.SendEvent("TIGHTEN");
                yield return null;
                fsm.SendEvent("FINISHED");
            }

            bumper.GetComponent<ItemHook>().enabled = true;
            RearBumperFixDone = true;
        }
    }
}
