// Modern Optimization Plugin
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
using System.Net.NetworkInformation;
using UnityEngine;

namespace MOP
{
    class RuleFilesLoader : MonoBehaviour
    {
        const string RemoteServer = "http://athlon.kkmr.pl/mop/rulefiles/";
        const int FileThresholdHours = 168; // 1 week

        string lastModListPath;
        string lastDateFilePath;

        bool fileDownloadCompleted;
        bool overrideUpdateCheck;

        List<PlayMakerFSM> buttonsFsms;
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
                text.transform.localPosition = new Vector3(-6.1f, 2.25f, 0.01f);
                text.name = "MOP_Messager";
                message = text.GetComponent<TextMesh>();
                message.alignment = TextAlignment.Left;
                message.anchor = TextAnchor.UpperLeft;
                shadow = text.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
                NewMessage("");
            }

            buttonsFsms = new List<PlayMakerFSM>
            {
                GameObject.Find("Interface/Buttons/ButtonContinue").GetComponent<PlayMakerFSM>(),
                GameObject.Find("Interface/Buttons/ButtonNewgame").GetComponent<PlayMakerFSM>()
            };

            ToggleButtons(false);

            if (MopSettings.IsAutoUpdateDisabled() && !overrideUpdateCheck)
            {
                ModConsole.Print("<color=orange>[MOP] Rule files auto update is disabled.</color>");
                GetAndReadRuleFiles();
                ToggleButtons(true);
                return;
            }

