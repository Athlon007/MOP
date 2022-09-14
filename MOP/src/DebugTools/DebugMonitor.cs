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
        private long lastMemoryUsage;
        private long[] differenceAverage = new long[128];
        private int differenceCounter;
        // SATSUMA
        private Transform subFrame, carMotorPivot, block;
        private Vector3 subFrameInitRot, carMotorPivotInitRot, blockInitRot; 

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

            if (GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Chassis/sub frame(xxxxx)") != null)
            {
                subFrame = GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Chassis/sub frame(xxxxx)");
                carMotorPivot = GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Chassis/sub frame(xxxxx)/CarMotorPivot");
                if (GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)") != null)
                {
                    block = GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)");
                    blockInitRot = block.localEulerAngles;
                }

                subFrameInitRot = subFrame.localEulerAngles;
                carMotorPivotInitRot = carMotorPivot.localEulerAngles;
            }
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
                          $"<color=yellow>Places</color> {CalculateEnabledPlaces()} / {PlaceManager.Instance.Count}\n\n" +
                          $"<color=yellow>DIFFERENCES</color>\n" +
                          $"<color=yellow>SubFrame</color> {Difference(subFrameInitRot, subFrame.localEulerAngles)}\n" +
                          $"<color=yellow>CarMotorPivot</color> {Difference(carMotorPivotInitRot, carMotorPivot.localEulerAngles)}\n";
            if (block != null)
            {
                text += $"<color=yellow>Block</color> {Difference(blockInitRot, block.localEulerAngles)}\n";
            }
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

        private Vector3 Difference(Vector3 a, Vector3 b)
        {
            float x = Mathf.Abs(a.x - b.x);
            float y = Mathf.Abs(a.y - b.y);
            float z = Mathf.Abs(a.z - b.z);

            return new Vector3(x, y, z);
        }
    }
}
