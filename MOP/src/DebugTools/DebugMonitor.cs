using System;
using System.Text;
using UnityEngine;

using MOP.Managers;
using MOP.Vehicles.Cases;

namespace MOP.DebugTools
{
    class DebugMonitor : MonoBehaviour
    {
        // GUI
        private TextMesh fps;
        private TextMesh fpsShadow;
        private enum DebugPage { MopInfo, SatsumaInfo, PlayerInfo } 
        private DebugPage debugPage = DebugPage.MopInfo;
        
        // DATA
        private long lastMemoryUsage;
        private long[] differenceAverage = new long[128];
        private int differenceCounter;
        // SATSUMA
        private Transform satsuma, block, driverHeadPivot;
        private Vector3 lastSatsumaPosition, blockInitRot, driverHeadPivotRot; 

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

            satsuma = GameObject.Find("SATSUMA(557kg, 248)").transform;
            lastSatsumaPosition = satsuma.position;
            if (satsuma.Find("Chassis/sub frame(xxxxx)") != null)
            {
                if (satsuma.Find("Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)") != null)
                {
                    block = satsuma.Find("Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)");
                    blockInitRot = block.localEulerAngles;
                }
            }
            
            driverHeadPivot = satsuma.Find("DriverHeadPivot");
            driverHeadPivotRot = driverHeadPivot.localEulerAngles;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if ((int)(debugPage + 1) >= Enum.GetNames(typeof(DebugPage)).Length)
                {
                    debugPage = 0;
                }
                else
                {
                    debugPage += 1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (debugPage <= 0)
                {
                    debugPage = (DebugPage)(Enum.GetNames(typeof(DebugPage)).Length - 1);
                }
                else
                {
                    debugPage -= 1;
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<- ").Append(debugPage).AppendLine(" ->");
            
            switch (debugPage)
            {
                case DebugPage.MopInfo:
                    long gcUsage = GC.GetTotalMemory(false);
                    long averageDiff = CalculateAverageMemoryUsage(gcUsage);
                    sb.Append("<color=yellow>Tick</color> ").Append(Hypervisor.Instance.Tick).AppendLine();
                    sb.Append("<color=yellow>GC</color> ").Append(gcUsage).Append(" (").Append(averageDiff).AppendLine(")");
                    sb.Append("<color=yellow>Items</color> ").Append(ItemsManager.Instance.EnabledCount).Append(" / ").Append(ItemsManager.Instance.Count).AppendLine();
                    sb.Append("<color=yellow>Vehicles</color> ").Append(VehicleManager.Instance.EnabledCount).Append(" / ").Append(VehicleManager.Instance.Count).AppendLine();
                    sb.Append("<color=yellow>WorldObj</color> ").Append(WorldObjectManager.Instance.EnabledCount).Append(" / ").Append(WorldObjectManager.Instance.Count).AppendLine();
                    sb.Append("<color=yellow>Places</color> ").Append(PlaceManager.Instance.EnabledCount).Append(" / ").Append(PlaceManager.Instance.Count).AppendLine();
                    sb.Append("<color=yellow>Sectors</color> ").Append(SectorManager.Instance.EnabledCount).Append(" / ").Append(SectorManager.Instance.Count).AppendLine();
                    break;
                case DebugPage.SatsumaInfo:
                    float satsumaVelocity = (lastSatsumaPosition - satsuma.position).magnitude / Time.deltaTime;
                    lastSatsumaPosition = satsuma.position;
                    sb.Append("<color=yellow>Velocity</color> ").Append(satsumaVelocity).AppendLine();
                    if (block != null)
                    {
                        sb.Append("<color=yellow>BlockRot</color> ").Append(Difference(blockInitRot, block.localEulerAngles)).AppendLine();
                    }
                    sb.Append("<color=yellow>InspectionArea</color> ").Append(Satsuma.Instance.IsSatsumaInInspectionArea).AppendLine();
                    sb.Append("<color=yellow>ParcFerme</color> ").Append(Satsuma.Instance.IsSatsumaInParcFerme).AppendLine();
                    sb.Append("<color=yellow>DriverHeadPivotRot</color> ").Append(Difference(driverHeadPivotRot, driverHeadPivot.localEulerAngles)).AppendLine();
                    break;
                case DebugPage.PlayerInfo:
                    sb.Append("<color=yellow>PlayerPos</color> ").Append(Hypervisor.Instance.GetPlayer().position).AppendLine();
                    sb.Append("<color=yellow>InSector</color> ").Append(Hypervisor.Instance.IsInSector()).AppendLine();
                    sb.Append("<color=yellow>SectorDrawDistance</color> ")
                        .Append(SectorManager.Instance.IsPlayerInSector() ? SectorManager.Instance.GetCurrentSectorDrawDistance().ToString() : "-").AppendLine();
                    break;
            }                 

            SetDebugGuiText(sb.ToString());
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

        private void OnDestroy()
        {
            Destroy(fpsShadow?.gameObject);
            Destroy(fps?.gameObject);
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
