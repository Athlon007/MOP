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
using System.Reflection;
using UnityEngine;
using MSCLoader;
using Newtonsoft.Json;
using HutongGames.PlayMaker;

using MOP.Common;

namespace MOP.Helpers
{
    static class SaveManager
    {
        public static string SavePath => Path.Combine(Application.persistentDataPath, "defaultES2File.txt").Replace('\\', '/');
        public static string ItemsPath => Path.Combine(Application.persistentDataPath, "items.txt").Replace("\\", "/");
        public static string OptionsPath => Path.Combine(Application.persistentDataPath, "options.txt").Replace("\\", "/");
        static List<SaveBugs> saveBugs;

        readonly static ES2Settings setting = new ES2Settings();

        // MOP Save Files.
        static string mopSavePath => Path.Combine(Application.persistentDataPath, "MopSave.json");
        static bool SaveFileExists => File.Exists(mopSavePath);

        // Minimum percentage of bolts, that must be screwed in,
        // in order for MOP to assume the save hasn't been tampered with.
        const double MSCEditorLikelihoodRatio = 0.15;
        // Minimum number of parts that must be installed to the car,
        // in order to check for MSCEditor save tampering.
        const int MinimumNumberOfPartsToCheckMSCEditorTampering = 40;

        const int MaximumSaveBugsDisplayed = 4;

        private static SaveManagerBehaviour saveManagerBehaviour;

        /// <summary>
        /// For some reason, the save files get marked as read only files, not allowing MSC to save the game.
        /// This script is ran in PreSaveGame() script and removes ReadOnly attribute.
        /// </summary>
        public static void RemoveReadOnlyAttribute()
        {
            if (saveManagerBehaviour == null)
            {
                GameObject saveManagerObject = new GameObject("MOP_SaveAttributeRemoval");
                saveManagerBehaviour = saveManagerObject.AddComponent<SaveManagerBehaviour>();
            }

            try
            {
                saveManagerBehaviour.Run();
            }
            catch (UnauthorizedAccessException)
            {
                ModConsole.Log("<color=red>MOP: MOP could not gain access to the save file at this moment.</color>");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "ATTRIBUTES_REMOVAL_ERROR");
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

        public static void WriteSaveTag<T>(string tag, T value)
        {
            ES2.Save(value, $"{SavePath}?tag={tag}", setting);
        }

        public static void WriteItemTag<T>(string tag, T value)
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
                        WriteSaveTag("bucket seat passenger(Clone)Purchased", true);
                        WriteSaveTag("bucket seat driver(Clone)Purchased", true); 
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
                            WriteSaveTag("TractorTrailerAttached", false);
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
                        WriteSaveTag("Bumper_RearTightness", save.rearBumperTightness);
                        WriteSaveTag("Bumper_RearBolts", save.rearBumperBolts);
                    }
                }
                catch
                {
                    ModConsole.LogSilentError("[MOP] MOP");
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
                                WriteSaveTag("Halfshaft_FLTightness", save.halfshaft_FLTightness);
                                WriteSaveTag("Halfshaft_FLBolts", save.halfshaft_FLBolts);
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
                                WriteSaveTag("Halfshaft_FRTightness", save.halfshaft_FRTightness);
                                WriteSaveTag("Halfshaft_FRBolts", save.halfshaft_FRBolts);
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
                                WriteSaveTag("WiringBatteryMinusTightness", save.wiringBatteryMinusTightness);
                                WriteSaveTag("WiringBatteryMinusBolts", save.wiringBatteryMinusBolts);
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
                            WriteSaveTag("FuelLineTightness", (float)boltOneValue);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VERIFY_FUELLINE_TIGHTNESS");
            }

            var dictionaryDb = ReadItemCounterToTransformDictionary();
            if (dictionaryDb != null)
            {
                FixItemsSaveFile(dictionaryDb);
            }

