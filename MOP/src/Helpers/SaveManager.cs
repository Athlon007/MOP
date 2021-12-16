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
        readonly static ES2Settings setting = new ES2Settings();

        /// <summary>
        /// For some reason, the save files get marked as read only files, not allowing MSC to save the game.
        /// This script is ran in PreSaveGame() script and removes ReadOnly attribute.
        /// </summary>
        public static void RemoveReadOnlyAttribute()
        {
            try
            {
                RemoveAttribute(SavePath);
                RemoveAttribute(ItemsPath);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "ATTRIBUTES_REMOVAL_ERROR");
            }
        }

        static void RemoveAttribute(string filename)
        {
            if (File.Exists(filename))
            {
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);
            }
        }

        static bool ReadBoolean(string tag)
        {
            return ES2.Load<bool>($"{SavePath}?tag={tag}", setting);
        }

        static Transform ReadTransform(string tag)
        {
            return ES2.Load<Transform>($"{SavePath}?tag={tag}", setting);
        }

        static float ReadFloat(string tag)
        {
            return ES2.Load<float>($"{SavePath}?tag={tag}", setting);
        }

        static List<string> ReadStringList(string tag)
        {
            return ES2.LoadList<string>($"{SavePath}?tag={tag}", setting);
        }

        static int ReadInt(string tag)
        {
            return ES2.Load<int>($"{SavePath}?tag={tag}", setting);
        }

        static void WriteSavePath<T>(string tag, T value)
        {
            ES2.Save(value, $"{SavePath}?tag={tag}");
        }

        public static void SaveToDefault<T>(string tag, T value)
        {
            WriteSavePath<T>(tag, value);
        }

        public static void SaveToItem<T>(string tag, T value)
        {
            ES2.Save(value, $"{ItemsPath}?tag={tag}");
        }

        public static void VerifySave()
        {
            if (!File.Exists(SavePath))
                return;

            // Passenger bucket seat.
            // Check if driver bucket seat is bought and check the same for passenger one.
            // If they do not match, fix it.
            saveBugs = new List<SaveBugs>();
            try
            {
                bool bucketPassengerSeat = ReadBoolean("bucket seat passenger(Clone)Purchased");
                bool bucketDriverSeat = ReadBoolean("bucket seat driver(Clone)Purchased");

                if (bucketDriverSeat != bucketPassengerSeat)
                {
                    saveBugs.Add(SaveBugs.New("Bucket Seats", "One bucket seat is present in the game world, while the other isn't - both should be in game world.", () =>
                    {
                        WriteSavePath("bucket seat passenger(Clone)Purchased", true);
                        WriteSavePath("bucket seat driver(Clone)Purchased", true);
                    }));
                }
            }
            catch (Exception e)
            {
                ExceptionManager.New(e, false, "VERIFY_SAVE_BUCKET_SEAT");
            }

            try
            {
                bool tractorTrailerAttached = ReadBoolean("TractorTrailerAttached");
                Transform flatbedTransform = ReadTransform("FlatbedTransform");
                Transform kekmetTransform = ReadTransform("TractorTransform");

                if (tractorTrailerAttached && Vector3.Distance(flatbedTransform.position, kekmetTransform.position) > 5.5f)
                {
                    saveBugs.Add(SaveBugs.New("Flatbed Trailer Attached", "Trailer and tractor are too far apart from each other - impossible for them to be attached.", () =>
                    {
                        WriteSavePath("TractorTrailerAttached", false);
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
                        bool bumperRearInstalled = ReadBoolean("bumper rear(Clone)Installed");
                        float bumperTightness = ReadFloat("Bumper_RearTightness");

                        if (bumperRearInstalled && bumperTightness != save.rearBumperTightness)
                        {
                            WriteSavePath("Bumper_RearTightness", save.rearBumperTightness);
                            WriteSavePath("Bumper_RearBolts", save.rearBumperBolts);
                        }

                        bool halfshaft_FLInstalled = ReadBoolean("halfshaft_flInstalled");
                        float halfshaft_FLTightness = ReadFloat("Halfshaft_FLTightness");
                        if (halfshaft_FLInstalled && halfshaft_FLTightness != save.halfshaft_FLTightness)
                        {
                            saveBugs.Add(SaveBugs.New("Halfshaft (FL) Missmateched Bolt Stages", "Bolt stages in Halfshaft (FL) aren't correct.", () =>
                            {
                                WriteSavePath("Halfshaft_FLTightness", save.halfshaft_FLTightness);
                                WriteSavePath("Halfshaft_FLBolts", save.halfshaft_FLBolts);
                            }));
                        }

                        bool halfshaft_FRInstalled = ReadBoolean("halfshaft_frInstalled");
                        float halfshaft_FRTightness = ReadFloat("Halfshaft_FRTightness");
                        if (halfshaft_FRInstalled && halfshaft_FRTightness != save.halfshaft_FRTightness)
                        {
                            saveBugs.Add(SaveBugs.New("Halfshaft (FR) Missmateched Bolt Stages", "Bolt stages in Halfshaft (FR) aren't correct.", () =>
                            {
                                WriteSavePath("Halfshaft_FRTightness", save.halfshaft_FRTightness);
                                WriteSavePath("Halfshaft_FRBolts", save.halfshaft_FRBolts);
                            }));
                        }

                        bool wiringBatteryMinusInstalled = ReadBoolean("battery_terminal_minus(xxxxx)Installed");
                        float wiringBatteryMinusTightness = ReadFloat("WiringBatteryMinusTightness");
                        if (wiringBatteryMinusInstalled && wiringBatteryMinusTightness != save.wiringBatteryMinusTightness)
                        {
                            saveBugs.Add(SaveBugs.New("Battery terminal minus bolt is not tightened.", "Incorrect bolt tightness of battery minus terminal.", () =>
                            {
                                WriteSavePath("WiringBatteryMinusTightness", save.wiringBatteryMinusTightness);
                                WriteSavePath("WiringBatteryMinusBolts", save.wiringBatteryMinusBolts);
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
                ModConsole.Log("[MOP] MOP hasn't found any problems with your save :)");
            }
        }

        static void ReadDetails(List<SaveBugs> saveBugs)
        {
            string problemReport = Path.Combine(Path.GetTempPath(), "mopproblemsdetail.txt");
            
            if (File.Exists(problemReport))
            {
                File.Delete(problemReport);            
            }

            StreamWriter writer = new StreamWriter(problemReport);
            foreach (SaveBugs bug in saveBugs)
            {
                writer.WriteLine($"{bug.BugName}:\n  {bug.Description}\n");
            }
            writer.Close();

            System.Diagnostics.Process.Start(problemReport);
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
            return ES2.Load<Transform>(SavePath + "?tag=radiator hose3(xxxxx)", setting);
        }

        static bool TagExists(string tag)
        {
            return ES2.Exists($"{SavePath}?tag={tag}");
        }

        internal static void AddSaveFlag()
        {
            if (TagExists("Bumper_RearTightness") && TagExists("Bumper_RearBolts"))
            {
                MopSaveData save = new MopSaveData
                {
                    version = MOP.ModVersion,
                    rearBumperTightness = ReadFloat("Bumper_RearTightness"),
                    rearBumperBolts = ReadStringList("Bumper_RearBolts"),
                    halfshaft_FLTightness = ReadFloat("Halfshaft_FLTightness"),
                    halfshaft_FLBolts = ReadStringList("Halfshaft_FLBolts"),
                    halfshaft_FRTightness = ReadFloat("Halfshaft_FRTightness"),
                    halfshaft_FRBolts = ReadStringList("Halfshaft_FRBolts"),
                    wiringBatteryMinusTightness = ReadFloat("WiringBatteryMinusTightness"),
                    wiringBatteryMinusBolts = ReadStringList("WiringBatteryMinusBolts")
                };
                ModSave.Save(mopSavePath, save);
            }
        }

        static bool SaveFileExists => File.Exists(mopSavePath + ".xml");

        internal static void ReleaseSave()
        {
            try
            {
                if (SaveFileExists)
                {
                    ModSave.Delete(mopSavePath);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SAVE_RELEASE_ERROR");
            }
        }

        public static bool IsSatsumaLoadedCompletely()
        {
            // Check if cylinder head is supposed to be installed, but for some reason its disabled, or not a part of Satsuma at all.
            GameObject satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            GameObject cylinderHead = Resources.FindObjectsOfTypeAll<GameObject>().First( g => g.name == "cylinder head(Clone)");
            GameObject block = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "block(Clone)");
            MopSaveData save = new MopSaveData();
            bool isCylinderHeadInstalled = ReadBoolean("cylinder head(Clone)Installed");
            bool isEngineBlockInstalled = ReadBoolean("block(Clone)Installed");

            if (!isEngineBlockInstalled)
            {
                if (isCylinderHeadInstalled && !cylinderHead.transform.Path().Contains("block(Clone)"))
                {
                    return false;
                }
            }
            else
            {
                if ((cylinderHead.gameObject.activeSelf == false || cylinderHead.transform.root != satsuma.transform) && isCylinderHeadInstalled)
                {
                    return false;
                }
            }

            Transform sparkPlug1Pivot = cylinderHead.transform.Find("pivot_sparkplug1");
            
            for (int i = 1; i < 1000; ++i)
            {
                if (ES2.Load<int?>(ItemsPath + $"?tag=spark plug{i}TriggerID") == null)
                {
                    break;
                }

                if (ReadInt($"spark plug{i}TriggerID") == 1 && ES2.Load<bool>(ItemsPath + $"?tag=spark plug{i}Installed") && sparkPlug1Pivot.childCount == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
