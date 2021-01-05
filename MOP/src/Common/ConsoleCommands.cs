// Modern Optimization Plugin
// Copyright(C) 2019-2021 Athlon

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
using System.Reflection;
using UnityEngine;

using MOP.Helpers;
using MOP.Rules;
using MOP.Rules.Types;

namespace MOP.Common
{
    class ConsoleCommands : ConsoleCommand
    {
        public override string Name => "mop";
        public override string Help => "Use \"mop help\" to see command list!";
        public override bool ShowInHelp => true;

        const string HelpList = "<color=yellow>help</color> - Show this list\n" +
                                "<color=yellow>version</color> - Prints MOP version\n" +
                                "<color=yellow>rules</color> - Show the list of active rules and loaded rule files\n" +
                                "<color=yellow>wiki</color> - Open wiki page of rule files\n" +
                                "<color=yellow>reload</color> - Forces MOP to reload rule files\n" +
                                "<color=yellow>new [ModID]</color> - Create custom rule file (if no ModID is provided, will create Custom.txt)\n" +
                                "<color=yellow>open [ModID]</color> - Opens the .modconfig for mod\n" +
                                "<color=yellow>open-config</color> - Opens MOP config folder\n" +
                                "<color=yellow>open-logs</color> - Opens logs folder\n" +
                                "<color=yellow>delete [ModID]</color> - Delete rule file\n" +
                                "<color=yellow>cat [File Name]</color> - Print the content of a rule file\n" +
                                "<color=yellow>generate-list [true/false]</color> - Generates text files which contain the list of items that are toggled by MOP\n" +
                                "<color=yellow>restore-save</color> - If you use experimental save optimiztion, allows you to restore your last save file.\n" +
                                "<color=yellow>load-rules [true/false]</color> - If set to false, prevents MOP from loading any rule files.";

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
                    ModConsole.Print("Invalid command. Type \"mop help\" for command list.");
                    break;
                case "help":
                    if (args.Length > 1)
                    {
                        string[] helpList = HelpList.Split('\n');
                        bool commandsFound = false;
                        foreach (string s in helpList)
                        {
                            if (s.Split('-')[0].Contains(args[1]))
                            {
                                ModConsole.Print(s);
                                commandsFound = true;
                            }
                        }
                        if (!commandsFound)
                            ModConsole.Print($"Command {args[1]} not found.");
                        return;
                    }
                    ModConsole.Print(HelpList);
                    break;
                case "rules":
                    if (args.Length > 1 && args[1] == "roll")
                    {
                        ModConsole.Print("\n<color=yellow>You know the rules and so do I\n" +
                                        "A full commitment's what I'm thinking of\n" +
                                        "You wouldn't get this from any other guy\n" +
                                        "I just wanna tell you how I'm feeling\n" +
                                        "Gotta make you understand\n" +
                                        "Never gonna give you up\n" +
                                        "Never gonna let you down\n" +
                                        "Never gonna run around and desert you\n" +
                                        "Never gonna make you cry\n" +
                                        "Never gonna say goodbye\n" +
                                        "Never gonna tell a lie and hurt you</color>\n\n");
                        return;
                    }

                    if (RulesManager.Instance.IgnoreRules.Count > 0)
                    {
                        ModConsole.Print("<color=yellow><b>Ignore Rules</b></color>");
                        foreach (IgnoreRule r in RulesManager.Instance.IgnoreRules)
                            ModConsole.Print($"<b>Object:</b> {r.ObjectName}");
                    }

                    if (RulesManager.Instance.IgnoreRulesAtPlaces.Count > 0)
                        {
                        ModConsole.Print("\n<color=yellow><b>Ignore Rules At Place</b></color>");
                        foreach (IgnoreRuleAtPlace r in RulesManager.Instance.IgnoreRulesAtPlaces)
                            ModConsole.Print($"<b>Place:</b> {r.Place} <b>Object:</b> {r.ObjectName}");
                    }

                    if (RulesManager.Instance.ToggleRules.Count > 0)
                    {
                        ModConsole.Print("\n<color=yellow><b>Toggle Rules</b></color>");
                        foreach (ToggleRule r in RulesManager.Instance.ToggleRules)
                            ModConsole.Print($"<b>Object:</b> {r.ObjectName} <b>Toggle Mode:</b> {r.ToggleMode}");
                    }

                    if (RulesManager.Instance.NewSectors.Count > 0)
                    {
                        ModConsole.Print("\n<color=yellow><b>New Sectors</b></color>");
                        foreach (NewSector r in RulesManager.Instance.NewSectors)
                            ModConsole.Print($"<b>Pos:</b> {r.Position} <b>Scale:</b> {r.Scale} <b>Rot:</b> {r.Rotation} <b>Ignore:</b> {string.Join(", ", r.Whitelist)}");
                    }

