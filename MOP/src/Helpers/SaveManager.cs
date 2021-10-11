// Modern Optimization Plugin
// Copyright(C) 2019-2021 Athlon

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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using MSCLoader;

using MOP.Common;

namespace MOP.Helpers
{
    class SaveManager
    {
        public static string SavePath => Path.Combine(Application.persistentDataPath, "defaultES2File.txt").Replace('\\', '/');
        public static string ItemsPath => Application.persistentDataPath + "/items.txt";
        static List<SaveBugs> saveBugs;

        static string mopSavePath => Path.Combine(Application.persistentDataPath, "MopSave");
        static ES2Settings setting = new ES2Settings();

        /// <summary>
        /// For some reason, the save files get marked as read only files, not allowing MSC to save the game.
        /// This script is ran in PreSaveGame() script and removes ReadOnly attribute.
        /// </summary>
        public static void RemoveReadOnlyAttribute()
        {
            if (File.Exists(SavePath))
                File.SetAttributes(SavePath, File.GetAttributes(SavePath) & ~FileAttributes.ReadOnly);

            if (File.Exists(ItemsPath))
                File.SetAttributes(ItemsPath, File.GetAttributes(ItemsPath) & ~FileAttributes.ReadOnly);
        }

        public static void VerifySave()
        {
            if (!File.Exists(SavePath))
                return;

            // Passenger bucket seat.
            // Check if driver bucket seat is bought and check the same for passenger one.
            // If they do not match, fix it.
            try
            {
                saveBugs = new List<SaveBugs>();

                bool bucketPassengerSeat = ES2.Load<bool>(SavePath + "?tag=bucket seat passenger(Clone)Purchased", setting);
                bool bucketDriverSeat = ES2.Load<bool>(SavePath + "?tag=bucket seat driver(Clone)Purchased", setting);
                if (bucketDriverSeat != bucketPassengerSeat)
                {
                    saveBugs.Add(SaveBugs.New("Bucket Seats", "One bucket seat is present in the game world, while the other isn't - both should be in game world.", () =>
                    {
                        ES2.Save(true, SavePath + "?tag=bucket seat passenger(Clone)Purchased");
                        ES2.Save(true, SavePath + "?tag=bucket seat driver(Clone)Purchased");
                    }));
                }
            }
            catch (Exception e)
            {
                ExceptionManager.New(e, false, "VERIFY_SAVE_BUCKET_SEAT");
            }

            try
            {
                bool tractorTrailerAttached = ES2.Load<bool>(SavePath + "?tag=TractorTrailerAttached", setting);
                Transform flatbedTransform = ES2.Load<Transform>(SavePath + "?tag=FlatbedTransform", setting);
                Transform kekmetTransform = ES2.Load<Transform>(SavePath + "?tag=TractorTransform", setting);
                if (tractorTrailerAttached && Vector3.Distance(flatbedTransform.position, kekmetTransform.position) > 5.5f)
                {
                    saveBugs.Add(SaveBugs.New("Flatbed Trailer Attached", "Trailer and tractor are too far apart from each other - impossible for them to be attached.", () =>
                    {
                        ES2.Save(false, SavePath + "?tag=TractorTrailerAttached", new ES2Settings());
                    }));
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VERIFY_SAVE_FLATBED");
            }

            try
            {
                // This one applies fix quietly, as it happens so often,
                // it would be annoying to nag player about that error.
                if (SaveFileExists)
                {
                    MopSaveData save = ModSave.Load<MopSaveData>(mopSavePath);

                    if (save.version == MOP.ModVersion)
                    {
                        bool bumperRearInstalled = ES2.Load<bool>(SavePath + "?tag=bumper rear(Clone)Installed", setting);
                        float bumperTightness = ES2.Load<float>(SavePath + "?tag=Bumper_RearTightness", setting);
                        if (bumperRearInstalled && bumperTightness != save.rearBumperTightness)
                        {
                            ES2.Save(save.rearBumperTightness, SavePath + "?tag=Bumper_RearTightness");
                            ES2.Save(save.rearBumperBolts, SavePath + "?tag=Bumper_RearBolts");
                        }

                        bool halfshaft_FLInstalled = ES2.Load<bool>(SavePath + "?tag=halfshaft_flInstalled", setting);
                        float halfshaft_FLTightness = ES2.Load<float>(SavePath + "?tag=Halfshaft_FLTightness", setting);
                        if (halfshaft_FLInstalled && halfshaft_FLTightness != save.halfshaft_FLTightness)
                        {
                            saveBugs.Add(SaveBugs.New("Halfshaft (FL) Missmateched Bolt Stages", "Bolt stages in Halfshaft (FL) aren't correct.", () =>
                            {
                                ES2.Save(save.halfshaft_FLTightness, SavePath + "?tag=Halfshaft_FLTightness");
                                ES2.Save(save.halfshaft_FLBolts, SavePath + "?tag=Halfshaft_FLBolts");
                            }));
                        }

                        bool halfshaft_FRInstalled = ES2.Load<bool>(SavePath + "?tag=halfshaft_frInstalled", setting);
                        float halfshaft_FRTightness = ES2.Load<float>(SavePath + "?tag=Halfshaft_FRTightness", setting);
                        if (halfshaft_FRInstalled && halfshaft_FRTightness != save.halfshaft_FRTightness)
                        {
                            saveBugs.Add(SaveBugs.New("Halfshaft (FR) Missmateched Bolt Stages", "Bolt stages in Halfshaft (FR) aren't correct.", () =>
                            {
                                ES2.Save(save.halfshaft_FRTightness, SavePath + "?tag=Halfshaft_FRTightness");
                                ES2.Save(save.halfshaft_FRBolts, SavePath + "?tag=Halfshaft_FRBolts");
                            }));
                        }

                        bool wiringBatteryMinusInstalled = ES2.Load<bool>(SavePath + "?tag=battery_terminal_minus(xxxxx)Installed", setting);
                        float wiringBatteryMinusTightness = ES2.Load<float>(SavePath + "?tag=WiringBatteryMinusTightness", setting);
                        if (wiringBatteryMinusInstalled && wiringBatteryMinusTightness != save.wiringBatteryMinusTightness)
                        {
                            saveBugs.Add(SaveBugs.New("Battery terminal minus bolt is not tightened.", "Incorrect bolt tightness of battery minus terminal.", () =>
                            {
                                ES2.Save(save.wiringBatteryMinusTightness, SavePath + "?tag=WiringBatteryMinusTightness");
                                ES2.Save(save.wiringBatteryMinusBolts, SavePath + "?tag=WiringBatteryMinusBolts");
                            }));
                        }
                    }
                    else
                    {
                        ReleaseSave();
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VERIFY_BUMPER_REAR");
            }

            if (saveBugs.Count > 0)
            {
                ModPrompt.CreateYesNoPrompt($"MOP found <color=yellow>{saveBugs.Count}</color> problem{(saveBugs.Count > 1 ? "s" : "")} with your save:\n\n" +
                                            $"<color=yellow>{string.Join(", ", saveBugs.Select(f => f.BugName).ToArray())}</color>\n\n" +
                                            $"Would you like MOP to try and fix {((saveBugs.Count > 1) ? "them" : "it")}?", "MOP - Save Integrity Verification", FixAllProblems);
            }
            else
            {
                ModConsole.Log("[MOP] MOP didn't find any problems with your save :)");
            }
        }

        static void FixAllProblems()
        {
            int success = 0, fail = 0;

            foreach (SaveBugs bug in saveBugs)
            {
                try
                {
                    bug.Fix.Invoke();
                    success++;
                }
                catch
                {
                    fail++;
                }
            }

            string msg = $"<color=yellow>{success}</color> issue{(success > 1 ? "s have" : " has")} been fixed.";
            if (fail > 0)
            {
                msg += $"\n<color=red>{fail}</color> issue{(fail > 1 ? "s" : "")} couldn't be fixed.";
            }

            ModPrompt.CreatePrompt(msg, "MOP - Save Integrity Check");
        }

        internal static Transform GetRadiatorHose3Transform()
        {
            ES2Settings settings = new ES2Settings();
            return ES2.Load<Transform>(SavePath + "?tag=radiator hose3(xxxxx)");
        }

        internal static void AddSaveFlag()
        {
            if (ES2.Exists(SavePath + "?tag=Bumper_RearTightness") && ES2.Exists(SavePath + "?tag=Bumper_RearBolts", setting))
            {
                MopSaveData save = new MopSaveData();

                save.version = MOP.ModVersion;

                save.rearBumperTightness = ES2.Load<float>(SavePath + "?tag=Bumper_RearTightness", setting);
                save.rearBumperBolts = ES2.LoadList<string>(SavePath + "?tag=Bumper_RearBolts", setting);

                save.halfshaft_FLTightness = ES2.Load<float>(SavePath + "?tag=Halfshaft_FLTightness", setting);
                save.halfshaft_FLBolts = ES2.LoadList<string>(SavePath + "?tag=Halfshaft_FLBolts", setting);

                save.halfshaft_FRTightness = ES2.Load<float>(SavePath + "?tag=Halfshaft_FRTightness", setting);
                save.halfshaft_FRBolts = ES2.LoadList<string>(SavePath + "?tag=Halfshaft_FRBolts", setting);

                save.wiringBatteryMinusTightness = ES2.Load<float>(SavePath + "?tag=WiringBatteryMinusTightness", setting);
                save.wiringBatteryMinusBolts = ES2.LoadList<string>(SavePath + "?tag=WiringBatteryMinusBolts", setting);

                ModSave.Save(mopSavePath, save);
            }
        }

        static bool SaveFileExists => File.Exists(mopSavePath + ".xml");

        internal static void ReleaseSave()
        {
            if (SaveFileExists)
                ModSave.Delete(mopSavePath);
        }

        internal static void SaveToItem<T>(string tag, T value)
        {
            ES2.Save<T>(value, ItemsPath + "?tag=" + tag, new ES2Settings());
        }

        internal static void SaveToDefault<T>(string tag, T value)
        {
            ES2.Save<T>(value, SavePath + "?tag=" + tag, new ES2Settings());
        }

        public static bool IsSatsumaLoadedCompletely()
        {
            // Check if cylinder head is supposed to be installed, but for some reason its disabled, or not a part of Satsuma at all.
            GameObject satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            GameObject cylinderHead = Resources.FindObjectsOfTypeAll<GameObject>().First( g => g.name == "cylinder head(Clone)");
            MopSaveData save = new MopSaveData();
            bool isCylinderHeadInstalled = ES2.Load<bool>(SavePath + "?tag=cylinder head(Clone)Installed", setting);

            if ((cylinderHead.gameObject.activeSelf == false || cylinderHead.transform.root != satsuma.transform) && isCylinderHeadInstalled)
            {
                return false;
            }

            Transform sparkPlug1Pivot = cylinderHead.transform.Find("pivot_sparkplug1");
            
            for (int i = 1; i < 1000; ++i)
            {
                if (ES2.Load<int?>(ItemsPath + $"?tag=spark plug{i}TriggerID") == null)
                {
                    break;
                }

                if (ES2.Load<int>(ItemsPath + $"?tag=spark plug{i}TriggerID") == 1 && ES2.Load<bool>(ItemsPath + $"?tag=spark plug{i}Installed"))
                {
                    if (sparkPlug1Pivot.childCount == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    struct SaveBugs
    {
        public string BugName;
        public string Description;
        public UnityAction Fix;

        public static SaveBugs New(string bugName, string description, UnityAction fix)
        {
            SaveBugs bug = new SaveBugs();
            bug.BugName = bugName;
            bug.Description = description;
            bug.Fix = fix;
            return bug;
        }
    }

    public class MopSaveData
    {
        public string version = "1.0";

        public float rearBumperTightness;
        public List<string> rearBumperBolts;

        public float halfshaft_FLTightness;
        public List<string> halfshaft_FLBolts;

        public float halfshaft_FRTightness;
        public List<string> halfshaft_FRBolts;

        public float wiringBatteryMinusTightness;
        public List<string> wiringBatteryMinusBolts;
    }
}
