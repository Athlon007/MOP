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

            switch (MopSettings.OcclusionMethod)
            {
                default:
                    StartCoroutine(ChequeredView());
                    break;
                case 0:
                    StartCoroutine(CheckView());
                    break;
                case 2:
                    StartCoroutine(FirstView());
                    StartCoroutine(SecondView());
                    break;
            }

            StartCoroutine(ControlCoroutine());
            MSCLoader.ModConsole.Print("OcclusionCamera initialized");
        }

        IEnumerator FirstView()
        {
            while (MopSettings.IsModActive)
            {
                ticks += 1;
                if (ticks > 1000)
                    ticks = 0;

                int step = Math.Max(Screen.width, Screen.height) / MopSettings.OcclusionSamples;
                int x, y;

                for (x = 0; x <= Screen.width; x += step * 2)
                {
                    for (y = 0; y <= Screen.height; y += step * 2)
                    {
                        Ray sampleRay = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
                        RaycastHit hit;

                        if (Physics.Raycast(sampleRay, out hit, MopSettings.ViewDistance))
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
                }

                yield return null;
            }
        }

        IEnumerator SecondView()
        {
            while (MopSettings.IsModActive)
            {
                ticks += 1;
                if (ticks > 1000)
                    ticks = 0;

                int step = Math.Max(Screen.width, Screen.height) / MopSettings.OcclusionSamples;
                int x, y;

                for (x = step / 2; x <= Screen.width; x += step * 2)
                {
                    for (y = step / 2; y <= Screen.height; y += step * 2)
                    {
                        Ray sampleRay = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
                        RaycastHit hit;

                        if (Physics.Raycast(sampleRay, out hit, MopSettings.ViewDistance))
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
                }
            }
        }

        /// <summary>
        /// Instead of checking screen area lineary (0-1-2-3-4), it checs first even areas (0-2-4-6-8), and then odd areas (1-3-5-7-9)
        /// </summary>
        /// <returns></returns>
        IEnumerator ChequeredView()
        {
            while (MopSettings.IsModActive)
            {
                ticks += 1;
                if (ticks > 100)
                    ticks = 0;

                int step = Math.Max(Screen.width, Screen.height) / MopSettings.OcclusionSamples;
                int x, y;

                for (x = 0; x <= Screen.width; x += step * 2)
                {
                    for (y = 0; y <= Screen.height; y += step * 2)
                    {
                        Ray sampleRay = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
                        RaycastHit hit;

                        if (Physics.Raycast(sampleRay, out hit, MopSettings.ViewDistance))
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
                }

                yield return null;

                for (x = step / 2; x <= Screen.width; x += step * 2)
                {
                    for (y = step / 2; y <= Screen.height; y += step * 2)
                    {
                        Ray sampleRay = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
                        RaycastHit hit;

                        if (Physics.Raycast(sampleRay, out hit, MopSettings.ViewDistance))
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
                }
            }
        }

        /// <summary>
        /// Classic method of checking occlusion by lineary checking areas of the screen.
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckView()
        {
            while (true)
            {
                ticks += 1;
                if (ticks > 1000)
                    ticks = 0;

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
            }
        }

        int ticks;
        int lastTick;

        /// <summary>
        /// Every 10 seconds check if the coroutine is still active.
        /// If not, try to restart it.
        /// It is checked by two values - ticks and lastTick
        /// Ticks are added by coroutine. If the value is different than the lastTick, everything is okay.
        /// If the ticks and lastTick is the same, that means coroutine stopped.
        /// </summary>
        /// <returns></returns>
        IEnumerator ControlCoroutine()
        {
            while (MopSettings.IsModActive)
            {
                yield return new WaitForSeconds(10);

                if (lastTick == ticks)
                {
                    MSCLoader.ModConsole.Error("Occlusion loop stopped working. Trying to restart the now...\n" +
                        "If the issue will still occure, please turn on output_log.txt in MSC Mod Loader settings, " +
                        "and check output_log.txt in MSC folder.");

                    switch (MopSettings.OcclusionMethod)
                    {
                        default:
                            StartCoroutine(ChequeredView());
                            break;
                        case 0:
                            StartCoroutine(CheckView());
                            break;
                        case 2:
                            StartCoroutine(FirstView());
                            StartCoroutine(SecondView());
                            break;
                    }
                }

                lastTick = ticks;
            }
        }
    }
}
