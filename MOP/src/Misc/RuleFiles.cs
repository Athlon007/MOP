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
    class IgnoreRule
    {
        public string ObjectName;
        public bool Ignore;
        public bool TotalIgnore;

        public IgnoreRule(string ObjectName, bool Ignore, bool TotalIgnore)
        {
            this.ObjectName = ObjectName;
            this.Ignore = Ignore;
            this.TotalIgnore = TotalIgnore;
        }
    }

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

    class RuleFiles
    {
        public static RuleFiles instance;

        public List<IgnoreRule> IgnoreRules;
        public List<ToggleRule> ToggleRules;

        internal string MopConfigFolder;

        public RuleFiles(string mopConfigFolder)
        {
            instance = this;
            IgnoreRules = new List<IgnoreRule>();
            ToggleRules = new List<ToggleRule>();

            MopConfigFolder = mopConfigFolder;

            string modsConfig = mopConfigFolder.Substring(0, mopConfigFolder.LastIndexOf('\\'));
            string[] dirs = Directory.GetDirectories(modsConfig);
            List<string> ruleFiles = new List<string>();
            foreach (string dir in dirs)
            {
                // Ignore MSCLoader folders
                if (dir.ContainsAny("MSCLoader_", "MOP")) continue;

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
            ModConsole.Print($"[MOP] Found {ruleFiles.Count} rule file{(ruleFiles.Count > 1 ? "s" : "")}!");
            foreach (string ruleFile in ruleFiles)
            {
                ReadRules(ruleFile);
            }

            ModConsole.Print("[MOP] Loading rule files done!");
        }


        void ReadRules(string rulePath)
        {
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

            string[] content = File.ReadAllLines(rulePath).Where(s => s.Length > 0 && !s.StartsWith("##") && s.Contains(":")).ToArray();
            foreach (string s in content)
            {
                string flag = s.Split(':')[0];
                string value = s.Split(':')[1].Trim();

                switch (flag)
                {
                    default:
                        ModConsole.Error($"[MOP] Unrecognized flag '{flag}' in file {rulePath}");
                        break;
                    case "ignore":
                        IgnoreRules.Add(new IgnoreRule(value, true, false));
                        break;
                    case "ignore_full":
                        IgnoreRules.Add(new IgnoreRule(value, true, true));
                        break;
                    case "toggle":
                        ToggleRules.Add(new ToggleRule(value, ToggleModes.Normal));
                        break;
                    case "toggle_renderer":
                        ToggleRules.Add(new ToggleRule(value, ToggleModes.Renderer));
                        break;
                    case "toggle_as_item":
                        ToggleRules.Add(new ToggleRule(value, ToggleModes.Item));
                        break;
                    case "toggle_as_vehicle":
                        ToggleRules.Add(new ToggleRule(value, ToggleModes.Vehicle));
                        break;
                    case "toggle_as_vehicle_physics_only":
                        ToggleRules.Add(new ToggleRule(value, ToggleModes.VehiclePhysics));
                        break;
                }
            }
        }
    }

    public class GenerateNewFileCommand : ConsoleCommand
    {
        public override string Name => "MOP";
        public override string Help => "- new: generate new .mopconfig file";

        const string MopConfigTemplate = "## MOP Config File\n\n" + 
                                         "## Visit https://github.com/Athlon007/MOP/wiki to learn how to configurate this file!\n\n";

        public override void Run(string[] args)
        {
            switch(args[0])
            {
                default:
                    ModConsole.Error($"[MOP] Unknown command \"{args[0]}\".");
                    break;
                case "new":
                    string newFilePath = RuleFiles.instance.MopConfigFolder + "\\template.mopconfig";
                    File.WriteAllText(newFilePath, MopConfigTemplate);
                    ModConsole.Print($"<color=green>[MOP] Created new .mopconfig file!</color>");
                    ModConsole.Print($"[MOP] A file has been saved to {newFilePath}");
                    break;
            }
        }
    }
}
