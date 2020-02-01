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

using System;
using System.Collections;
using UnityEngine;
using System.Diagnostics;

namespace MOP
{
    class OcclusionCamera : MonoBehaviour
    {
        private bool isOcclusionHideDelayCalculated;
        private int calculationDelayStep = 0;
        private const int DelayEnd = 2;

        void Start()
        {
            if (!MopSettings.EnableObjectOcclusion)
                return;

            switch (MopSettings.OcclusionMethod)
            {
                default:
                    StartCoroutine(ChequeredView());
                    break;
                case OcclusionMethods.Double:
                    StartCoroutine(FirstView());
                    StartCoroutine(SecondView());
                    break;
            }

            StartCoroutine(ControlCoroutine());
        }

        /// <summary>
        /// Calculates how long it takes for the occlusion loop to end, and sets OcclusionHideDelay by this value
        /// </summary>
        /// <param name="elapsedTime"></param>
        void SetOcclusionHideDelayTime(long elapsedTime)
        {
            float toFloat = Mathf.Round(float.Parse((elapsedTime * 0.001).ToString()));
            int toInt = int.Parse(toFloat.ToString());
            MopSettings.OcclusionHideDelay = toInt;
        }

        IEnumerator FirstView()
        {
            while (MopSettings.IsModActive)
            {
                Stopwatch stopwatch = new Stopwatch();
                if (!isOcclusionHideDelayCalculated)
                {
                    calculationDelayStep += 1;
                    if (calculationDelayStep > DelayEnd)
                        stopwatch = Stopwatch.StartNew();
                }

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
                            try
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
                                        thisTargetScript.LastSeen = DateTime.Now;
                                        thisTargetScript.IsVisible = true;
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    yield return null;
                }

                yield return null;

                if (!isOcclusionHideDelayCalculated && calculationDelayStep > DelayEnd)
                {
                    stopwatch.Stop();
                    isOcclusionHideDelayCalculated = true;
                    long time = stopwatch.ElapsedMilliseconds * 2;
                    SetOcclusionHideDelayTime(time);
                }
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
                            try
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
                                        thisTargetScript.LastSeen = DateTime.Now;
                                        thisTargetScript.IsVisible = true;
                                    }
                                }
                            }
                            catch { }
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
                Stopwatch stopwatch = new Stopwatch();
                if (!isOcclusionHideDelayCalculated)
                {
                    calculationDelayStep += 1;
                    if (calculationDelayStep > DelayEnd)
                        stopwatch = Stopwatch.StartNew();
                }

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
                            try
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
                                        thisTargetScript.LastSeen = DateTime.Now;
                                        thisTargetScript.IsVisible = true;
                                    }
                                }
                            }
                            catch { }
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
                            try
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
                                        thisTargetScript.LastSeen = DateTime.Now;
                                        thisTargetScript.IsVisible = true;
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    yield return null;
                }

                if (!isOcclusionHideDelayCalculated && calculationDelayStep > DelayEnd)
                {
                    stopwatch.Stop();
                    isOcclusionHideDelayCalculated = true;
                    long time = stopwatch.ElapsedMilliseconds;
                    SetOcclusionHideDelayTime(time);
                }
            }
        }

        private int ticks;
        private int lastTick;

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
                        case OcclusionMethods.Double:
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