            // If server or user is offline, skip downloading and simply load available files.
            if (!IsServerOnline())
            {
                ModConsole.Error("[MOP] Connection error. Couldn't download new rule files.");

                GetAndReadRuleFiles();
                ToggleButtons(true);
            }
            else
            {
                StartCoroutine(DownloadAndUpdateRoutine());
            }
        }

        IEnumerator DownloadAndUpdateRoutine()
        {
            string lastModList = File.Exists(lastModListPath) ? File.ReadAllText(lastModListPath) : "";
            Mod[] mods = ModLoader.LoadedMods.Where(m => !m.ID.ContainsAny("MSCLoader_", "MOP")).ToArray();
            string modListString = "";

            ModConsole.Print("[MOP] Looking for rule updates...");

            bool isUpdateTime = !File.Exists(lastDateFilePath) || IsUpdateTime();

            foreach (Mod mod in mods)
            {
                string modId = mod.ID;
                modListString += $"{modId}\n";

                string ruleUrl = $"{RemoteServer}{modId}.mopconfig";
                string filePath = $"{MOP.ModConfigPath}\\{modId}.mopconfig";

                if (File.Exists(filePath) && !IsFileBelowThreshold(filePath, FileThresholdHours))
                {
                    continue;
                }
                else
                {
                    if (lastModList.Contains(mod.ID) && !isUpdateTime)
                        continue;
                }

                if (!RemoteFileExists(ruleUrl))
                    continue;

                fileDownloadCompleted = false;
                using (WebClient web = new WebClient())
                {
                    ModConsole.Print($"<color=yellow>[MOP] Downloading new rule file for {mod.Name}...</color>");
                    NewMessage($"MOP: Downloading new rule file for {mod.Name}...");
                    web.Headers.Add("user-agent", $"MOP/{MOP.ModVersion} {SystemInfo.operatingSystem}");
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
                            ToggleButtons(true);
                            GetAndReadRuleFiles();
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
            GetAndReadRuleFiles();
        }

        void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            fileDownloadCompleted = true;
        }

        /// <summary>
        /// Seeks for rule files (.mopconfig) in MOP config folder.
        /// </summary>
        void GetAndReadRuleFiles()
        {
            overrideUpdateCheck = false;

            try
            {
                // Find and .mopconfig files.
                DirectoryInfo dir = new DirectoryInfo(MOP.ModConfigPath);
                FileInfo[] files = dir.GetFiles().Where(d => d.Name.EndsWith(".mopconfig")).ToArray();
                if (files.Length == 0)
                {
                    ModConsole.Print($"[MOP] No rule files found.");
                    NewMessage("");
                    return;
                }

                ModConsole.Print($"[MOP] Found {files.Length} rule file{(files.Length > 1 ? "s" : "")}!");

                // Read rule files.
                foreach (FileInfo file in files)
                {
                    // Delete rules for mods that don't exist.
                    if (ModLoader.LoadedMods.Find(m => m.ID == Path.GetFileNameWithoutExtension(file.Name)) == null)
                    {
                        File.Delete(file.FullName);
                        ModConsole.Print($"<color=yellow>[MOP] Rule file {file.Name} has been deleted, " +
                            $"because corresponding mod is not present.</color>");
                        continue;
                    }

                    RuleFiles.instance.RuleFileNames.Add(file.Name);
                    ReadRulesFromFile(file.FullName);
                }

                ModConsole.Print("[MOP] Loading rule files done!");
                NewMessage($"MOP: Loading {files.Length} rule file{(files.Length > 1 ? "s" : "")} done!");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, "Rule Files Error");
            }

            ToggleButtons(true);
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
        /// Checks if the server is online
        /// </summary>
        bool IsServerOnline()
        {
            try
            {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                // 10 seconds time out (in ms).
                PingReply reply = ping.Send("athlon.kkmr.pl", 10 * 1000);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the rule file should be updated.
        /// </summary>
        bool IsFileBelowThreshold(string filename, int hours)
        {
            if (overrideUpdateCheck)
                return true;

            var threshold = DateTime.Now.AddHours(-hours);
            return File.GetCreationTime(filename) <= threshold;
        }

        /// <summary>
        /// Returns true, if the time saved into the file with added FileThresholdHours is larger than the current time.
        /// </summary>
        bool IsUpdateTime()
        {
            if (DateTime.TryParse(File.ReadAllText(lastDateFilePath), out DateTime past))
            {
                past = past.AddHours(FileThresholdHours);
                return DateTime.Now > past;
            }

            return true;
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

                    // Apply these rules
                    switch (flag)
                    {
                        default:
                            ModConsole.Error($"[MOP] Unrecognized flag '{flag}' in file {rulePath}");
                            break;
                        case "ignore":
                            RuleFiles.instance.IgnoreRules.Add(new IgnoreRule(objects[0], false));
                            break;
                        case "ignore_full":
                            RuleFiles.instance.IgnoreRules.Add(new IgnoreRule(objects[0], true));
                            break;
                        case "ignore_at_place":
                            string place = objects[0];
                            string obj = objects[1];
                            RuleFiles.instance.IgnoreRulesAtPlaces.Add(new IgnoreRuleAtPlace(place, obj));
                            break;
                        case "toggle":
                            RuleFiles.instance.ToggleRules.Add(new ToggleRule(objects[0], ToggleModes.Normal));
                            break;
                        case "toggle_renderer":
                            RuleFiles.instance.ToggleRules.Add(new ToggleRule(objects[0], ToggleModes.Renderer));
                            break;
                        case "toggle_item":
                            RuleFiles.instance.ToggleRules.Add(new ToggleRule(objects[0], ToggleModes.Item));
                            break;
                        case "toggle_vehicle":
                            RuleFiles.instance.ToggleRules.Add(new ToggleRule(objects[0], ToggleModes.Vehicle));
                            break;
                        case "toggle_vehicle_physics_only":
                            RuleFiles.instance.ToggleRules.Add(new ToggleRule(objects[0], ToggleModes.VehiclePhysics));
                            break;
                        case "satsuma_ignore_renderer":
                            RuleFiles.instance.SpecialRules.SatsumaIgnoreRenderers = true;
                            break;
                        case "dont_destroy_empty_beer_bottles":
                            RuleFiles.instance.SpecialRules.DontDestroyEmptyBeerBottles = true;
                            break;
                        case "prevent_toggle_on_object":
                            RuleFiles.instance.PreventToggleOnObjectRule.Add(new PreventToggleOnObjectRule(objects[0], objects[1]));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ModConsole.Error($"[MOP] Error loading rule {rulePath}: {ex.ToString()}.");
                NewMessage($"<color=red>MOP: Error loading rule :(");
                ToggleButtons(true);
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

        /// <summary>
        /// Toggles FSMs of Continue and New Game buttons.
        /// </summary>
        /// <param name="enabled"></param>
        void ToggleButtons(bool enabled)
        {
            foreach (PlayMakerFSM fsm in buttonsFsms)
                fsm.enabled = enabled;
        }
    }
}