                    ModConsole.Print("\n<color=yellow><b>Special Rules</b></color>");
                    // Obtain all fields
                    FieldInfo[] fields = typeof(SpecialRules).GetFields();
                    // Loop through fields
                    foreach (var field in fields) 
                    {
                        ModConsole.Print($"<b>{field.Name}</b>: {field.GetValue(RulesManager.Instance.SpecialRules)}");
                    }

                    // List rule files.
                    string output = "\n<color=yellow><b>Rule Files</b></color>\n";
                    foreach (string ruleFile in RulesManager.Instance.RuleFileNames)
                        output += $"{ruleFile}\n";

                    ModConsole.Print(output);
                    break;
                case "wiki":
                    ExternalExecuting.OpenWikiDialog();
                    break;
                case "reload":
                    if (ModLoader.GetCurrentScene() != CurrentScene.MainMenu)
                    {
                        ModConsole.Print("You can only reload rule files in the main menu");
                        return;
                    }

                    RulesManager.Instance.WipeAll(false);
                    break;
                case "new":
                    string path = $"{MOP.ModConfigPath}/Custom.txt";

                    if (args.Length > 1)
                    {
                        path = $"{MOP.ModConfigPath}/{args[1]}.mopconfig";
                    }

                    if (File.Exists(path))
                    {
                        ModConsole.Print("Custom file already exists. Use \"mop open\" to edit it now.");
                        return;
                    }

                    File.WriteAllText(path, "## Every line which starts with ## will be ignored.\n" +
                                            "## All new flags MUST be written in a new line.\n" +
                                            "## Visit http://athlon.kkmr.pl/mop/wiki/#/rulefiles_commands for documentation.\n" +
                                            "## WARNING: Using custom rule files may cause issues. Use only at your own risk!");

                    Process.Start(path);
                    if (path.EndsWith("Custom.txt"))
                    {
                        ModConsole.Print("A custom rule file has been created. You can find it as Custom.txt.\n" +
                            "<color=red>Careless use of rule files may cause bugs and glitchess. Use only at yout own risk!</color>");
                    }
                    else
                    {
                        ModConsole.Print($"A rule file for {args[1]} mod has been created.");
                    }
                    break;
                case "version":
                    ModConsole.Print(MOP.ModVersion);
                    break;
                case "cowsay":
                    string say = string.Join(" ", args, 1, args.Length - 1);

