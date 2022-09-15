using System;
using System.Linq;
using UnityEngine;

using MOP.Items;
using MOP.Managers;
using MOP.Vehicles;
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
        private Transform satsuma, block;
        private Vector3 lastSatsumaPosition, blockInitRot; 

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
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if ((int)(debugPage + 1) >= Enum.GetNames(typeof(DebugPage)).Length)
                {
                    debugPage = (DebugPage)0;
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

            string text = $"<- {debugPage} ->\n"; 
            
            switch (debugPage)
            {
                case DebugPage.MopInfo:
                    long gcUsage = GC.GetTotalMemory(false);
                    long averageDiff = CalculateAverageMemoryUsage(gcUsage);
                    text += $"<color=yellow>Tick</color> {Hypervisor.Instance.Tick}\n" +
                            $"<color=yellow>GC</color> {gcUsage} ({averageDiff})\n" +
                            $"<color=yellow>Items</color> {CalculateEnabledItems()} / {ItemsManager.Instance.Count}\n" +
                            $"<color=yellow>Vehicles</color> {CalculateEnabledVehicles()} / {VehicleManager.Instance.Count}\n" +
                            $"<color=yellow>World Obj</color> {CalculateEnabledWorldObjects()} / {WorldObjectManager.Instance.Count}\n" +
                            $"<color=yellow>Places</color> {CalculateEnabledPlaces()} / {PlaceManager.Instance.Count}\n";
                    break;
                case DebugPage.SatsumaInfo:
                    float satsumaVelocity = (lastSatsumaPosition - satsuma.position).magnitude / Time.deltaTime;
                    lastSatsumaPosition = satsuma.position;
                    text += $"<color=yellow>Velocity</color> {satsumaVelocity}\n";
                            if (block != null)
                            {
                                text += $"<color=yellow>Block_rot</color> {Difference(blockInitRot, block.localEulerAngles)}\n";
                            }
                    text += $"<color=yellow>inspection area</color> {Satsuma.Instance.IsSatsumaInInspectionArea}\n" +
                            $"<color=yellow>parcferme</color> {Satsuma.Instance.IsSatsumaInParcFerme}\n";
                    break;
                case DebugPage.PlayerInfo:
                    text += $"<color=yellow>PlayerPos</color> {Hypervisor.Instance.GetPlayer().position}\n" +
                            $"<color=yellow>InSector</color> {Hypervisor.Instance.IsInSector()}\n" +
                            $"<color=yellow>SectorDrawDistance</color> " +
                            $"{(SectorManager.Instance.IsPlayerInSector() ? SectorManager.Instance.GetCurrentSectorDrawDistance().ToString() : "-")}\n";
                    break;
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
