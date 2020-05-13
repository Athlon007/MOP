﻿// Modern Optimization Plugin
// Copyright(C) 2019-2020 Athlon

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

using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace MOP
{
    class ServerContentData
    {
        public string ID;
        public DateTime UpdateTime;

        public ServerContentData(string content)
        {
            ID = content.Split(',')[0];
            string time = content.Split(',')[1];
            int day = int.Parse(time.Split('.')[0]);
            int month = int.Parse(time.Split('.')[1]);
            int year = int.Parse(time.Split('.')[2]);
            UpdateTime = new DateTime(year, month, day);
        }
    }

    class RuleFilesLoader : MonoBehaviour
    {
        const string RemoteServer = "http://athlon.kkmr.pl/mop/rulefiles/";
        const string ServerContent = "servercontent.mop";
        //const int FileThresholdHours = 168; // 1 week

        List<ServerContentData> serverContent;
        string lastModListPath;
        string lastDateFilePath;

        bool fileDownloadCompleted;
        bool overrideUpdateCheck;

        TextMesh message;
        TextMesh shadow;

        public void Initialize(bool overrideUpdateCheck)
        {
            lastModListPath = $"{MOP.ModConfigPath}\\LastModList.mop";
            lastDateFilePath = $"{MOP.ModConfigPath}\\LastUpdate.mop";
            this.overrideUpdateCheck = overrideUpdateCheck;

            if (GameObject.Find("MOP_Messager") != null)
            {
                message = GameObject.Find("MOP_Messager").GetComponent<TextMesh>();
                shadow = message.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
            }
            else
            {
                GameObject text = GameObject.Instantiate(GameObject.Find("Interface/Songs/Text"));
                text.transform.localPosition = new Vector3(6f, 2.6f, 0.01f);
                text.name = "MOP_Messager";
                message = text.GetComponent<TextMesh>();
                message.alignment = TextAlignment.Right;
                message.anchor = TextAnchor.UpperRight;
                shadow = text.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
                shadow.alignment = TextAlignment.Right;
                shadow.anchor = TextAnchor.UpperRight;
                NewMessage("");
            }

            if (!MopSettings.RuleFilesAutoUpdateEnabled && !overrideUpdateCheck)
            {
                ModConsole.Print("<color=orange>[MOP] Rule files auto update is disabled.</color>");
                GetAndReadRules();
                return;
            }

            // If server or user is offline, skip downloading and simply load available files.
            if (!IsServerOnline())
            {
                ModConsole.Print("<color=red>[MOP] Connection error. Check your Internet connection.</color>");

                GetAndReadRules();
                return;
            }

            StartCoroutine(DownloadAndUpdateRoutine());
        }

        IEnumerator DownloadAndUpdateRoutine()
        {
            string lastModList = File.Exists(lastModListPath) ? File.ReadAllText(lastModListPath) : "";
            Mod[] mods = ModLoader.LoadedMods.Where(m => !m.ID.ContainsAny("MSCLoader_", "MOP")).ToArray();
            string modListString = "";

            ModConsole.Print("[MOP] Checking for new mods...");

            bool isUpdateTime = !File.Exists(lastDateFilePath) || IsUpdateTime();

            if (isUpdateTime)
                ModConsole.Print("[MOP] Looking for updates...");

            foreach (Mod mod in mods)
            {
                string modId = mod.ID;
                modListString += $"{modId}\n";

                string ruleUrl = $"{RemoteServer}{modId}.mopconfig";
                string filePath = $"{MOP.ModConfigPath}\\{modId}.mopconfig";

                // Prevent downloading, if file is on the server.
                if (lastModList.Contains(mod.ID) && !isUpdateTime)
                    continue;

                // Check if rule file for mod is on the server.
                // If not, continue.
                if (!IsFileOnServer(modId))
                    continue;

                // Check if the newer file is available on the server.
                if (!overrideUpdateCheck)
                {
                    if (serverContent == null)
                    {
                        GetServerContent();
                    }

                    DateTime lastLocalFileWrite = GetFileWriteTime(filePath);
                    ServerContentData data = serverContent.First(t => t.ID == modId);
                    DateTime lastRemoteFileWrite = data.UpdateTime;
                    if (lastRemoteFileWrite <= lastLocalFileWrite)
                    {
                        ModConsole.Print($"<color=orange>[MOP] Skipping {modId}, because local file and remote file are the same.</color>");
                        continue;
                    }
                }

                // Check if file actually exists on remote server.
                if (!RemoteFileExists(ruleUrl))
                {
                    ModConsole.Error($"[MOP] Rule file for mod doesn't exist!\nID: {modId}\nURL: {ruleUrl}");
                    continue;
                }

                fileDownloadCompleted = false;
                using (WebClient web = new WebClient())
                {
                    ModConsole.Print($"<color=yellow>[MOP] Downloading new rule file for {mod.Name}...</color>");
                    NewMessage($"MOP: Downloading new rule file for {mod.Name}...");
#if DEBUG
                    web.Headers.Add("user-agent", $"MOP/{MOP.ModVersion}_DEBUG {ExceptionManager.GetSystemInfo()}");
#else
                    web.Headers.Add("user-agent", $"MOP/{MOP.ModVersion} {ExceptionManager.GetSystemInfo()}");
#endif
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    web.DownloadFileCompleted += DownloadFileCompleted;
                    web.DownloadFileAsync(new Uri(ruleUrl), filePath);

                    int waitTime = 0;
                    while (!fileDownloadCompleted)
                    {
                        yield return new WaitForSeconds(.5f);
                        waitTime++;

                        // If wait time is longer than 30 seconds, abandon downloading.
                        if (waitTime > 60)
                        {
                            ModConsole.Error("[MOP] Downloading failed. Skipping downloading.");
                            NewMessage("MOP: Downloading failed. Skipping downloading.");
                            GetAndReadRules();
                            yield break;
                        }
                    }

                    web.Dispose();
                }

                ModConsole.Print("<color=green>[MOP] Downloading completed!</color>");
            }

            File.WriteAllText(lastModListPath, modListString);
            if (isUpdateTime)
                File.WriteAllText(lastDateFilePath, DateTime.Now.ToString());

            // File downloading and updating completed!
            // Start reading those files.   
            GetAndReadRules();
        }

        void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            fileDownloadCompleted = true;
        }

        /// <summary>
        /// Seeks for rule files (.mopconfig) in MOP config folder.
        /// </summary>
        void GetAndReadRules()
        {
            overrideUpdateCheck = false;

            try
            {
                // Find and .mopconfig files.
                DirectoryInfo dir = new DirectoryInfo(MOP.ModConfigPath);
                List<FileInfo> files = dir.GetFiles().Where(d => d.Name.EndsWith(".mopconfig")).ToList();
                if (files.Count == 0)
                {
                    ModConsole.Print($"[MOP] No rule files found.");
                    NewMessage("");
                    return;
                }

                // Load custom rule file.
                if (File.Exists($"{dir}\\Custom.txt"))
                {
                    files.Add(new FileInfo($"{dir}\\Custom.txt"));
                    ModConsole.Print("[MOP] User custom rule file found!");
                }

                string message = $"[MOP] Found {files.Count} rule file{(files.Count > 1 ? "s" : "")}!";
                if (files.Count == 69)
                    message = message.Rainbowmize();

                ModConsole.Print(message);

                // Read rule files.
                foreach (FileInfo file in files)
                {
                    // Delete rules for mods that don't exist.
                    if (ModLoader.LoadedMods.Find(m => m.ID == Path.GetFileNameWithoutExtension(file.Name)) == null && file.Name != "Custom.txt")
                    {
                        if ((bool)MOP.NoDeleteRuleFiles.GetValue())
                        {
                            ModConsole.Print($"<color=yellow>[MOP] Skipped {file.Name} rule, " +
                                $"because the corresponding mod is not present.</color>");
                            continue;
                        }
                        File.Delete(file.FullName);
                        ModConsole.Print($"<color=yellow>[MOP] Rule file {file.Name} has been deleted, " +
                            $"because corresponding mod is not present.</color>");
                        continue;
                    }

                    Rules.instance.RuleFileNames.Add(file.Name);
                    ReadRulesFromFile(file.FullName);
                }

                ModConsole.Print("<color=green>[MOP] Loading rule files done!</color>");
                NewMessage($"MOP: Loading {files.Count} rule file{(files.Count > 1 ? "s" : "")} done!");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, "RULE_FILES_READ_ERROR");
            }
        }

        /// <summary>
        /// Checks if there's a rule file on the server corresponding to the mod.
        /// </summary>
        bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                string responseUri = response.ResponseUri.ToString();
                response.Close();

                //Returns TRUE if the Status code == 200 and it's not 404.html website
                return (response.StatusCode == HttpStatusCode.OK && responseUri != "http://athlon.kkmr.pl/404.html");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads serverContent file downloaded from server, and check if it contains the modId we're interested in.
        /// </summary>
        bool IsFileOnServer(string modId)
        {
            if (serverContent == null)
            {
                GetServerContent();
            }

            return serverContent.Where(t => t.ID == modId).ToArray().Length > 0;
        }

        /// <summary>
        /// Checks if the server is online
        /// </summary>
        bool IsServerOnline()
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect("193.143.77.46", 80);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true, if the time saved into the file with added FileThresholdHours is larger than the current time.
        /// </summary>
        bool IsUpdateTime()
        {
            if (DateTime.TryParse(File.ReadAllText(lastDateFilePath), out DateTime past))
            {
                int fileThresholdsHours = MopSettings.GetRuleFilesUpdateDaysFrequency() * 24;
                past = past.AddHours(fileThresholdsHours);
                return DateTime.Now > past;
            }

            return true;
        }

        void GetServerContent()
        {
            try
            {
                if (serverContent == null)
                {
                    WebClient web = new WebClient();
#if DEBUG
                    web.Headers.Add("user-agent", $"MOP/{MOP.ModVersion}_DEBUG {ExceptionManager.GetSystemInfo()}");
#else
                    web.Headers.Add("user-agent", $"MOP/{MOP.ModVersion} {ExceptionManager.GetSystemInfo()}");
#endif
                    string[] serverContentArray = web.DownloadString(new Uri($"{RemoteServer}{ServerContent}")).Split('\n');
                    serverContent = new List<ServerContentData>();
                    for (int i = 0; i < serverContentArray.Length; i++)
                    {
                        serverContent.Add(new ServerContentData(serverContentArray[i]));
                    }

                    web.Dispose();
                }
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, "SERVER_CONTENT_DOWNLOAD_ERROR");
            }
        }

        /// <summary>
        /// Returns the last write time of file.
        /// </summary>
        DateTime GetFileWriteTime(string filename)
        {
            return File.GetLastWriteTime(filename);
        }

        // You know the rules and so do I
        // A full commitment's what I'm thinking of
        // You wouldn't get this from any other guy
        // I just wanna tell you how I'm feeling
        // Gotta make you understand
        // Never gonna give you up
        // Never gonna let you down
        // Never gonna run around and desert you
        // Never gonna make you cry
        // Never gonna say goodbye
        // Never gonna tell a lie and hurt you

        void ReadRulesFromFile(string rulePath)
        {
            try
            {
                string[] content = File.ReadAllLines(rulePath).Where(s => s.Length > 0 && !s.StartsWith("##")).ToArray();
                string fileName = Path.GetFileName(rulePath);
                foreach (string s in content)
                { 
                    string[] splitted = s.Split(':');

                    // Read flag and rules.
                    string flag = splitted[0];
                    string[] objects = new string[0];
                    if (splitted.Length > 1)
                    {
                        objects = splitted[1].Trim().Split(' ');
                        for (int i = 0; i < objects.Length; i++)
                            objects[i] = objects[i].Replace("%20", " ");
                    }

                    if (objects.Length > 0 && objects.ContainsAny("MOP", "MSCLoader", "MSCUnloader", "Steam", "PLAYER", "cObject", "FPSCamera"))
                    {
                        ModConsole.Error($"[MOP] Illegal object: {objects[0]} in rule file {fileName}.");
                        continue;
                    }

                    // Apply these rules
                    switch (flag)
                    {
                        default:
                            ModConsole.Error($"[MOP] Unrecognized flag '{flag}' in file {rulePath}");
                            break;
                        case "ignore":
                            Rules.instance.IgnoreRules.Add(new IgnoreRule(objects[0], false));
                            break;
                        case "ignore_full":
                            Rules.instance.IgnoreRules.Add(new IgnoreRule(objects[0], true));
                            break;
                        case "ignore_at_place":
                            string place = objects[0];
                            string obj = objects[1];
                            Rules.instance.IgnoreRulesAtPlaces.Add(new IgnoreRuleAtPlace(place, obj));
                            break;
                        case "toggle":
                            Rules.instance.ToggleRules.Add(new ToggleRule(objects[0], ToggleModes.Normal));
                            break;
                        case "toggle_renderer":
                            Rules.instance.ToggleRules.Add(new ToggleRule(objects[0], ToggleModes.Renderer));
                            break;
                        case "toggle_item":
                            Rules.instance.ToggleRules.Add(new ToggleRule(objects[0], ToggleModes.Item));
                            break;
                        case "toggle_vehicle":
                            Rules.instance.ToggleRules.Add(new ToggleRule(objects[0], ToggleModes.Vehicle));
                            break;
                        case "toggle_vehicle_physics_only":
                            Rules.instance.ToggleRules.Add(new ToggleRule(objects[0], ToggleModes.VehiclePhysics));
                            break;
                        case "satsuma_ignore_renderer":
                            Rules.instance.SpecialRules.SatsumaIgnoreRenderers = true;
                            break;
                        case "dont_destroy_empty_beer_bottles":
                            Rules.instance.SpecialRules.DontDestroyEmptyBeerBottles = true;
                            break;
                        case "prevent_toggle_on_object":
                            Rules.instance.PreventToggleOnObjectRule.Add(new PreventToggleOnObjectRule(objects[0], objects[1]));
                            break;
                        case "sector":
                            Vector3 pos = GetVector3(objects[0]);
                            Vector3 scale = GetVector3(objects[1]);
                            Vector3 rot = GetVector3(objects[2]);
                            string[] whitelist = GetWhitelist(objects);
                            Rules.instance.NewSectors.Add(new NewSector(pos, scale, rot, whitelist));
                            break;

                        // Custom.txt exclusives.
                        case "ignore_mod_vehicles":
                            if (fileName != "Custom.txt")
                            {
                                ModConsole.Error($"[MOP] Flag: {flag} is only allowed to be used in custom rule file.");
                                continue;
                            }
                            Rules.instance.SpecialRules.IgnoreModVehicles = true;
                            break;
                        case "toggle_all_vehicles_physics_only":
                            if (fileName != "Custom.txt")
                            {
                                ModConsole.Error($"[MOP] Flag: {flag} is only allowed to be used in custom rule file.");
                                continue;
                            }
                            Rules.instance.SpecialRules.ToggleAllVehiclesPhysicsOnly = true;
                            break;
                        case "experimental_driveway_sector":
                            if (fileName != "Custom.txt")
                            {
                                ModConsole.Error($"[MOP] Flag: {flag} is only allowed to be used in custom rule file.");
                                continue;
                            }
                            Rules.instance.SpecialRules.DrivewaySector = true;
                            break;
                        case "experimental_satsuma_trunk":
                            if (fileName != "Custom.txt")
                            {
                                ModConsole.Error($"[MOP] Flag: {flag} is only allowed to be used in custom rule file.");
                                continue;
                            }
                            Rules.instance.SpecialRules.ExperimentalSatsumaTrunk = true;
                            break;
                        case "experimental_optimization":
                            if (fileName != "Custom.txt")
                            {
                                ModConsole.Error($"[MOP] Flag: {flag} is only allowed to be used in custom rule file.");
                                continue;
                            }
                            Rules.instance.SpecialRules.ExperimentalOptimization = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ModConsole.Error($"[MOP] Error loading rule {rulePath}: {ex}.");
                NewMessage($"<color=red>MOP: Error loading rule :(");
            }
        }

        /// <summary>
        /// Sets the text of MOP_Messager object.
        /// </summary>
        /// <param name="value"></param>
        void NewMessage(string value)
        {
            message.text = value;
            shadow.text = value;
        }

        Vector3 GetVector3(string s)
        {
            try
            {
                float x, y, z;
                string[] split = s.Split(',');
                x = float.Parse(split[0]);
                y = float.Parse(split[1]);
                z = float.Parse(split[2]);
                return new Vector3(x, y, z);
            }
            catch
            {
                ModConsole.Error($"[MOP] Incorrect vector format! Refer to MOP wiki.");
                return Vector3.zero;
            }
        }

        string[] GetWhitelist(string[] s)
        {
            try
            {
                int start = 3;
                int end = s.Length;
                List<string> newList = new List<string>();
                for (int i = start; i < end; i++)
                    newList.Add(s[i]);

                return newList.ToArray();
            }
            catch
            {
                return new string[] { };
            }
        }
    }
}