            if (saveBugs.Count > 0)
            {
                string message = "";
                if (saveBugs.Count > 4)
                {
                    message = $"MOP has found <color=yellow>{saveBugs.Count}</color> problem{(saveBugs.Count > 1 ? "s" : "")} with your save:\n\n" +
                                       $"<color=yellow>{string.Join("\n", saveBugs.Select(f => f.BugName).Take(MaximumSaveBugsDisplayed).ToArray())}</color>\n" +
                                       $"...and <color=yellow>{saveBugs.Count - MaximumSaveBugsDisplayed}</color> more.\n\n" +
                                       $"Do you want the MOP to try to fix {((saveBugs.Count > 1) ? "these problems" : "this problem")}?";
                }
                else
                {
                    message = $"MOP has found <color=yellow>{saveBugs.Count}</color> problem{(saveBugs.Count > 1 ? "s" : "")} with your save:\n\n" +
                   $"<color=yellow>{string.Join("\n", saveBugs.Select(f => f.BugName).ToArray())}</color>\n\n" +
                   $"Do you want the MOP to try to fix {((saveBugs.Count > 1) ? "these problems" : "this problem")}?";
                }


#if PRO
                ModPrompt prompt = ModPrompt.CreateCustomPrompt();
                prompt.Title = "MOP - Save Integrity Verification";
                prompt.Text = message;
                prompt.DestroyOnDisable = false;
                prompt.AddButton("YES", () => { FixAllProblems(); GameObject.Destroy(prompt.gameObject); });
                prompt.AddButton("SHOW REPORT", () => { ShowReport(); prompt.gameObject.SetActive(true); });
                prompt.AddButton("NO", () => { GameObject.Destroy(prompt.gameObject); });
#else
                ModUI.ShowCustomMessage(message, "MOP - Save Integrity Verification", new MsgBoxBtn[]
                {
                    ModUI.CreateMessageBoxBtn("YES", FixAllProblems),
                    ModUI.CreateMessageBoxBtn("SHOW REPORT", ShowReport, true),
                    ModUI.CreateMessageBoxBtn("NO")
                }, new MsgBoxBtn[] { });
#endif
            }
            else
            {
                ModConsole.Log("[MOP] MOP hasn't found any problems with your save :)");
            }
        }

        private static void FixItemsSaveFile(Dictionary<string, string> db)
        {
            foreach (KeyValuePair<string, string> entry in db)
            {
                try
                {
                    string key = entry.Key;
                    // Sometimes the items are saved in "itemnamex" format, instead of "itemname".
                    string xVariant = key.Split(new string[] { "ID" }, StringSplitOptions.None)[0] + "xID";
                    bool useXVariant = false;
                    if (!IsItemTagPresent(key))
                    {
                        key = xVariant;
                        if (!IsItemTagPresent(key))
                        {
                            continue;
                        }

                        useXVariant = true;
                    }
                    string value = entry.Value;
                    if (useXVariant)
                    {
                        value += "x";
                    }

                    int savedItemCount = ReadItemInt(key);
                    int counter = 1;
                    while (IsItemTagPresent($"{value}{counter}Transform"))
                    {
                        counter++;
                    }

                    if (counter > 0)
                    {
                        counter--;
                    }

                    if (savedItemCount < counter)
                    {
                        saveBugs.Add(SaveBugs.New($"{key} is not a correct value.\nExpected: {counter}. Actual: {savedItemCount}\n", 
                            () => WriteItemTag(key, counter)));
                    }

                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, $"VERIFY_ITEMS_COUNTER_{entry.Key}/{entry.Value}");
                }
            }
        }

        static void FixAllProblems()
        {
            BackupSave();
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

            msg += "\n\nYour save file has been backed up into <color=yellow>defaultES2Save.txt.mopbackup</color> and <color=yellow>items.txt.mopbackup</color>.";

            ModUI.ShowMessage(msg, "MOP - Save Integrity Check");
        }

        internal static Transform GetRadiatorHose3Transform()
        {
            return ES2.Load<Transform>(SavePath + "?tag=radiator hose3(xxxxx)", setting);
        }

        internal static void AddSaveFlag()
        {
            if (IsSaveTagPresent("Bumper_RearTightness") && IsSaveTagPresent("Bumper_RearBolts"))
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

            bool isCylinderHeadInstalled = ReadBoolean("cylinder head(Clone)Installed");
            bool isEngineBlockInstalled = ReadBoolean("block(Clone)Installed");

            if (!isEngineBlockInstalled)
            {
                if (isCylinderHeadInstalled && !cylinderHead.transform.Path().Contains("block(Clone)"))
                {
                    throw new Exception("Cylinder head is not a part of the engine block.");
                }
            }
            else
            {
                if ((!cylinderHead.gameObject.activeSelf || cylinderHead.transform.root != satsuma.transform) && isCylinderHeadInstalled)
                {
                    throw new Exception("Cylinder head is not active, or is not a part of Satsuma.");
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
                    throw new Exception($"Spark plug {i} is installed, but not a part of the engine.");
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

        public static bool IsCarAssembledWithMSCEditor()
        {
            if (ModLoader.GetCurrentScene() != CurrentScene.Game)
            {
                throw new AccessViolationException("Can only be executed in-game.");
            }

            int boltedCount = 0;
            int installedCount = 0;
            CountBoltedAndInstalled(GameObject.Find("Database/DatabaseBody"), ref boltedCount, ref installedCount);
            CountBoltedAndInstalled(GameObject.Find("Database/DatabaseMechanics"), ref boltedCount, ref installedCount);
            CountBoltedAndInstalled(GameObject.Find("Database/DatabaseMotor"), ref boltedCount, ref installedCount);

            // If the number of parts is too low for the game to check if car has been assembled with MSCEditor, 
            // don't bother.
            // It would be really weird, if someone installed a single part, not bolt it, then have that error appear.
            if (installedCount < MinimumNumberOfPartsToCheckMSCEditorTampering)
            {
                ModConsole.Log($"[MOP] Only {installedCount} parts are installed.");
                return false;
            }

            double percentageOfPartsBolted = boltedCount / (double)installedCount;
            ModConsole.Log($"[MOP] {((1d - percentageOfPartsBolted) * 100):0.00}% of Satsuma parts are installed, but not bolted.\n" +
                           $"{boltedCount}/{installedCount} parts are bolted and installed.");

            if (percentageOfPartsBolted <= MSCEditorLikelihoodRatio)
                return true;

            return IsEngineStupidlyAssembled();
        }



        private static void CountBoltedAndInstalled(GameObject databaseObject, ref int boltedCount, ref int installedCount)
        {
            foreach (PlayMakerFSM fsm in databaseObject.GetComponentsInChildren<PlayMakerFSM>())
            {
                FsmBool installed = fsm.FsmVariables.FindFsmBool("Installed");
                FsmBool bolted = fsm.FsmVariables.FindFsmBool("Bolted");
                if (bolted != null && installed != null)
                {
                    if (installed.Value)
                    {
                        installedCount++;
                        if (bolted.Value)
                        {
                            boltedCount++;
                        }
                    }
                }
            }
        }

        private static Dictionary<string, string> ReadItemCounterToTransformDictionary()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().SingleOrDefault(str => str.EndsWith("itemcounter_to_transform.csv"));

            if (string.IsNullOrEmpty(resourceName))
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] arr = line.Split(',');
                        keyValuePairs.Add(arr[0], arr[1]);
                    }
                }
            }

            return keyValuePairs;
        }

        private static void ShowReport()
        {
            if (!Paths.LogDirectoryExists)
            {
                Directory.CreateDirectory(Paths.LogFolder);
            }

            string path = Path.Combine(Paths.LogFolder, Paths.SaveFileBugsReport + ".txt");
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("=== Generated with MOP " + MOP.ModVersion + " ===\n\n");
                foreach (SaveBugs bug in saveBugs)
                {
                    writer.WriteLine($"{bug.BugName}");
                }
            }

            System.Diagnostics.Process.Start(path);
        }

        private static void BackupSave()
        {
            if (File.Exists(SavePath))
            {
                File.Copy(SavePath, SavePath + ".mopbackup", true);
            }

            if (File.Exists(ItemsPath))
            {
                File.Copy(SavePath, ItemsPath + ".mopbackup", true);
            }
        }

        /// <summary>
        /// Checks if engine has been assembled in using MSCEditor (aka: engine is installed, bolted, but the arrays are 0).
        /// </summary>
        /// <returns></returns>
        private static bool IsEngineStupidlyAssembled()
        {
            List<string> blockBolts = ReadStringList("BlockBolts");
            bool blockInstalled = ReadBoolean("block(Clone)Installed");
            bool blockBolted = ReadBoolean("block(Clone)Bolted");
            bool blockInHoist = ReadBoolean("block(Clone)InHoist");

            bool areAllbolted = true;
            foreach (string block in blockBolts)
            {
                if(int.Parse(block.Replace("int(", "").Replace(")", "")) == 0)
                { 
                    areAllbolted = false;
                }
            }

            return blockInstalled && blockBolted && !areAllbolted && !blockInHoist;
        }
    }
}