                    switch (say.ToLower())
                    {
                        case "tell me your secrets":
                            say = "all pls fix and no appreciation makes Athlon an angry boy";
                            break;
                        case "tell me your wisdoms":
                            say = "people saying that MOP is just improved KruFPS are straight up wrong";
                            break;
                        case "wieski":
                            say = "it really do be like dat doe sometimes";
                            break;
                        case "embu":
                            say = "pee vee good";
                            break;
                        case "owo":
                            say = "UwU";
                            break;
                        case "uwu":
                            say = "OwO";
                            break;
                        case "mop sucks":
                            say = "no u";
                            Process.Start("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
                            break;
                    }

                    ModConsole.Print($"< {say} >\n" +
                                    "        \\   ^__^\n" +
                                    "         \\  (oo)\\____\n" +
                                    "            (__)\\          )\\/\\\n" +
                                    "                ||  ----w  |\n" +
                                    "                ||           || ");
                    break;
                case "open-config":
                    Process.Start(MOP.ModConfigPath);
                    break;
                case "open-logs":
                    Process.Start($"{Application.dataPath}/{ExceptionManager.LogFolder}");
                    break;
                case "open":
                    if (args.Length == 1)
                    {
                        ModConsole.Print($"Missing argument.");
                        return;
                    }

                    if (args[1].StartsWith("Custom") || args[1].StartsWith("custom"))
                    {
                        if (!args[1].EndsWith(".txt"))
                            args[1] += ".txt";
                    }
                    else
                    {
                        if (!args[1].EndsWith(".mopconfig"))
                            args[1] += ".mopconfig";
                    }

                    if (!File.Exists($"{MOP.ModConfigPath}/{args[1]}"))
                    { 
                        ModConsole.Print($"File {args[1]} doesn't exist.");
                        return;
                    }

                    Process.Start($"{MOP.ModConfigPath}/{args[1]}");
                    break;
                case "delete":
                    if (args.Length == 1)
                    {
                        ModConsole.Print($"Missing argument.");
                        return;
                    }

                    if (args[1].StartsWith("Custom") && !args[1].EndsWith(".txt"))
                    {
                        args[1] += ".txt";
                    }
                    else
                    {
                        if (!args[1].EndsWith(".mopconfig"))
                            args[1] += ".mopconfig";
                    }

                    if (!File.Exists($"{MOP.ModConfigPath}/{args[1]}"))
                    {
                        ModConsole.Print($"File {args[1]} doesn't exist.");
                        return;
                    }

                    File.Delete($"{MOP.ModConfigPath}/{args[1]}");
                    break;
                case "cat":
                    if (args.Length == 1)
                    {
                        ModConsole.Print($"Missing argument.");
                        return;
                    }

                    if (args[1].StartsWith("Custom") && !args[1].EndsWith(".txt"))
                    {
                        args[1] += ".txt";
                    }
                    else
                    {
                        if (!args[1].EndsWith(".mopconfig"))
                            args[1] += ".mopconfig";
                    }

                    if (!File.Exists($"{MOP.ModConfigPath}/{args[1]}"))
                    {
                        ModConsole.Print($"File {args[1]} doesn't exist.");
                        return;
                    }

                    ModConsole.Print(File.ReadAllText($"{MOP.ModConfigPath}/{args[1]}"));
                    break;
                case "restore-save":
                    if (ModLoader.GetCurrentScene() != CurrentScene.MainMenu)
                    {
                        ModConsole.Print("You can only restore game save in the main menu.");
                        break;
                    }

                    bool defaultBackupMissing = false;
                    bool itemsBackupMissing = false;
                    if (!File.Exists(SaveManager.GetDefaultES2SavePosition() + ".mopbackup"))
                    {
                        defaultBackupMissing = true;
                        ModConsole.Print("defaultES2Save.txt.mopbackup file is missing.");
                    }

                    if (!File.Exists(SaveManager.GetItemsPosition() + ".mopbackup"))
                    {
                        itemsBackupMissing = true;
                        ModConsole.Print("items.txt.mopbackup file is missing.");
                    }

                    if (defaultBackupMissing && itemsBackupMissing)
                    {
                        ModConsole.Print("Save backups don't exists. Do you use the save optimization?");
                        break;
                    }

                    if (!defaultBackupMissing)
                    {
                        File.Delete(SaveManager.GetDefaultES2SavePosition());
                        File.Move(SaveManager.GetDefaultES2SavePosition() + ".mopbackup", SaveManager.GetDefaultES2SavePosition());
                    }

                    if (!itemsBackupMissing)
                    {
                        File.Delete(SaveManager.GetItemsPosition());
                        File.Move(SaveManager.GetItemsPosition() + ".mopbackup", SaveManager.GetItemsPosition());
                    }

                    ModConsole.Print("Save backup succesfully restored!");
                    break;
                case "generate-list":
                    if (args.Length > 2)
                    {
                        if (RulesManager.Instance.LoadRules && 
                            (RulesManager.Instance.IgnoreRules.Count > 0 || RulesManager.Instance.IgnoreRulesAtPlaces.Count > 0 ||
                             RulesManager.Instance.NewSectors.Count > 0 || RulesManager.Instance.ToggleRules.Count > 0)
                            )
                        {
                            ModConsole.Print("<color=red>WARNING:</color> For accurate results, use \"mop load-rules false\" to prevent MOP from using rule files.");
                        }

                        MopSettings.GenerateToggledItemsListDebug = args[1].ToLower() == "true";
                    }

                    ModConsole.Print($"Generating toggled elements list is set to " +
                                     $"<color={(MopSettings.GenerateToggledItemsListDebug ? "green" : "red")}>{MopSettings.GenerateToggledItemsListDebug}</color>");
                    break;
                case "do-bumper-fix":
                    GameObject trigger = GameObject.Find("trigger_bumper_rear");
                    GameObject bumper = GameObject.Find("bumper rear(Clone)");
                    
                    if (trigger == null)
                    {
                        ModConsole.Error("Trigger not found");
                        return;
                    }

                    if (bumper == null)
                    {
                        ModConsole.Error("Bumper not found");
                        return;
                    }

                    GameFixes.Instance.RearBumperFix(trigger, bumper);
                    break;
                case "load-rules":
                    if (args.Length > 2)
                    {
                        RulesManager.Instance.LoadRules = args[1].ToLower() == "true";
                        if (!RulesManager.Instance.LoadRules)
                        {
                            ModConsole.Print("\n\n<color=red>WARNING:</color>\nDisabling rule files may cause serious issues with game, or even break your game save.\n\n" +
                                             "<b><color=red>!!! USE ONLY AT YOUR OWN RISK !!!</color></b>\n\n");
                        }
                    }

                    ModConsole.Print($"Loading rule files is set to " +
                                     $"<color={(RulesManager.Instance.LoadRules ? "green" : "red")}>{RulesManager.Instance.LoadRules}</color>");
                    break;
            }
        }
    }
}
