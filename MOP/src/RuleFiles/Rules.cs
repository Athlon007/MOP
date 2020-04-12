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
using System.Collections.Generic;
using UnityEngine;

namespace MOP
{
    class Rules
    {
        public static Rules instance;

        // Ignore rules.
        public List<IgnoreRule> IgnoreRules;
        public List<IgnoreRuleAtPlace> IgnoreRulesAtPlaces;
        public List<PreventToggleOnObjectRule> PreventToggleOnObjectRule;

        // Toggling rules.
        public List<ToggleRule> ToggleRules;

        // Special rules.
        public SpecialRules SpecialRules;

        // Used for mod report only.
        public List<string> RuleFileNames;

        // Rules applied by sectors.
        public List<string> SectorRules;

        public Rules(bool overrideUpdateCheck = false)
        {
            instance = this;

            IgnoreRules = new List<IgnoreRule>();
            IgnoreRulesAtPlaces = new List<IgnoreRuleAtPlace>();
            PreventToggleOnObjectRule = new List<PreventToggleOnObjectRule>();

            ToggleRules = new List<ToggleRule>();

            SpecialRules = new SpecialRules();

            RuleFileNames = new List<string>();

            SectorRules = new List<string>();

            // Destroy old rule files loader object, if it exists.
            GameObject oldRuleFilesLoader = GameObject.Find("MOP_RuleFilesLoader");
            if (oldRuleFilesLoader != null)
                GameObject.Destroy(oldRuleFilesLoader);

            GameObject ruleFileDownloader = new GameObject("MOP_RuleFilesLoader");
            RuleFilesLoader ruleFilesLoader = ruleFileDownloader.AddComponent<RuleFilesLoader>();
            ruleFilesLoader.Initialize(overrideUpdateCheck);
        }

        public void AddSectorRule(params string[] names)
        {
            SectorRules.AddRange(names);
        }

        public void ClearSectorRules()
        {
            SectorRules = new List<string>();
        }

        public bool SectorRulesContains(string name)
        {
            if (SectorRules.Count == 0)
                return false;

            return SectorRules.Contains(name);
        }
    }

    class PrintRulesCommand : ConsoleCommand
    {
        public override string Name => "mop-rules";
        public override string Help => "Prints the list of active rules";
        public override bool ShowInHelp => true;
        public override void Run(string[] args)
        {
            ModConsole.Print("<b>Ignore Rules</b>");
            foreach (IgnoreRule r in Rules.instance.IgnoreRules)
                ModConsole.Print($"Object: {r.ObjectName}");

            ModConsole.Print("<b>Ignore Rules At Place</b>");
            foreach (IgnoreRuleAtPlace r in Rules.instance.IgnoreRulesAtPlaces)
                ModConsole.Print($"Place: {r.Place} Object: {r.ObjectName}");

            ModConsole.Print("<b>Prevent Toggle On Object Rule</b>");
            foreach (PreventToggleOnObjectRule r in Rules.instance.PreventToggleOnObjectRule)
                ModConsole.Print($"Main Object: {r.MainObject} Object: {r.ObjectName}");

            ModConsole.Print("<b>Toggle Rules</b>");
            foreach (ToggleRule r in Rules.instance.ToggleRules)
                ModConsole.Print($"Object: {r.ObjectName} Toggle Mode: {r.ToggleMode}");

            ModConsole.Print("<b>Special Rules</b>");
            ModConsole.Print($"DontDestroyEmptyBeerBottles: {Rules.instance.SpecialRules.DontDestroyEmptyBeerBottles}");
            ModConsole.Print($"SatsumaIgnoreRenderers: {Rules.instance.SpecialRules.SatsumaIgnoreRenderers}");
        }
    }
}