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

using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;

using MOP.Common;
using MOP.Common.Enumerations;
using MOP.Rules.Types;

namespace MOP.Rules.Configuration
{
    class Loader : MonoBehaviour
    {
        const string RemoteServer = "http://athlon.kkmr.pl/mop/rulefiles";
        const string ServerContent = "servercontent.mop";
        const string CustomFile = "Custom.txt";
        const string RuleExtension = ".mopconfig";

        readonly string[] illegalValues = { "MOP", "MSCLoader", "MSCUnloader", "Steam", "PLAYER", "cObject", "FPSCamera" };

        List<ServerContentData> serverContent;

        bool fileDownloadCompleted;
        bool overrideUpdateCheck;
        bool loadAll;

        TextMesh message, shadow;

        public void Initialize(bool overrideUpdateCheck, bool loadAll = false)
        {
            this.overrideUpdateCheck = overrideUpdateCheck;
            this.loadAll = loadAll;

            if (GameObject.Find("MOP_Messager") != null)
            {
                message = GameObject.Find("MOP_Messager").GetComponent<TextMesh>();
                shadow = message.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
            }
            else
            {
                GameObject text = GameObject.Instantiate(GameObject.Find("Interface/Songs/Text"));
                text.transform.localPosition = new Vector3(0, 2.6f, 0.01f);
                text.name = "MOP_Messager";
                text.GetComponent<MeshRenderer>().materials[0].color = Color.white;
                message = text.GetComponent<TextMesh>();
                message.alignment = TextAlignment.Center;
                message.anchor = TextAnchor.UpperCenter;
                shadow = text.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
                shadow.alignment = TextAlignment.Center;
                shadow.anchor = TextAnchor.UpperCenter;
                NewMessage("");
            }

            string issue = "";

            if (!MOP.RulesAutoUpdate.GetValue() && !overrideUpdateCheck)
            {
                issue = "<color=orange>[MOP] Rule files auto update is disabled.</color>";
            }

            // Don't if the update check has been done already.
            if (RulesManager.Instance.UpdateChecked)
            {
                issue = "[MOP] Update check has already been done.";
            }

            // If server or user is offline, skip downloading and simply load available files.
            if (!IsServerOnline())
            {
                issue = "<color=red>[MOP] Connection error. Check your Internet connection.</color>";
            }

            if (issue.Length > 0)
            {
                ModConsole.Log(issue);
                GetAndReadRules();
            }
            else
            {
                StartCoroutine(DownloadAndUpdateRoutine());
            }
        }

        IEnumerator DownloadAndUpdateRoutine()
        {
            MopData rulesInfo = MopSettings.Data;
            Mod[] mods = ModLoader.LoadedMods.Where(m => !m.ID.ContainsAny("MSCLoader_", "MOP")).ToArray();

            ModConsole.Log("[MOP] Checking for new mods...");

            bool isUpdateTime = IsUpdateTime(rulesInfo);

            if (isUpdateTime)
            {
                ModConsole.Log("[MOP] Looking for updates...");
            }
            else
            {
                ModConsole.Log("[MOP] No updates required.");
            }

            List<string> previousMods = rulesInfo.LastModList;
            rulesInfo.LastModList = new List<string>();

            foreach (Mod mod in mods)
            {
                string modId = mod.ID;
                rulesInfo.LastModList.Add(modId);

                string ruleUrl = $"{RemoteServer}/{modId}{RuleExtension}";
                string filePath = $"{MOP.ModConfigPath}/{modId}{RuleExtension}";

                // Continue if it's not time for an update, and the mod is in the list of last mods.
                if (previousMods.Contains(mod.ID) && !isUpdateTime)
                {
                    continue;
                }

                // Check if rule file for mod is on the server.
                // If not, continue.
                if (!IsFileOnServer(modId))
                {
                    continue;
                }

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
                        ModConsole.Log($"<color=orange>[MOP] Skipping {modId}, because local file and remote file are the same.</color>");
                        continue;
                    }
                }

