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

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using MSCLoader;

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
                                "<color=yellow>reload [all]</color> - Forces MOP to reload rule files. If \"all\" modifier is added, MOP will load all rule files\n" +
                                "<color=yellow>new [ModID]</color> - Create custom rule file (if no ModID is provided, will create Custom.txt)\n" +
                                "<color=yellow>open [ModID]</color> - Opens the .modconfig for mod\n" +
                                "<color=yellow>open-config</color> - Opens MOP config folder\n" +
                                "<color=yellow>delete [ModID]</color> - Delete rule file\n" +
                                "<color=yellow>cat [File Name]</color> - Print the content of a rule file\n" +
                                "<color=yellow>generate-list [true/false]</color> - Generates text files which contain the list of items that are toggled by MOP\n" +
                                "<color=yellow>load-rules [true/false]</color> - If set to false, prevents MOP from loading any rule files.\n" +
                                "<color=yellow>force-crash [critical]</color> - Forces MOP to crash itself. If 'critical' is added, it will force a critical crash.\n" +
                                "<color=yellow>resolution</color> - Lists the available resolutions\n" +
                                "<color=yellow>resolution [nOfResolution]</color> - Sets the desired resolution from 'mop resolution' list\n" +
                                "<color=yellow>resolution [width] [height]</color> - Sets the desired resolution\n" +
                                "<color=yellow>force-load-restart [true/false]</color> - Forces the 'missing Satsuma parts' game reload to happen\n" +
                                "<color=yellow>stop</color> - Stops MOP. MIGHT BREAK THE GAME!\n" +
                                "<color=yellow>start</color> - Starts MOP back again\n" +
                                "<color=yellow>quality [0-5]</color> - Set quality setting";

        public override void Run(string[] args)
        {
            if (args.Length == 0)
            {
                ModConsole.Log("See \"mop help\" for command list.");
                return;
            }

            switch (args[0].ToUpperInvariant())
            {
                default:
                    ModConsole.Log("Invalid command. Type \"mop help\" for command list.");
                    break;
                case "HELP":
                    string[] helpList = HelpList.Split('\n');
                    Array.Sort(helpList, StringComparer.InvariantCulture);
                    if (args.Length > 1)
                    {
                        bool commandsFound = false;
                        foreach (string s in helpList)
                        {
                            if (s.Split('-')[0].Contains(args[1]))
                            {
                                ModConsole.Log(s);
                                commandsFound = true;
                            }
                        }
                        if (!commandsFound)
                        {
                            ModConsole.Log($"Command {args[1]} not found.");
                        }
                        return;
                    }
                    ModConsole.Log(string.Join("\n", helpList));
                    break;
                case "RULES":
                    if (args.Length > 1 && args[1] == "roll")
                    {
                        ModConsole.Log("\n<color=yellow>You know the rules and so do I\n" +
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
                        ModConsole.Log("<color=yellow><b>Ignore Rules</b></color>");
                        foreach (IgnoreRule r in RulesManager.Instance.IgnoreRules)
                            ModConsole.Log($"<b>Object:</b> {r.ObjectName}" + (r.TotalIgnore ? " <i>(Full Ignore)</i>" : "") + $" <b>By</b> {r.Mod.ID}");
                    }

                    if (RulesManager.Instance.IgnoreRulesAtPlaces.Count > 0)
                    {
                        ModConsole.Log("\n<color=yellow><b>Ignore Rules At Place</b></color>");
                        foreach (IgnoreRuleAtPlace r in RulesManager.Instance.IgnoreRulesAtPlaces)
                            ModConsole.Log($"<b>Place:</b> {r.Place} <b>Object:</b> {r.ObjectName}" + $" <b>By</b> {r.Mod.ID}");
                    }

                    if (RulesManager.Instance.ToggleRules.Count > 0)
                    {
                        ModConsole.Log("\n<color=yellow><b>Toggle Rules</b></color>");
                        foreach (ToggleRule r in RulesManager.Instance.ToggleRules)
                            ModConsole.Log($"<b>Object:</b> {r.ObjectName} <b>Toggle Mode:</b> {r.ToggleMode}" + $" <b>By</b> {r.Mod.ID}");
                    }

                    if (RulesManager.Instance.NewSectors.Count > 0)
                    {
                        ModConsole.Log("\n<color=yellow><b>New Sectors</b></color>");
                        foreach (NewSector r in RulesManager.Instance.NewSectors)
                            ModConsole.Log($"<b>Pos:</b> {r.Position} <b>Scale:</b> {r.Scale} <b>Rot:</b> {r.Rotation} " +
                                $"<b>Ignore:</b> {string.Join(", ", r.Whitelist)}" + $" <b>By</b> {r.Mod.ID}");
                    }

                    if (RulesManager.Instance.ChangeParentRules.Count > 0)
                    {
                        ModConsole.Log("\n<color=yellow><b>Change Parent</b></color>");
                        foreach (ChangeParentRule r in RulesManager.Instance.ChangeParentRules)
                            ModConsole.Log($"<b>Object:</b> {r.ObjectName} <b>New Parent:</b> {r.NewParentName}" + $" <b>By</b> {r.Mod.ID}");
                    }

                    ModConsole.Log("\n<color=yellow><b>Special Rules</b></color>");
                    // Obtain all fields
                    FieldInfo[] fields = typeof(SpecialRules).GetFields();
                    // Loop through fields
                    foreach (var field in fields)
                    {
                        ModConsole.Log($"<b>{field.Name}</b>: {field.GetValue(RulesManager.Instance.SpecialRules)}");
                    }

                    // List rule files.
                    ModConsole.Log("\n<color=yellow><b>Rule Files</b></color>");
                    List<string> files = new List<string>();
                    foreach (Rule ruleFile in RulesManager.Instance.Rules)
                    {
                        if (!files.Contains(ruleFile.ToString()))
                        {
                            files.Add(ruleFile.ToString());
                        }
                    }
                    ModConsole.Log(string.Join("\n", files.ToArray()));
                    break;
                case "RELOAD":
                    if (ModLoader.CurrentScene != CurrentScene.MainMenu)
                    {
                        ModConsole.Log("You can only reload the rule files in the main menu.");
                        return;
                    }

                    bool loadAll = false;
                    if (args.Length > 1)
                        loadAll = args[1].ToUpperInvariant() == "ALL";

                    RulesManager.Instance.WipeAll(false, loadAll);
                    break;
                case "NEW":
                    string path = $"{MOP.ModConfigPath}/Custom.txt";

                    if (args.Length > 1)
                    {
                        path = $"{MOP.ModConfigPath}/{args[1]}.mopconfig";
                    }

                    if (File.Exists(path))
                    {
                        ModConsole.Log("Custom file already exists. Use \"mop open\" to edit it now.");
                        return;
                    }

                    File.WriteAllText(path, "## Every line which starts with '##' will be ignored.\n" +
                                            "## All new flags MUST be written in a new line.\n" +
                                            "## Visit http://athlon.kkmr.pl/mop/wiki/#/rulefiles_commands for documentation.\n" +
                                            "## WARNING: Using custom rule files may cause issues. Use only at your own risk!");

                    Process.Start(path);
                    if (path.EndsWith("Custom.txt"))
                    {
                        ModConsole.Log("A custom rule file has been created. You can find it as Custom.txt.\n" +
                            "<color=red>Careless use of rule files may cause bugs and glitchess. Use only at yout own risk!</color>");
                    }
                    else
                    {
                        ModConsole.Log($"A rule file for {args[1]} mod has been created.");
                    }
                    break;
                case "VERSION":
                    ModConsole.Log($"{MOP.ModVersion} for {MOP.Edition}");
                    break;
                case "COWSAY":
                    string say = string.Join(" ", args, 1, args.Length - 1);

                    ModConsole.Log($"< {say} >\n" +
                                    "        \\   ^__^\n" +
                                    "         \\  (oo)\\____\n" +
                                    "            (__)\\          )\\/\\\n" +
                                    "                ||  ----w  |\n" +
                                    "                ||           || ");
                    break;
                case "OPEN-CONFIG":
                    Process.Start(MOP.ModConfigPath);
                    break;
                case "OPEN":
                    if (args.Length == 1)
                    {
                        ModConsole.Log($"Missing argument.");
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
                        ModConsole.Log($"File {args[1]} doesn't exist.");
                        return;
                    }

                    Process.Start($"{MOP.ModConfigPath}/{args[1]}");
                    break;
                case "DELETE":
                    if (args.Length == 1)
                    {
                        ModConsole.Log($"Missing argument.");
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
                        ModConsole.Log($"File {args[1]} doesn't exist.");
                        return;
                    }

                    File.Delete($"{MOP.ModConfigPath}/{args[1]}");
                    break;
                case "CAT":
                    if (args.Length == 1)
                    {
                        ModConsole.Log($"Missing argument.");
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
                        ModConsole.Log($"File {args[1]} doesn't exist.");
                        return;
                    }

                    ModConsole.Log(File.ReadAllText($"{MOP.ModConfigPath}/{args[1]}"));
                    break;
                case "GENERATE-LIST":
                    if (args.Length > 1)
                    {
                        if (RulesManager.Instance.LoadRules &&
                            (RulesManager.Instance.IgnoreRules.Count > 0 || RulesManager.Instance.IgnoreRulesAtPlaces.Count > 0 ||
                             RulesManager.Instance.NewSectors.Count > 0 || RulesManager.Instance.ToggleRules.Count > 0))
                        {
                            ModConsole.Log("<color=red>WARNING:</color> For accurate results, use \"mop load-rules false\" to prevent MOP from using rule files.");
                        }

                        MopSettings.GenerateToggledItemsListDebug = args[1].ToUpperInvariant() == "TRUE";
                    }

                    ModConsole.Log($"Generating toggled elements list is set to " +
                                   $"<color={(MopSettings.GenerateToggledItemsListDebug ? "green" : "red")}>{MopSettings.GenerateToggledItemsListDebug}</color>");
                    break;
                case "LOAD-RULES":
                    if (args.Length > 1)
                    {
                        RulesManager.Instance.LoadRules = args[1].ToUpperInvariant() == "TRUE";
                        if (!RulesManager.Instance.LoadRules)
                        {
                            ModConsole.Log("\n\n<color=red>WARNING:</color>\nDisabling rule files may cause serious issues with the game, or even break your save file.\n\n" +
                                             "<b><color=red>!!! USE ONLY AT YOUR OWN RISK !!!</color></b>\n\n");
                        }
                    }

                    ModConsole.Log($"Loading rule files is set to " +
                                     $"<color={(RulesManager.Instance.LoadRules ? "green" : "red")}>{RulesManager.Instance.LoadRules}</color>");
                    break;
                case "FORCE-CRASH":
                    bool isCritical = false;
                    if (args.Length > 1 && args[1].ToUpperInvariant() == "CRITICAL")
                    {
                        isCritical = true;
                    }

                    try
                    {
                        throw new Exception("Test Exception");
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, isCritical, "TEST_EXCEPTION");
                    }
                    break;
                case "RESOLUTION":
                    try
                    {
                        if (args.Length == 1)
                        {
                            string availableResolutions = "";
                            for (int i = 0; i < Screen.resolutions.Length; ++i)
                            {
                                Resolution res = Screen.resolutions[i];
                                if (res.width == Screen.width && res.height == Screen.height)
                                {
                                    availableResolutions += $"  <color=green>{i + 1}. {res.width}x{res.height}</color>\n";
                                }
                                else
                                {
                                    availableResolutions += $"  {i + 1}. {res.width}x{res.height}\n";
                                }
                            }
                            ModConsole.Log(availableResolutions);
                            ModConsole.Log("Use '<color=yellow>mop resolution [nOfResolution]</color>' to set the preferred resolution.");
                            return;
                        }

                        if (args.Length == 2)
                        {
                            int nOfResolution = int.Parse(args[1]) - 1;
                            Resolution res = Screen.resolutions[nOfResolution];
                            Screen.SetResolution(res.width, res.height, false);
                            return;
                        }

                        int width = int.Parse(args[1]);
                        int height = int.Parse(args[2]);

                        Screen.SetResolution(width, height, false);
                    }
                    catch
                    {
                        ModConsole.LogError("Failed setting resolution.");
                    }
                    break;
                case "QUALITY":
                    try
                    {
                        QualitySettings.SetQualityLevel(int.Parse(args[1]), true);
                    }
                    catch
                    {
                        ModConsole.LogError("Failed setting quality settings.");
                    }
                    break;
                case "FORCE-LOAD-RESTART":
                    if (args.Length == 2)
                    {
                        string var = args[1].ToLower();
                        if (var == "true")
                        {
                            MopSettings.ForceLoadRestart = true;
                        }
                        else if (var == "false")
                        {
                            MopSettings.ForceLoadRestart = false;
                        }
                        else
                        {
                            ModConsole.LogError("Unrecognized argument: " + var);
                            return;
                        }
                    }

                    ModConsole.Log($"force-load-restart is set to <color=yellow>{MopSettings.ForceLoadRestart}</color>.");
                    break;
                case "STOP":
                    if (ModLoader.CurrentScene != CurrentScene.Game)
                    {
                        ModConsole.Log("[MOP] MOP can only be stopped while in-game.");
                        return;
                    }

                    if (MopSettings.IsModActive == false)
                    {
                        ModConsole.Log("[MOP] MOP is not running.");
                        return;
                    }

                    Hypervisor.Instance.StopAllCoroutines();
                    Hypervisor.Instance.ToggleAll(true);
                    MopSettings.IsModActive = false; 
                    ModConsole.Log("[MOP] MOP has been stopped. Some things may be broken. Saving game in this state may break your game!");
                    break;
                case "START":
                    if (ModLoader.CurrentScene != CurrentScene.Game)
                    {
                        ModConsole.Log("[MOP] MOP can only be started while in-game.");
                        return;
                    }

                    if (MopSettings.IsModActive)
                    {
                        ModConsole.Log("[MOP] MOP is already running.");
                        return;
                    }

                    Hypervisor.Instance.ToggleAll(false, Enumerations.ToggleAllMode.OnLoad);
                    Hypervisor.Instance.Startup();
                    break;
                case "DEBUG_MONITOR":
                    if (ModLoader.CurrentScene != CurrentScene.Game)
                    {
                        ModConsole.Log("[MOP] Debug monitor can only be used in-game.");
                        return;
                    }

                    Hypervisor.Instance.ToggleDebugMode();

                    break;
            }
        }
    }
}
