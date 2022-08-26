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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MSCLoader;
using Newtonsoft.Json;

using MOP.Common;

namespace MOP.Helpers
{
    class SaveManager
    {
        public static string SavePath => Path.Combine(Application.persistentDataPath, "defaultES2File.txt").Replace('\\', '/');
        public static string ItemsPath => Path.Combine(Application.persistentDataPath, "items.txt").Replace("\\", "/");
        public static string OptionsPath => Path.Combine(Application.persistentDataPath, "options.txt").Replace("\\", "/");
        static List<SaveBugs> saveBugs;

        readonly static ES2Settings setting = new ES2Settings();

        // MOP Save Files.
        static string mopSavePath => Path.Combine(Application.persistentDataPath, "MopSave.json");
        static bool SaveFileExists => File.Exists(mopSavePath);

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
            string path = $"{SavePath}?tag={tag}";
            if (!ES2.Exists(path, setting))
            {
                throw new NullReferenceException($"Boolean '{tag}' is not present in save file.");
            }

            return ES2.Load<bool>(path, setting);
        }

        public static Transform ReadTransform(string tag)
        {
            string path = $"{SavePath}?tag={tag}";
            if (!ES2.Exists(path, setting))
            {
                throw new NullReferenceException($"Transform '{tag}' is not present in save file.");
            }

            return ES2.Load<Transform>(path, setting);
        }

        public static Transform ReadItemTranform(string tag)
        {
            string path = $"{ItemsPath}?tag={tag}";
            if (!ES2.Exists(path, setting))
            {
                throw new NullReferenceException($"Transform '{tag}' is not present in item save file.");
            }

            return ES2.Load<Transform>(path, setting);
        }

        static float ReadFloat(string tag)
        {
            string path = $"{SavePath}?tag={tag}";
            if (!ES2.Exists(path, setting))
            {
                throw new NullReferenceException($"Float '{tag}' is not present in save file.");
            }

            return ES2.Load<float>(path, setting);
        }

        static List<string> ReadStringList(string tag)
        {
            string path = $"{SavePath}?tag={tag}";
            if (!ES2.Exists(path, setting))
            {
                throw new NullReferenceException($"List<String> '{tag}' is not present in the save file.");
            }

            return ES2.LoadList<string>(path, setting);
        }

        static int ReadInt(string tag)
        {
            string path = $"{SavePath}?tag={tag}";
            if (!ES2.Exists(path, setting))
            {
                throw new NullReferenceException($"Int '{tag}' is not present in save file.");
            }

            return ES2.Load<int>(path, setting);
        }

        static int ReadItemInt(string tag)
        {
            string path = $"{ItemsPath}?tag={tag}";
            if (!ES2.Exists(path, setting))
            {
                throw new NullReferenceException($"Int '{tag}' is not present in item save file.");
            }

            return ES2.Load<int>(path, setting);
        }

        static void WriteSavePath<T>(string tag, T value)
        {
            ES2.Save(value, $"{SavePath}?tag={tag}", setting);
        }

        public static void SaveToDefault<T>(string tag, T value)
        {
            WriteSavePath<T>(tag, value);
        }

        public static void SaveToItem<T>(string tag, T value)
        {
            ES2.Save(value, $"{ItemsPath}?tag={tag}");
        }

        public static bool IsSaveTagPresent(string tag)
        {
            return ES2.Exists($"{SavePath}?tag={tag}");
        }

        public static bool IsItemTagPresent(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return false;
            }

