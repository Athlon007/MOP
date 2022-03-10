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

using System.Collections.Generic;
using System.IO;
using UnityEngine;

using MOP.Rules.Configuration;
using MOP.Rules.Types;
using MSCLoader;

namespace MOP.Rules
{
    class RulesManager
    {
        static RulesManager instance;
        public static RulesManager Instance { get => instance; }

        // Ignore rules.
        public List<IgnoreRule> IgnoreRules;
        public List<IgnoreRuleAtPlace> IgnoreRulesAtPlaces;

        // Toggling rules.
        public List<ToggleRule> ToggleRules;

        // Special rules.
        public SpecialRules SpecialRules;

        // Used for mod report only.
        public List<string> RuleFileNames;

        // Sectors (adds a new sector).
        public List<NewSector> NewSectors;

        // Change Parent
        public List<ChangeParentRule> ChangeParentRules;

        // If false, no rule files will be loaded.
        public bool LoadRules = true;

        public bool UpdateChecked;

        public List<string> UnusedRules;

        public RulesManager(bool overrideUpdateCheck = false)
        {
            instance = this;
            WipeAll(overrideUpdateCheck);
        }

        void ResetLists()
        {
            IgnoreRules = new List<IgnoreRule>();
            IgnoreRulesAtPlaces = new List<IgnoreRuleAtPlace>();
            ToggleRules = new List<ToggleRule>();
            SpecialRules = new SpecialRules();
            RuleFileNames = new List<string>();
            NewSectors = new List<NewSector>();
            UnusedRules = new List<string>();
            ChangeParentRules = new List<ChangeParentRule>();
        }

        public void WipeAll(bool overrideUpdateCheck)
        {
            ResetLists();

            // Destroy old rule files loader object, if it exists.
            GameObject oldRuleFilesLoader = GameObject.Find("MOP_RuleFilesLoader");
            if (oldRuleFilesLoader != null)
                Object.Destroy(oldRuleFilesLoader);

            GameObject ruleFileDownloader = new GameObject("MOP_RuleFilesLoader");
            Loader ruleFilesLoader = ruleFileDownloader.AddComponent<Loader>();
            ruleFilesLoader.Initialize(overrideUpdateCheck);
        }

        /// <summary>
        /// Triggered by Hypervisor, if the LoadRules is set to false.
        /// </summary>
        public void Unload()
        {
            ResetLists();
        }

        public void DeleteAll()
        {
            DirectoryInfo di = new DirectoryInfo(MOP.ModConfigPath);
            foreach (var f in di.GetFiles("*.mopconfig"))
                f.Delete();
        }

        public static void DeleteUnused()
        {
            if (Instance.UnusedRules.Count == 0)
            {
                ModUI.ShowMessage("No unused rule files found.", "MOP");
                return;
            }
            else
            {
                ModUI.ShowYesNoMessage($"Are you sure you want to delete <color=yellow>{Instance.UnusedRules.Count}</color> unused rule file{(Instance.UnusedRules.Count == 1 ? "" : "s")}?", 
                                       "MOP", DoDeleteUnused);
            }
        }

        static void DoDeleteUnused()
        {
            foreach (string file in Instance.UnusedRules)
            {
                string path = Path.Combine(MOP.ModConfigPath, file).Replace('\\', '/');
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Instance.UnusedRules.Remove(file);
                }
                else
                {
                    ModConsole.LogError($"[MOP] Can't find path to file: {path}");
                }
            }
        }

        public bool IsObjectInIgnoreList(GameObject gm)
        {
            return IgnoreRules.Find(g => g.ObjectName == gm.name) != null;
        }
    }
}
