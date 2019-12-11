using System;
using System.Collections;
using UnityEngine;

namespace MOP
{
    class OcclusionCamera : MonoBehaviour
    {
        void Start()
        {
            if (!MopSettings.EnableObjectOcclusion)
                return;

            //Camera camera = gameObject.GetComponent<Camera>();
            //camera.farClipPlane = MopSettings.ViewDistance;
            StartCoroutine(CheckView());
            MSCLoader.ModConsole.Print("OcclusionCamera initialized");
        }

        IEnumerator CheckView()
        {
            while (true)
            {
                int step = Math.Max(Screen.width, Screen.height) / MopSettings.OcclusionSamples;

                int xHalf = Screen.width / 2;
                int x;

                for (x = 0; x <= xHalf; x += step)
                {
                    int yHalf = Screen.height / 2;
                    int y;
                    for (y = 0; y <= yHalf; y += step)
                    {
                        Ray SampleRay = Camera.main.ScreenPointToRay(new Vector3(x, y, 0f));
                        RaycastHit hit;

                        if (Physics.Raycast(SampleRay, out hit, MopSettings.ViewDistance))
                        {
                            GameObject target = hit.transform.gameObject;
                            OcclusionBase thisTargetScript = target.GetComponent<OcclusionBase>();

                            while (thisTargetScript == null && target.transform.parent != null)
                            {
                                if (target.transform.parent != null)
                                {
                                    target = target.transform.parent.gameObject;

                                    thisTargetScript = target.GetComponent<OcclusionBase>();
                                };

                                if (thisTargetScript != null)
                                {
                                    thisTargetScript.lastSeen = DateTime.Now;
                                    thisTargetScript.isVisible = true;
                                }
                            }
                        }
                    }

                    yield return null;

                    for (; y <= Screen.height; y += step)
                    {
                        Ray SampleRay = Camera.main.ScreenPointToRay(new Vector3(x, y, 0f));
                        RaycastHit hit;

                        if (Physics.Raycast(SampleRay, out hit, MopSettings.ViewDistance))
                        {
                            GameObject target = hit.transform.gameObject;
                            OcclusionBase thisTargetScript = target.GetComponent<OcclusionBase>();

                            while (thisTargetScript == null && target.transform.parent != null)
                            {
                                if (target.transform.parent != null)
                                {
                                    target = target.transform.parent.gameObject;

                                    thisTargetScript = target.GetComponent<OcclusionBase>();
                                };

                                if (thisTargetScript != null)
                                {
                                    thisTargetScript.lastSeen = DateTime.Now;
                                    thisTargetScript.isVisible = true;
                                }
                            }
                        }
                    }
                }

                yield return null;

                for (; x <= Screen.width; x += step)
                {
                    int yHalf = Screen.height / 2;
                    int y;
                    for (y = 0; y <= yHalf; y += step)
                    {
                        Ray SampleRay = Camera.main.ScreenPointToRay(new Vector3(x, y, 0f));
                        RaycastHit hit;

                        if (Physics.Raycast(SampleRay, out hit, MopSettings.ViewDistance))
                        {
                            GameObject target = hit.transform.gameObject;
                            OcclusionBase thisTargetScript = target.GetComponent<OcclusionBase>();

                            while (thisTargetScript == null && target.transform.parent != null)
                            {
                                if (target.transform.parent != null)
                                {
                                    target = target.transform.parent.gameObject;

                                    thisTargetScript = target.GetComponent<OcclusionBase>();
                                };

                                if (thisTargetScript != null)
                                {
                                    thisTargetScript.lastSeen = DateTime.Now;
                                    thisTargetScript.isVisible = true;
                                }
                            }
                        }
                    }

                    yield return null;

                    for (; y <= Screen.height; y += step)
                    {
                        Ray SampleRay = Camera.main.ScreenPointToRay(new Vector3(x, y, 0f));
                        RaycastHit hit;

                        if (Physics.Raycast(SampleRay, out hit, MopSettings.ViewDistance))
                        {
                            GameObject target = hit.transform.gameObject;
                            OcclusionBase thisTargetScript = target.GetComponent<OcclusionBase>();

                            while (thisTargetScript == null && target.transform.parent != null)
                            {
                                if (target.transform.parent != null)
                                {
                                    target = target.transform.parent.gameObject;

                                    thisTargetScript = target.GetComponent<OcclusionBase>();
                                };

                                if (thisTargetScript != null)
                                {
                                    thisTargetScript.lastSeen = DateTime.Now;
                                    thisTargetScript.isVisible = true;
                                }
                            }
                        }
                    }
                }
                //yield return null;
            }
        }
    }
}