            return ES2.Exists($"{ItemsPath}?tag={tag}");
        }

        public static void VerifySave()
        {
            if (!File.Exists(SavePath))
            {
                return;
            }

            if (IsSaveFileAfterPermadeath())
            {
                ModConsole.Log($"[MOP] Save file wasn't checked, as it is post-permadeath.");
                return;
            }

            RemoveReadOnlyAttribute();

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
                    saveBugs.Add(SaveBugs.New("Bucket Seats", () =>
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
                if (!IsSaveTagPresent("TractorTrailerAttached"))
                {
                    ModConsole.Log("[MOP] Save Verification: TractorTrailerAttached hasn't been found. Transform fix will be skipped...");
                }
                else
                { 
                    bool tractorTrailerAttached = ReadBoolean("TractorTrailerAttached");
                    Transform flatbedTransform = ReadTransform("FlatbedTransform");
                    Transform kekmetTransform = ReadTransform("TractorTransform");

                    if (tractorTrailerAttached && Vector3.Distance(flatbedTransform.position, kekmetTransform.position) > 5.5f)
                    {
                        saveBugs.Add(SaveBugs.New("Flatbed Trailer Attached", () =>
                        {
                            WriteSavePath("TractorTrailerAttached", false);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VERIFY_SAVE_FLATBED");
            }

            MopSaveFileData save = ReadData();
            if (save != null && save.version == MOP.ModVersion)
            {
                try
                {
                    // This one applies fix quietly, as it happens so often,
                    // it would be annoying to nag player about that error.
                    bool bumperRearInstalled = ReadBoolean("bumper rear(Clone)Installed");
                    float bumperTightness = ReadFloat("Bumper_RearTightness");
                    if (bumperRearInstalled && bumperTightness != save.rearBumperTightness)
                    {
                        WriteSavePath("Bumper_RearTightness", save.rearBumperTightness);
                        WriteSavePath("Bumper_RearBolts", save.rearBumperBolts);
                    }
                }
                catch (Exception ex)
                {
                    //ExceptionManager.New(ex, false, "VERIFY_BUMPER_REAR");
                }

                try
                {
                    if (!IsSaveTagPresent("Halfshaft_FLTightness"))
                    {
                        ModConsole.Log("[MOP] Save Verification: Halfshaft_FLTightness hasn't been found. Halfshaft FR mismatch won't be checked...");
                    }
                    else
                    {
                        bool halfshaft_FLInstalled = ReadBoolean("halfshaft_flInstalled");
                        float halfshaft_FLTightness = ReadFloat("Halfshaft_FLTightness");
                        if (halfshaft_FLInstalled && halfshaft_FLTightness != save.halfshaft_FLTightness)
                        {
                            saveBugs.Add(SaveBugs.New("Halfshaft (FL) Mismateched Bolt Stages", () =>
                            {
                                WriteSavePath("Halfshaft_FLTightness", save.halfshaft_FLTightness);
                                WriteSavePath("Halfshaft_FLBolts", save.halfshaft_FLBolts);
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "VERIFY_HALFSHAFT_FL");
                }

                try 
                {
                    if (!IsSaveTagPresent("Halfshaft_FRTightness"))
                    {
                        ModConsole.Log("[MOP] Save Verification: Halfshaft_FRTightness hasn't been found. Halfshaft FR mismatch won't be checked...");
                    }
                    else
                    {
                        bool halfshaft_FRInstalled = ReadBoolean("halfshaft_frInstalled");
                        float halfshaft_FRTightness = ReadFloat("Halfshaft_FRTightness");
                        if (halfshaft_FRInstalled && halfshaft_FRTightness != save.halfshaft_FRTightness)
                        {
                            saveBugs.Add(SaveBugs.New("Halfshaft (FR) Mismateched Bolt Stages", () =>
                            {
                                WriteSavePath("Halfshaft_FRTightness", save.halfshaft_FRTightness);
                                WriteSavePath("Halfshaft_FRBolts", save.halfshaft_FRBolts);
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "VERIFY_HALFSHAFT_FR");
                }

                try
                {
                    if (!IsSaveTagPresent("WiringBatteryMinusTightness"))
                    {
                        ModConsole.Log("[MOP] Save Verification: WiringBatteryMinusTightness hasn't been found. Battery terminal minus tightness won't be checked...");
                    }
                    else
                    {
                        bool wiringBatteryMinusInstalled = ReadBoolean("battery_terminal_minus(xxxxx)Installed");
                        float wiringBatteryMinusTightness = ReadFloat("WiringBatteryMinusTightness");
                        if (wiringBatteryMinusInstalled && wiringBatteryMinusTightness != save.wiringBatteryMinusTightness)
                        {
                            saveBugs.Add(SaveBugs.New("Battery terminal minus bolt is not tightened.", () =>
                            {
                                WriteSavePath("WiringBatteryMinusTightness", save.wiringBatteryMinusTightness);
                                WriteSavePath("WiringBatteryMinusBolts", save.wiringBatteryMinusBolts);
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "VERIFY_BATTERY_TERMINAL_MINUS");
                }
            }
            else
            {
                ReleaseSave();
            }

            try
            {
                // Fix fuel line tightness going below 0.
                List<string> fuelLineBolts = ReadStringList("FuelLineBolts");
                if (fuelLineBolts != null)
                {
                    float fuelLineTightness = ReadFloat("FuelLineTightness");

                    int boltOneValue = int.Parse(fuelLineBolts[0].Replace("int(", "").Replace(")", ""));

                    if (boltOneValue != fuelLineTightness)
                    {
                        saveBugs.Add(SaveBugs.New($"Fuel Line tightness is not a correct value." +
                                                  $"\n\nValue is <b>{fuelLineTightness}</b>.\n<b>{boltOneValue}</b> is expected", () =>
                        {
                            WriteSavePath("FuelLineTightness", (float)boltOneValue);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VERIFY_FUELLINE_TIGHTNESS");
            }

            if (saveBugs.Count > 0)
            {
                ModUI.ShowYesNoMessage($"MOP has found <color=yellow>{saveBugs.Count}</color> problem{(saveBugs.Count > 1 ? "s" : "")} with your save:\n\n" +
                                       $"<color=yellow>{string.Join(", ", saveBugs.Select(f => f.BugName).ToArray())}</color>\n\n" +
                                       $"Would you like MOP to try and fix {((saveBugs.Count > 1) ? "them" : "it")}?", "MOP - Save Integrity Verification", FixAllProblems);
            }
            else
            {
                ModConsole.Log("[MOP] MOP hasn't found any problems with your save :)");
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

            ModUI.ShowMessage(msg, "MOP - Save Integrity Check");
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
                MopSaveFileData save = new MopSaveFileData
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
                WriteData(save);
            }
        }

        internal static void ReleaseSave()
        {
            try
            {
                if (SaveFileExists)
                {
                    File.Delete(mopSavePath);
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
            if (satsuma == null)
            {
                throw new NullReferenceException("Satsuma is missing");
            }

            GameObject cylinderHead = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "cylinder head(Clone)");
            if (cylinderHead == null)
            {
                throw new NullReferenceException("Cylinder head is missing");
            }

            GameObject block = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "block(Clone)");
            if (block == null)
            {
                throw new NullReferenceException("Block is missing");
            }

            MopSaveFileData save = new MopSaveFileData();
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
            if (sparkPlug1Pivot == null)
            {
                throw new NullReferenceException("Unable to locate pivot_sparkplug1");
            }

            for (int i = 1; i < 1000; ++i)
            {
                if (ES2.Load<int?>($"{ItemsPath}?tag=spark plug{i}TriggerID") == null)
                {
                    break;
                }

                // Check if the spark plug is missing or not.
                if (ReadItemInt($"spark plug{i}TriggerID") == 1 && ES2.Load<bool>($"{ItemsPath}?tag=spark plug{i}Installed") && sparkPlug1Pivot.childCount == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetMopSavePath()
        {
            return mopSavePath;
        }

        static MopSaveFileData ReadData()
        {
            MopSaveFileData mopSaveData = null;
            if (SaveFileExists)
            {
                StreamReader reader = new StreamReader(mopSavePath);
                string json = reader.ReadToEnd();
                reader.Close();

                mopSaveData = JsonConvert.DeserializeObject<MopSaveFileData>(json);
            }

            return mopSaveData;
        }

        static void WriteData(MopSaveFileData data)
        {
            string json = JsonConvert.SerializeObject(data);

            StreamWriter writer = new StreamWriter(mopSavePath);
            writer.Write(json);
            writer.Close();
        }

        /// <summary>
        /// Returns true, if the save file is post permadeath.
        /// </summary>
        private static bool IsSaveFileAfterPermadeath()
        {
            return !ES2.Exists($"{SavePath}?tag=PlayerIsDead");
        }
    }
}
