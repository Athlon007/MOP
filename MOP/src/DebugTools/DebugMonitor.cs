using System;
using System.Linq;
using UnityEngine;

using MOP.Items;
using MOP.Managers;
using MOP.Vehicles;

namespace MOP.DebugTools
{
    class DebugMonitor : MonoBehaviour
    {
        // GUI
        private TextMesh fps;
        private TextMesh fpsShadow;
        
        // DATA
        private long lastMemoryUsage = 0;
        private long[] differenceAverage = new long[128];
        private int differenceCounter = 0;

        private void Start()
        {
            GameObject fpsObject = GameObject.Instantiate(GameObject.Find("GUI").transform.Find("HUD/FPS/HUDValue").gameObject);
            fpsObject.transform.localPosition = new Vector3(fpsObject.transform.position.x, fpsObject.transform.position.y + 10, fpsObject.transform.position.z);
            PlayMakerFSM[] fsms = fpsObject.GetComponents<PlayMakerFSM>();
            foreach (var fsm in fsms)
            {
                fsm.enabled = false;
            }

            fps = fpsObject.GetComponent<TextMesh>();
            fpsShadow = fpsObject.transform.Find("HUDValueShadow").GetComponent<TextMesh>();
        }

        private void Update()
        {
            long gcUsage = GC.GetTotalMemory(false);
            long averageDiff = CalculateAverageMemoryUsage(gcUsage);

            string text = $"<color=yellow>Tick</color> {Hypervisor.Instance.Tick}\n" +
                          $"<color=yellow>GC</color> {gcUsage} ({averageDiff})\n" +
                          $"<color=yellow>Items</color> {CalculateEnabledItems()} / {ItemsManager.Instance.Count}\n" +
                          $"<color=yellow>Vehicles</color> {CalculateEnabledVehicles()} / {VehicleManager.Instance.Count}\n" +
                          $"<color=yellow>World Obj</color> {CalculateEnabledWorldObjects()} / {WorldObjectManager.Instance.Count}\n" +
                          $"<color=yellow>Places</color> {CalculateEnabledPlaces()} / {PlaceManager.Instance.Count}";
            SetDebugGuiText(text);
        }

        private long CalculateAverageMemoryUsage(long memoryUsage)
        {
            long diff = memoryUsage - lastMemoryUsage;
            differenceAverage[differenceCounter] = diff;
            differenceCounter++;
            if (differenceCounter >= differenceAverage.Length) differenceCounter = 0;

            long averageDiff = 0;
            int divBy = 0;
            for (int i = 0; i < differenceAverage.Length; ++i)
            {
                if (differenceAverage[i] <= 0) continue;
                averageDiff += differenceAverage[i];
                divBy++;
            }

            averageDiff /= divBy;
            lastMemoryUsage = averageDiff;
            return averageDiff;
        }

        private void SetDebugGuiText(string text)
        {
            fps.text = text;
            fpsShadow.text = text.Replace("<color=yellow>", "").Replace("</color>", "");
        }

        private int CalculateEnabledItems()
        {
            int i = 0;
            foreach (ItemBehaviour item in ItemsManager.Instance.GetList)
            {
                if (item.ActiveSelf)
                {
                    i++;
                }
            }

            return i;
        }

        private int CalculateEnabledVehicles()
        {
            int i = 0;
            foreach (Vehicle item in VehicleManager.Instance.GetList)
            {
                if (item.gameObject.activeSelf)
                {
                    i++;
                }
            }

            return i;
        }

        private int CalculateEnabledPlaces()
        {
            return PlaceManager.Instance.GetList().Where(g => g.isActive).Count();
        }

        private int CalculateEnabledWorldObjects()
        {
            return WorldObjectManager.Instance.GetList().Where(g => g.GameObject.activeSelf).Count();
        }

        private void OnDestroy()
        {
            Destroy(fpsShadow.gameObject);
            Destroy(fps.gameObject);
        }
    }
}
