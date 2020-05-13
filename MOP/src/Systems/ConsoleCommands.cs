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
using System.IO;
using System.Diagnostics;
using System;

namespace MOP
{
    class ConsoleCommands : ConsoleCommand
    {
        public override string Name => "mop";
        public override string Help => "Use \"mop help\" to see command list!";
        public override bool ShowInHelp => true;
        public override void Run(string[] args)
        {
            if (args.Length == 0)
            {
                ModConsole.Print("See \"mop help\" for command list.");
                return;
            }

            switch (args[0])
            {
                default:
                    ModConsole.Print("Invalid command. See \"mop help\" for command list.");
                    break;
                case "help":
                    ModConsole.Print("<color=yellow>help</color> - Show this list\n" +
                        "<color=yellow>version</color> - Prints MOP version\n" +
                        "<color=yellow>rules</color> - Show the list of active rules and loaded rule files\n" +
                        "<color=yellow>wiki</color> - Open wiki page of rule files\n" +
                        "<color=yellow>reload</color> - Forces MOP to reload rule files\n" +
                        "<color=yellow>new</color> - Create custom rule file\n" +
                        "<color=yellow>open-custom</color> - Open custom rule file\n" +
                        "<color=yellow>delete-custom</color> - Delete custom rule file");
                    break;
                case "rules":
                    if (Rules.instance.IgnoreRules.Count > 0)
                    {
                        ModConsole.Print("<color=yellow><b>Ignore Rules</b></color>");
                        foreach (IgnoreRule r in Rules.instance.IgnoreRules)
                            ModConsole.Print($"<b>Object:</b> {r.ObjectName}");
                    }

                    if (Rules.instance.IgnoreRulesAtPlaces.Count > 0)
                        {
                        ModConsole.Print("\n<color=yellow><b>Ignore Rules At Place</b></color>");
                        foreach (IgnoreRuleAtPlace r in Rules.instance.IgnoreRulesAtPlaces)
                            ModConsole.Print($"<b>Place:</b> {r.Place} <b>Object:</b> {r.ObjectName}");
                    }

                    if (Rules.instance.PreventToggleOnObjectRule.Count > 0)
                    {
                        ModConsole.Print("\n<color=yellow><b>Prevent Toggle On Object Rule</b></color>");
                        foreach (PreventToggleOnObjectRule r in Rules.instance.PreventToggleOnObjectRule)
                            ModConsole.Print($"<b>Main Object:</b> {r.MainObject} <b>Object:</b> {r.ObjectName}");
                    }

                    if (Rules.instance.ToggleRules.Count > 0)
                    {
                        ModConsole.Print("\n<color=yellow><b>Toggle Rules</b></color>");
                        foreach (ToggleRule r in Rules.instance.ToggleRules)
                            ModConsole.Print($"<b>Object:</b> {r.ObjectName} <b>Toggle Mode:</b> {r.ToggleMode}");
                    }

                    if (Rules.instance.NewSectors.Count > 0)
                    {
                        ModConsole.Print("\n<color=yellow><b>New Sectors</b></color>");
                        foreach (NewSector r in Rules.instance.NewSectors)
                            ModConsole.Print($"<b>Pos:</b> {r.Position} <b>Scale:</b> {r.Scale} <b>Rot:</b> {r.Rotation} <b>Ignore:</b> {string.Join(", ", r.Whitelist)}");
                    }

                    ModConsole.Print("\n<color=yellow><b>Special Rules</b></color>");
                    ModConsole.Print($"<b>DontDestroyEmptyBeerBottles:</b> {Rules.instance.SpecialRules.DontDestroyEmptyBeerBottles}");
                    ModConsole.Print($"<b>SatsumaIgnoreRenderers:</b> {Rules.instance.SpecialRules.SatsumaIgnoreRenderers}");
                    ModConsole.Print($"<b>DrivewaySector:</b> {Rules.instance.SpecialRules.DrivewaySector}");
                    ModConsole.Print($"<b>ExperimentalSatsumaTrunk:</b> {Rules.instance.SpecialRules.ExperimentalSatsumaTrunk}");
                    ModConsole.Print($"<b>ExperimentalOptimization:</b> {Rules.instance.SpecialRules.ExperimentalOptimization}");

                    // List rule files.
                    string output = "\n<color=yellow><b>Rule Files</b></color>\n";
                    foreach (string ruleFile in Rules.instance.RuleFileNames)
                        output += $"{ruleFile}\n";

                    ModConsole.Print(output);
                    break;
                case "wiki":
                    Process.Start("https://github.com/Athlon007/MOP/wiki/Rule-Files-Documentation");
                    break;
                case "reload":
                    if (ModLoader.GetCurrentScene() != CurrentScene.MainMenu)
                    {
                        ModConsole.Print("You can only reload rule files in the main menu");
                        return;
                    }

                    Rules.instance.WipeAll();
                    break;
                case "new":
                    string path = $"{MOP.ModConfigPath}\\Custom.txt";

                    if (File.Exists(path))
                    {
                        ModConsole.Print("Custom file already exists. Use \"mop open\" to edit it now.");
                        return;
                    }

                    File.WriteAllText(path, "## Every line which starts with ## will be ignored.\n" +
                        "## All new flags MUST be written in a new line." + 
                        "## Visit https://github.com/Athlon007/MOP/wiki/Rule-Files-Documentation for documentation.\n" +
                        "## WARNING: Using custom rule files may cause issues. Use only at your own risk!");

                    Process.Start(path);
                    ModConsole.Print("A custom rule file has been created. You can find it as Custom.txt.\n" +
                        "<color=red>Careless use of rule files may cause bugs and glitchess. Use only at yout own risk!</color>");
                    break;
                case "open-custom":
                    if (!File.Exists($"{MOP.ModConfigPath}\\Custom.txt"))
                    {
                        ModConsole.Print("<color=red>Custom rule file doesn't exist. Create one using \"mop new\".</color>");
                        return;
                    }

                    Process.Start($"{MOP.ModConfigPath}\\Custom.txt");
                    ModConsole.Print("Custom rule file opened");
                    break;
                case "delete-custom":
                    if (!File.Exists($"{MOP.ModConfigPath}\\Custom.txt"))
                    {
                        ModConsole.Print("<color=red>Custom rule file doesn't exist.</color>");
                        return;
                    }
                    
                    File.Delete($"{MOP.ModConfigPath}\\Custom.txt");
                    ModConsole.Print("Custom file succesfully deleted. Use \"mop reload\" to reload the rule files list.");
                    break;
                case "version":
                    ModConsole.Print(MOP.ModVersion);
                    break;
                case "cowsay":
                    string say = String.Join(" ", args, 1, args.Length - 1);

                    if (say == "Tell me your secrets")
                    {
                        say = "all pls fix and no appreciation makes Athlon an angry boy";
                    }

                    if (say == "Tell me your wisdoms")
                    {
                        say = "people saying that MOP is just improved KruFPS are straight up wrong";
                    }

                    ModConsole.Print($"< {say} >\n" +
                                    "        \\   ^__^\n" +
                                    "         \\  (oo)\\____\n" +
                                    "            (__)\\          )\\/\\\n" +
                                    "                ||  ----w  |\n" +
                                    "                ||           || ");
                    break;
                case "sector_debug":
                    if (args.Length == 1)
                    {
                        ModConsole.Print($"Sector debug mode is set to {MopSettings.SectorDebugMode}");
                        return;
                    }
                    MopSettings.SectorDebugMode = args[1].ToLower() == "true";
                    ModConsole.Print($"Sector debug mode is {(MopSettings.SectorDebugMode ? "on" : "off")}!");
                    break;
            }
        }
    }
}