                // Check if file actually exists on remote server.
                if (!RemoteFileExists(ruleUrl))
                {
                    ModConsole.LogError($"[MOP] Rule file for mod doesn't exist!\nID: {modId}\nURL: {ruleUrl}");
                    continue;
                }

                fileDownloadCompleted = false;
                using (WebClient web = new WebClient())
                {
                    ModConsole.Log($"<color=yellow>[MOP] Downloading new rule file for {mod.Name}...</color>");
                    NewMessage($"MOP: Downloading new rule file for {mod.Name}...");
#if DEBUG
                    web.Headers.Add("user-agent", $"MOP/{MOP.ModVersion}_DEBUG {ExceptionManager.GetSystemInfo()}");
#else
                    web.Headers.Add("user-agent", $"MOP/{MOP.ModVersion} {MOP.Edition} {ExceptionManager.GetSystemInfo()}");
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
                            ModConsole.LogError("[MOP] Downloading failed. Skipping downloading.");
                            NewMessage("MOP: Downloading failed. Skipping downloading.");
                            GetAndReadRules();
                            yield break;
                        }
                    }
                }

                ModConsole.Log("<color=green>[MOP] Downloading completed!</color>");
            }

            ModConsole.Log("[MOP] Updating rules completed.");

            if (isUpdateTime)
            {
                rulesInfo.LastTimeUpdate = DateTime.Now;
            }
            MopSettings.WriteData(rulesInfo);

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
                List<FileInfo> files = dir.GetFiles().Where(d => d.Name.EndsWith(RuleExtension) && !d.Name.StartsWith("._")).ToList();
                // Load custom rule file.
                if (File.Exists($"{dir}/{CustomFile}"))
                {
                    files.Add(new FileInfo($"{dir}/{CustomFile}"));
                    ModConsole.Log("[MOP] User custom rule file found!");
                }

                if (files.Count == 0)
                {
                    ModConsole.Log($"[MOP] No rule files found.");
                    NewMessage("");
                    return;
                }

                ModConsole.Log($"[MOP] Found {files.Count} rule file{(files.Count > 1 ? "s" : "")}!");

                int removed = 0;

                // Read rule files.
                foreach (FileInfo file in files)
                {
                    // Delete rules for mods that don't exist.
                    Mod mod = ModLoader.LoadedMods.Find(m => m.ID == Path.GetFileNameWithoutExtension(file.Name));
                    if (mod == null && file.Name != CustomFile && !loadAll)
                    {
                        removed++;
                        if (MOP.DeleteUnusedRules.GetValue())
                        {
                            File.Delete(file.FullName);
                            ModConsole.Log($"<color=yellow>[MOP] Rule file {file.Name} has been deleted, " +
                                             $"because corresponding mod is not present.</color>");
                            removed++;
                            continue;
                        }
                        ModConsole.Log($"<color=yellow>[MOP] Skipped {file.Name} rule, " +
                                         $"because the corresponding mod is not present.</color>");
                        RulesManager.Instance.UnusedRules.Add(file.Name);
                        continue;
                    }

                    ModConsole.Log($"[MOP] Loading {file.Name}...");

                    // Verify if the servercontent has that rule file.
                    // Some mod makers may include poorly configured rule files,
                    // that's why they have to be only provided by the server.
                    if (serverContent != null && MOP.VerifyRuleFiles.GetValue() && file.Name != CustomFile)
                    {
                        if (serverContent.Find(m => m.ID == Path.GetFileNameWithoutExtension(file.Name)) == null)
                        {
                            ModConsole.LogWarning($"[MOP] Rule file {file.Name} has been skipped, because it couldn't be verified.");
                            removed++;
                            continue;
                        }
                    }

                    ReadRulesFromFile(mod, file.FullName);
                }

                int loaded = files.Count - removed;
                ModConsole.Log($"<color=green>[MOP] Loading {loaded}/{files.Count} rule files done!</color>");
                if (loaded == 0)
                {
                    NewMessage($"MOP: No rules have been loaded ({removed} rule{(removed == 1 ? "" : "s")} skipped).");
                }
                else
                {
                    NewMessage($"MOP: Succesfully loaded {loaded} rule file{(loaded == 1 ? "" : "s")}! {(removed > 0 ? $"({removed} rule{(removed == 1 ? "" : "s")} skipped)" : "")}");
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "RULE_FILES_READ_ERROR");
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
                ModConsole.Log("[MOP] Server Status: Online");
                return true;
            }
            catch (Exception)
            {
                ModConsole.Log("[MOP] Server Status: Offline");
                return false;
            }
        }

        /// <summary>
        /// Returns true, if the time saved into the file with added FileThresholdHours is larger than the current time.
        /// </summary>
        bool IsUpdateTime(MopData rulesInfo)
        {
            if (RulesManager.Instance.UpdateChecked)
            {
                return false;
            }

            RulesManager.Instance.UpdateChecked = true;

            int fileThresholdsHours = MopSettings.GetRuleFilesUpdateDaysFrequency() * 24;
            DateTime past = rulesInfo.LastTimeUpdate.AddHours(fileThresholdsHours);
            return DateTime.Now > past;
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
                    string[] serverContentArray = web.DownloadString(new Uri($"{RemoteServer}/{ServerContent}")).Split('\n');
                    serverContent = new List<ServerContentData>();
                    for (int i = 0; i < serverContentArray.Length; i++)
                    {
                        serverContent.Add(new ServerContentData(serverContentArray[i]));
                    }

                    web.Dispose();
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SERVER_CONTENT_DOWNLOAD_ERROR",
                    "MOP could not obtain rule files server content. " +
                    "In order to fix that:\n" +
                    "- Check your firewall configuraiton\n" +
                    "- Check your router DNS configuration\n" +
                    "- Disable 'UPDATE RULES AUTOMATICALLY' option in the settings");
            }
        }

        /// <summary>
        /// Returns the last write time of file.
        /// </summary>
        DateTime GetFileWriteTime(string filename)
        {
            return File.GetLastWriteTime(filename);
        }

        void ReadRulesFromFile(Mod mod, string rulePath)
        {
            try
            {
                string[] content = File.ReadAllLines(rulePath).Where(s => s.Length > 0 && !s.StartsWith("##")).ToArray();
                string fileName = Path.GetFileName(rulePath);
                int lines = File.ReadAllLines(rulePath).Where(s => s.StartsWith("##")).ToArray().Length;
                int currentLine = 0;
                foreach (string s in content)
                {
                    lines++;
                    currentLine++;
                    string[] splitted = s.Split(':');

                    // Read flag and rules.
                    string flag = splitted[0];
                    string[] objects = new string[0];
                    if (splitted.Length > 1)
                    {
                        // Support for object names that have space in it, using quotation mark, instead of using %20.
                        if (splitted[1].Contains("\""))
                        {
                            // Check if there is an odd number of quotation marks.
                            if (splitted[1].Count(f => f == '\"') % 2 != 0)
                            {
                                ModConsole.LogError($"[MOP] Quote hasn't been closed properly: {s} in rule file {fileName}:{lines}.");
                                continue;
                            }

                            string interpret = Regex.Match(splitted[1], "\"[^\"]*\"").Value.Replace(" ", "%20").Replace("\"", "");
                            interpret += splitted[1].Split('"')[2].Replace("\"", "");
                            splitted[1] = interpret;
                        }

                        // Split all objects with space char.
                        objects = splitted[1].Trim().Split(' ');

                        // Replace the %20 in object names to space.
                        for (int i = 0; i < objects.Length; i++)
                            objects[i] = objects[i].Replace("%20", " ");
                    }

                    if (objects.Length > 0 && objects.ContainsAny(illegalValues))
                    {
                        throw new ArgumentException($"[MOP] Illegal object: {objects[0]} in rule file {fileName}.");
                    }

                    // Apply these rules
                    switch (flag)
                    {
                        default:
                            ModConsole.LogError($"[MOP] Unrecognized flag '{flag}' in {fileName}:{lines}.");
                            break;
                        case "ignore":
                            // Ignore at place
                            bool fullIgnore = false;
                            if (objects.Length > 1)
                            {
                                if (objects[objects.Length - 1].ToLower() == "fullignore")
                                {
                                    fullIgnore = true;
                                }
                                else
                                {
                                    RulesManager.Instance.AddRule(new IgnoreRuleAtPlace(mod, fileName, objects[0], objects[1]));
                                    break;
                                }
                            }

                            // Disabling some of the root object is pointless, so we inform abot that the user.

                            RulesManager.Instance.AddRule(new IgnoreRule(mod, fileName, objects[0], fullIgnore));
                            break;
                        case "ignore_full":
                            ObsoleteWarning(flag, fileName, lines, s, "ignore: <object_name> fullIgnore");
                            RulesManager.Instance.AddRule(new IgnoreRule(mod, fileName, objects[0], true));
                            break;
                        case "toggle":
                            ToggleModes mode = ToggleModes.Simple;
                            if (objects.Length > 1)
                            {
                                switch (objects[1])
                                {
                                    default:
                                        ModConsole.LogError($"[MOP] Unrecognized method '{objects[1]}' in {fileName}:{lines}.");
                                        break;
                                    case "renderer":
                                        mode = ToggleModes.Renderer;
                                        break;
                                    case "item":
                                        mode = ToggleModes.Item;
                                        break;
                                    case "vehicle":
                                        mode = ToggleModes.Vehicle;
                                        break;
                                    case "vehicle_physics":
                                        mode = ToggleModes.VehiclePhysics;
                                        break;
                                }
                            }

                            RulesManager.Instance.AddRule(new ToggleRule(mod, fileName, objects[0], mode));
                            break;
                        case "change_parent":
                            if (objects.Length != 2)
                            {
                                throw new ArgumentException("Incorrect use of change_parent. Usage: ObjectName NewParentName");
                            }

                            RulesManager.Instance.AddRule(new ChangeParentRule(mod, fileName, objects[0], objects[1]));
                            break;
                        case "satsuma_ignore_renderer":
                            RulesManager.Instance.SpecialRules.SatsumaIgnoreRenderers = true;
                            break;
                        case "dont_destroy_empty_beer_bottles":
                            ObsoleteWarning(flag, fileName, lines, s, "dont_destroy_empty_bottles");
                            RulesManager.Instance.SpecialRules.DontDestroyEmptyBeerBottles = true;
                            break;
                        case "dont_destroy_empty_bottles":
                            RulesManager.Instance.SpecialRules.DontDestroyEmptyBeerBottles = true;
                            break;
                        case "sector":
                            Vector3 pos = StringToVector3(objects[0]);
                            Vector3 scale = StringToVector3(objects[1]);
                            Vector3 rot = StringToVector3(objects[2]);
                            string[] whitelist = GetWhitelist(objects);
                            RulesManager.Instance.AddRule(new NewSector(mod, fileName, pos, scale, rot, whitelist));
                            break;
                        case "min_ver":
                            if (fileName == CustomFile)
                            {
                                continue;
                            }

                            if (currentLine != 1)
                            {
                                ModConsole.Log($"\n=================================" +
                                $"\n\n<color=cyan>[MOP] Flag '{flag}' must be first in the order!\n\n" +
                                $"File: {fileName}\n" +
                                $"Line: {lines}\n" +
                                $"You can ignore that message.</color>");
                            }

                            int[] minVer = GetVersionFromString(objects[0]);
                            int[] mopVersion = GetVersionFromString(MOP.ModVersionShort);

                            if (IsOutdated(mopVersion, minVer))
                            {
                                ModConsole.LogError($"[MOP] Rule file {fileName} is for the newer version of MOP. Please update MOP right now!\n\n" +
                                                    $"Your MOP version: {mopVersion[0]}.{mopVersion[1]}.{mopVersion[2]}\n" +
                                                    $"Required version: {minVer[0]}.{minVer[1]}.{minVer[2]}");
                                return;
                            }

                            break;
                        case "skip_fury_collider_fix":
                            RulesManager.Instance.SpecialRules.SkipFuryColliderFix = true;
                            break;
                        // Custom.txt exclusives.
                        case "ignore_mod_vehicles":
                            if (fileName != CustomFile)
                            {
                                ModConsole.LogError($"[MOP] Flag: {flag} is only allowed to be used in custom rule file.");
                                continue;
                            }
                            RulesManager.Instance.SpecialRules.IgnoreModVehicles = true;
                            break;
                        case "toggle_all_vehicles_physics_only":
                            if (fileName != CustomFile)
                            {
                                ModConsole.LogError($"[MOP] Flag: {flag} is only allowed to be used in custom rule file.");
                                continue;
                            }
                            RulesManager.Instance.SpecialRules.ToggleAllVehiclesPhysicsOnly = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ModConsole.LogError($"[MOP] Error loading rule {Path.GetFileName(rulePath)}: {ex.Message}.");
                NewMessage($"<color=red>MOP: Error loading rule :(");
            }
        }

        /// <summary>
        /// Sets the text of MOP_Messager object.
        /// </summary>
        /// <param name="value"></param>
        void NewMessage(string value)
        {
            string text = $"<color=yellow>{value}</color>";

            if (!string.IsNullOrEmpty(MOP.SubVersion))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    text += "\n";
                }
                text += $"<color=magenta>MOP {MOP.ModVersion.Replace("_", " ").Replace("-", " ")}</color>";
            }

            message.text = text;
            shadow.text = text;
        }

        /// <summary>
        /// Reands and parses the string to Vector3.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        Vector3 StringToVector3(string s)
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
                ModConsole.LogError($"[MOP] Incorrect vector format! Refer to MOP wiki.");
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

        void ObsoleteWarning(string flag, string fileName, int lines, string content, string alternative)
        {
            ModConsole.Log($"\n=================================" +
                            $"\n\n<color=cyan>[MOP] Flag '{flag}' is obsolete.\n" +
                            $"Please use '{alternative}' instead!\n\n" +
                            $"File: {fileName}\n" +
                            $"Line: {lines}\n" +
                            $"Context: {content}\n\n" +
                            $"You can ignore that message.</color>");
        }

        void WrongArgumentWarning(string reason, string fileName, int lines, string content)
        {
            ModConsole.Log($"\n=================================" +
                            $"\n\n<color=cyan>[MOP] {reason}.\n\n" +
                            $"File: {fileName}\n" +
                            $"Line: {lines}\n" +
                            $"Context: {content}\n\n" +
                            $"You can ignore that message.</color>");
        }

        int[] GetVersionFromString(string s)
        {
            int[] version = new int[3];
            string[] splitted = s.Split('.');
            for (int i = 0; i < splitted.Length; ++i)
            {
                if (int.TryParse(splitted[i], out int digit))
                {
                    version[i] = digit;
                }
                else
                {
                    version[i] = 0;
                }
            }

            return version;
        }

        bool IsOutdated(int[] mopVersion, int[] minimalVersion)
        {
            // This array keeps track of numbers that are equal.
            bool[] isEqual = new bool[mopVersion.Length];
            for (int i = 0; i < mopVersion.Length; ++i)
            {
                if (i == 0)
                {
                    if (mopVersion[i] < minimalVersion[i])
                    {
                        return true;
                    }
                    else if (mopVersion[i] > minimalVersion[i])
                    {
                        return false;
                    }
                }
                else
                {
                    // If index is more than 0, make sure that the previou set of numbers is equal - otherwise this comparison does not make sense.
                    if (isEqual[i - 1] && mopVersion[i] < minimalVersion[i])
                    {
                        return true;
                    }
                }

                isEqual[i] = mopVersion[i] == minimalVersion[i];
            }

            return false;
        }
    }
}
