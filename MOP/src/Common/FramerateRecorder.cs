// Modern Optimization Plugin
// Copyright(C) 2019-2022 Athlon

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
using System.Collections.Generic;
using UnityEngine;

namespace MOP.Common
{
    class FramerateRecorder : MonoBehaviour
    {
        static FramerateRecorder instance;
        public static FramerateRecorder Instance => instance;

        TextMesh fpsMesh;
        static List<float> samples;

        internal void Initialize()
        {
            try
            {
                instance = this;

                fpsMesh = GameObject.Find("GUI").transform.Find("HUD/FPS/HUDValue").GetComponent<TextMesh>();
                if (samples == null) samples = new List<float>();

                currentFrameRateWait = FrameWait();
                StartCoroutine(currentFrameRateWait);
            }
            catch (Exception ex)
            {
                ModConsole.LogError(ex.ToString());
            }
        }

        private IEnumerator currentFrameRateWait;
        IEnumerator FrameWait()
        { 
            while (MopSettings.IsModActive)
            {
                if (!fpsMesh.gameObject.transform.parent.gameObject.activeSelf) 
                {
                    yield return new WaitForSeconds(5);
                    continue; 
                }
                samples.Add(float.Parse(fpsMesh.text));
                yield return new WaitForSeconds(5);
            }

            Stop();
        }

        void Stop()
        {
            enabled = false;
        }

        internal float GetAverage()
        {
            if (samples.Count == 0)
            {
                return -1;
            }

            float x = 0;
            foreach (float f in samples)
                x += f;

            return Mathf.Round(x / samples.Count);
        }
    }
}
