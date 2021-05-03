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
                MSCLoader.ModConsole.LogError(ex.ToString());
            }
        }

        private IEnumerator currentFrameRateWait;
        IEnumerator FrameWait()
        { 
            while (MopSettings.IsModActive)
            {
                if (!fpsMesh.gameObject.transform.parent.gameObject.activeSelf) continue;
                samples.Add(float.Parse(fpsMesh.text));
                MSCLoader.ModConsole.Log(fpsMesh.text);
                yield return new WaitForSeconds(5);
            }
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
