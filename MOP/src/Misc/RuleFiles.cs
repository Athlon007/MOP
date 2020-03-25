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
    class IgnoreRule
    {
        public string ObjectName;
        public bool TotalIgnore;

        public IgnoreRule(string ObjectName, bool TotalIgnore)
        {
            this.ObjectName = ObjectName;
            this.TotalIgnore = TotalIgnore;
        }
    }

    /// <summary>
    /// Toggling methods for items.
    /// <list type="bullet">Normal - uses world objec toggling.</list>
    /// <list type="bullet">Renderer - Toggles only renderer of the object.</list>
    /// <list type="bullet">Item - Toggles object as it was an item.</list>
    /// <list type="bullet">Vehicle - Toggles object as vehicle.</list>
    /// <list type="bullet">VehiclePhysics - Toggles object as vehicle, but only the UnityCar part.</list>
    /// </summary>
    public enum ToggleModes { Normal, Renderer, Item, Vehicle, VehiclePhysics };

    class ToggleRule
    {
        public string ObjectName;
        public ToggleModes ToggleMode;

        public ToggleRule(string ObjectName, ToggleModes ToggleMode)
        {
            this.ObjectName = ObjectName;
            this.ToggleMode = ToggleMode;
        }
    }

    /// <summary>
    /// This class is intended for special flags used in specific cases.
    /// </summary>
    class SpecialRules
    {
        public bool SatsumaIgnoreRenderers;
        public bool DontDestroyEmptyBeerBottles;
    }

    class RuleFiles
    {
        public static RuleFiles instance;

        // Ignore rules.
        public List<IgnoreRule> IgnoreRules;
        public List<IgnoreRule> YardIgnoreRules;
        public List<IgnoreRule> StoreIgnoreRules;
        public List<IgnoreRule> RepairShopIgnoreRules;
        public List<IgnoreRule> InspectionIgnoreRules;

        // Toggling rules.
        public List<ToggleRule> ToggleRules;        

        // Special rules.
        public SpecialRules SpecialRules;

        // Used for mod report only.
        public List<string> RuleFileNames;

        public RuleFiles(bool overrideUpdateCheck = false)
        {
            instance = this;
            IgnoreRules = new List<IgnoreRule>();
            ToggleRules = new List<ToggleRule>();
            
            YardIgnoreRules = new List<IgnoreRule>();
            StoreIgnoreRules = new List<IgnoreRule>();
            RepairShopIgnoreRules = new List<IgnoreRule>();
            InspectionIgnoreRules = new List<IgnoreRule>();

            this.SpecialRules = new SpecialRules();
            this.RuleFileNames = new List<string>();

            if (GameObject.Find("MOP_RuleFilesLoader") != null)
            {
                GameObject.Destroy(GameObject.Find("MOP_RuleFilesLoader"));
            }

            GameObject ruleFileDownloader = new GameObject("MOP_RuleFilesLoader");
            RuleFilesLoader ruleFilesLoader = ruleFileDownloader.AddComponent<RuleFilesLoader>();
            ruleFilesLoader.Initialize(overrideUpdateCheck);
        }
    }

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

        public void Initialize(bool overrideUpdateCheck)
        {
            lastModListPath = $"{MOP.ModConfigPath}\\LastModList.mop";
            lastDateFilePath = $"{MOP.ModConfigPath}\\LastUpdate.mop";

            this.overrideUpdateCheck = overrideUpdateCheck;

            if (GameObject.Find("MOP_Messager") != null)
            {
                message = GameObject.Find("MOP_Messager").GetComponent<TextMesh>();
            }
            else
            {
                GameObject text = GameObject.Instantiate(GameObject.Find("Interface/Songs/Text"));
                GameObject.Destroy(text.transform.GetChild(0).gameObject);
                text.transform.localPosition = new Vector3(0, -2.40f, 0.01f);
                text.name = "MOP_Messager";
                message = text.GetComponent<TextMesh>();
                message.alignment = TextAlignment.Center;
                message.anchor = TextAnchor.UpperCenter;
                message.text = "";
            }

            buttonsFsms = new List<PlayMakerFSM>
            {
                GameObject.Find("Interface/Buttons/ButtonContinue").GetComponent<PlayMakerFSM>(),
                GameObject.Find("Interface/Buttons/ButtonNewgame").GetComponent<PlayMakerFSM>()
            };

            foreach (PlayMakerFSM fsm in buttonsFsms)
                fsm.enabled = false;

            if (!IsServerOnline())
            {
                ModConsole.Error("[MOP] Connection error. Couldn't download new rule files.");

                ReadFiles();

                foreach (PlayMakerFSM fsm in buttonsFsms)
                    fsm.enabled = true;
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

                string ruleUrl = RemoteServer + modId + ".mopconfig";
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

                ModConsole.Print($"<color=yellow>[MOP] Downloading new rule file for {mod.Name}...</color>");
                message.text = $"MOP: Downloading new rule file for {mod.Name}...";
                fileDownloadCompleted = false;
                using (WebClient web = new WebClientWithTimeout())
                {
                    web.DownloadFileCompleted += DownloadFileCompleted;
                    web.DownloadFileAsync(new Uri(ruleUrl), filePath);
                    
                    while (!fileDownloadCompleted)
                        yield return new WaitForSeconds(.5f);

                    web.Dispose();
                }

                ModConsole.Print("<color=green>[MOP] Downloading completed!</color>");
            }

            File.WriteAllText(lastModListPath, modListString);
            if (isUpdateTime)
                File.WriteAllText(lastDateFilePath, DateTime.Now.ToString());

            // File downloading and updating completed!
            // Start reading those files.   
            ReadFiles();
        }

        void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            fileDownloadCompleted = true;
        }

        void ReadFiles()
        {
            overrideUpdateCheck = false;

            DirectoryInfo dir = new DirectoryInfo(MOP.ModConfigPath);
            FileInfo[] files = dir.GetFiles().Where(d => d.Name.EndsWith(".mopconfig")).ToArray();
            if (files.Length == 0)
            {
                ModConsole.Print($"[MOP] No rule files found.");
                message.text = "";
                return;
            }

            ModConsole.Print($"[MOP] Found {files.Length} rule file{(files.Length > 1 ? "s" : "")}!");

            // Read rule files
            foreach (FileInfo file in files)
            {
                // Delete rules for mods that don't exist.
                if (ModLoader.LoadedMods.Find(m => m.ID == Path.GetFileNameWithoutExtension(file.Name)) == null)
                {
                    File.Delete(file.FullName);
                    ModConsole.Print($"<color=yellow>[MOP] Rule file {file.Name} has been deleted, because corresponding mod is not present.</color>");
                    continue;
                }

                RuleFiles.instance.RuleFileNames.Add(file.Name);
                ReadRule(file.FullName);
            }

            ModConsole.Print("[MOP] Loading rule files done!");
            message.text = "MOP: Loading rule files done!";

            foreach (PlayMakerFSM fsm in buttonsFsms)
                fsm.enabled = true;
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

        void ReadRule(string rulePath)
        {
            string[] content = File.ReadAllLines(rulePath).Where(s => s.Length > 0 && !s.StartsWith("##")).ToArray();
            foreach (string s in content)
            {
                string[] split = s.Split(':');

                switch (split[0])
                {
                    default:
                        ModConsole.Error($"[MOP] Unrecognized flag '{split[0]}' in file {rulePath}");
                        break;
                    case "ignore":
                        RuleFiles.instance.IgnoreRules.Add(new IgnoreRule(split[1].Trim(), false));
                        break;
                    case "ignore_full":
                        RuleFiles.instance.IgnoreRules.Add(new IgnoreRule(split[1].Trim(), true));
                        break;
                    case "ignore_at_place":
                        string place = split[1].Trim().Split(' ')[0];
                        string obj = split[1].Trim().Split(' ')[1];
                        switch (place)
                        {
                            default:
                                ModConsole.Error($"[MOP] Unrecognized place '{place}' in flag '{split[0]}' in file {rulePath}");
                                break;
                            case "STORE":
                                RuleFiles.instance.StoreIgnoreRules.Add(new IgnoreRule(obj, false));
                                break;
                            case "YARD":
                                RuleFiles.instance.YardIgnoreRules.Add(new IgnoreRule(obj, false));
                                break;
                            case "REPAIRSHOP":
                                RuleFiles.instance.RepairShopIgnoreRules.Add(new IgnoreRule(obj, false));
                                break;
                            case "INSPECTION":
                                RuleFiles.instance.InspectionIgnoreRules.Add(new IgnoreRule(obj, false));
                                break;
                        }
                        break;
                    case "toggle":
                        RuleFiles.instance.ToggleRules.Add(new ToggleRule(split[1].Trim(), ToggleModes.Normal));
                        break;
                    case "toggle_renderer":
                        RuleFiles.instance.ToggleRules.Add(new ToggleRule(split[1].Trim(), ToggleModes.Renderer));
                        break;
                    case "toggle_item":
                        RuleFiles.instance.ToggleRules.Add(new ToggleRule(split[1].Trim(), ToggleModes.Item));
                        break;
                    case "toggle_vehicle":
                        RuleFiles.instance.ToggleRules.Add(new ToggleRule(split[1].Trim(), ToggleModes.Vehicle));
                        break;
                    case "toggle_vehicle_physics_only":
                        RuleFiles.instance.ToggleRules.Add(new ToggleRule(split[1].Trim(), ToggleModes.VehiclePhysics));
                        break;
                    case "satsuma_ignore_renderer":
                        RuleFiles.instance.SpecialRules.SatsumaIgnoreRenderers = true;
                        break;
                    case "dont_destroy_empty_beer_bottles":
                        RuleFiles.instance.SpecialRules.DontDestroyEmptyBeerBottles = true;
                        break;
                }
            }
        }
    }

    public class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest wr = base.GetWebRequest(address);
            wr.Timeout = 30 * 1000; // timeout in milliseconds (ms)
            return wr;
        }
    }
}
