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
    class Rule
    {
        public string ObjectName;
        public bool Ignore;
        public bool TotalIgnore;

        public Rule(string ObjectName, bool Ignore, bool TotalIgnore)
        {
            this.ObjectName = ObjectName;
            this.Ignore = Ignore;
            this.TotalIgnore = TotalIgnore;
        }
    }

    class RuleFiles
    {
        public static RuleFiles instance;

        public List<Rule> Rules;

        public RuleFiles(string mopConfigFolder)
        {
            instance = this;
            Rules = new List<Rule>();

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
                        Rules.Add(new Rule(value, true, false));
                        break;
                    case "ignore_full":
                        Rules.Add(new Rule(value, true, true));
                        break;
                }
            }
        }
    }
}
