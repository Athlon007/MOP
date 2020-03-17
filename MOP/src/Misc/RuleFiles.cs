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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MOP
{
    class VehicleRule
    {
        public string VehicleName;
        public bool Ignore;
        public bool TotalIgnore;

        public VehicleRule(string VehicleName, bool Ignore, bool TotalIgnore)
        {
            this.VehicleName = VehicleName;
            this.Ignore = Ignore;
            this.TotalIgnore = TotalIgnore;
        }
    }

    class Rules
    {
        public List<VehicleRule> VehicleRules = new List<VehicleRule>();

        public List<string> IgnoredObjects = new List<string>();
        public List<string> IgnoredTotalObjects = new List<string>();
    }

    class RuleFiles
    {
        public static RuleFiles instance;

        public Rules ActiveRules;

        public RuleFiles(string mopConfigFolder)
        {
            instance = this;
            ActiveRules = new Rules();

            string modsConfig = mopConfigFolder.Substring(0, mopConfigFolder.LastIndexOf('\\'));
            string[] dirs = Directory.GetDirectories(modsConfig);
            List<string> ruleFiles = new List<string>();
            foreach (string dir in dirs)
            {
                // Ignore MSCLoader folders
                if (dir.Contains("MSCLoader_")) continue;

                DirectoryInfo di = new DirectoryInfo(dir);
                FileInfo[] files = di.GetFiles("*.mopconfig");

                if (files.Length == 0) continue;

                foreach (FileInfo file in files)
                    ruleFiles.Add(file.FullName);
            }

            // If no rule files have been found, quit.
            if (ruleFiles.Count == 0)
            {
                ModConsole.Print($"[MOP] No rule files found");
                return;
            }

            // Read rule files, if some have been found.
            ModConsole.Print($"[MOP] Found {ruleFiles.Count} rule files!");
            foreach (string ruleFile in ruleFiles)
            {
                ReadRules(ruleFile);
            }

            ModConsole.Print("[MOP] Loading rule files done!");
        }

        void ReadRules(string rulePath)
        {
            string[] content = File.ReadAllLines(rulePath).Where(s => s.Length > 0 && !s.StartsWith("##") && s.Contains(":")).ToArray();
            foreach (string s in content)
            {
                string flag = s.Split(':')[0];
                string value = s.Split(':')[1].Trim();

                switch (flag)
                {
                    case "ignore":
                        NewIgnoreRule(value);
                        break;
                    case "ignore_full":
                        NewIgnoreRule(value, true);
                        break;
                }
            }
        }

        void NewIgnoreRule(string value, bool full = false)
        {
            switch (value)
            {
                default:
                    if (full)
                    {
                        if (!ActiveRules.IgnoredTotalObjects.Contains(value))
                            ActiveRules.IgnoredTotalObjects.Add(value);
                        return;
                    }

                    if (!ActiveRules.IgnoredObjects.Contains(value))
                        ActiveRules.IgnoredObjects.Add(value);
                    break;
                case "SATSUMA":
                    ActiveRules.VehicleRules.Add(new VehicleRule("SATSUMA(557kg, 248)", true, full));
                    break;
                case "HAYOSIKO":
                    ActiveRules.VehicleRules.Add(new VehicleRule("HAYOSIKO(1500kg, 250)", true, full));
                    break;
                case "JONNEZ_ES":
                    ActiveRules.VehicleRules.Add(new VehicleRule("JONNEZ ES(Clone)", true, full));
                    break;
                case "KEKMET":
                    ActiveRules.VehicleRules.Add(new VehicleRule("KEKMET(350-400psi)", true, full));
                    break;
                case "RUSCKO":
                    ActiveRules.VehicleRules.Add(new VehicleRule("RCO_RUSCKO12(270)", true, full));
                    break;
                case "FERNDALE":
                    ActiveRules.VehicleRules.Add(new VehicleRule("FERNDALE(1630kg)", true, full));
                    break;
                case "FLATBED":
                    ActiveRules.VehicleRules.Add(new VehicleRule("FLATBED", true, full));
                    break;
                case "GIFU":
                    ActiveRules.VehicleRules.Add(new VehicleRule("GIFU(750/450psi)", true, full));
                    break;
            }
        }
    }
}
