﻿// Modern Optimization Plugin
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
using System.Diagnostics;
using System.IO;
using System.Collections;
using Ionic.Zip;
using UnityEngine;

using MOP.FSM;
using MOP.Helpers;

namespace MOP.Common
{
    class BugReporter : MonoBehaviour
    {
        static BugReporter instance;
        public static BugReporter Instance => instance;

        const string ReportFileMessage = "A MOP report archive has been successfully generated. Please follow these steps:\n\n" +
                                        "1.) Go to https://github.com/Athlon007/MOP/issues/new?assignees=&labels=bug&template=template-bug-report.md&title=Bug%20Report \n" +
                                        "2.) Drag & Drop the MOP Bug Report .ZIP file into the report window.\n" +
                                        "3.) Fill in the information about the issue.\n\n" +
                                        "Incorrectly filled bug and/or pirate-game-copy reports WILL BE IGNORED.\n\n" +
                                        "Alternatively, you can send an e-mail to bugreport@kkmr.pl";

        const string FileBugReportMessage = "It's strongly preferred to start the game at least once, before generating the bug report.\n\n" +
                                            "If you can't load the game, press <color=yellow>CONTINUE</color>, to generate the mod report anyway.";



        public BugReporter()
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            StartCoroutine(CheckSaveSlots());
        }

        IEnumerator CheckSaveSlots()
        {
            yield return null;

            GameObject saveSlotsCanvas = GameObject.Find("SaveSlotsCanvas(Clone)");

            if (saveSlotsCanvas != null)
            {
                saveSlotsCanvas.transform.Find("SaveSlotsUI/Saves/SavesList").gameObject.AddComponent<SaveSlotsSupport>();
            }
        }

        public static void FileBugReport()
        {
            if (!MopSettings.LoadedOnce)
            {
#if PRO
                MSCLoader.ModPrompt.CreateContinueAbortPrompt(FileBugReportMessage, "MOP - Bug Report", instance.BugReport);
#else
                MSCLoader.ModUI.ShowContinueAbortMessage(FileBugReportMessage, "MOP - Bug Report", instance.BugReport);
#endif
                return;
            }

            instance.BugReport();
        }

        public void BugReport()
        {
            if (Directory.Exists(Paths.BugReportPath))
            {
                Directory.Delete(Paths.BugReportPath, true);
            }

            Directory.CreateDirectory(Paths.BugReportPath);

            // Now we are getting logs generated today.
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            foreach (string log in Directory.GetFiles(Paths.LogFolder, $"{Paths.DefaultErrorLogName}_*.txt"))
            {
                FileInfo fi = new FileInfo(log);
                if (fi.CreationTime > DateTime.Today.AddDays(-7))
                {
                    string[] pathArray = log.Replace("\\", "/").Split('/');
                    File.Copy(log, Path.Combine(Paths.BugReportPath, pathArray[pathArray.Length - 1]));
                }
            }

            // Generate a MOP report.
            using (StreamWriter sw = new StreamWriter(Path.Combine(Paths.BugReportPath, Paths.DefaultReportLogName + ".txt")))
            {
                sw.WriteLine(ExceptionManager.GetGameInfo());
            }

            // Now we are packing up everything.
            string lastZipFilePath = Path.Combine(Paths.BugReportPath, $"{Paths.BugReportFileName} - {DateTime.Now:yyyy-MM-dd_HH-mm}.zip");
            using (ZipFile zip = new ZipFile())
            {
                // Include all text files from BugReportPath.
                foreach (string file in Directory.GetFiles(Paths.BugReportPath, "*.txt"))
                {
                    zip.AddFile(file, "");
                }

                // Output Log Path 
                zip.AddFile(Paths.OutputLogPath, "");

                // MOP settings file.
                string mopConfigPath = Paths.MopSettingsFile;
                if (File.Exists(mopConfigPath))
                {
                    zip.AddFile(mopConfigPath, "");
                }

                // MopData
                if (File.Exists(MopSettings.DataFile))
                {
                    zip.AddFile(MopSettings.DataFile, "");
                }

                // MOP save-specific file.
                zip.AddDirectoryByName("Save");
                string mopSave = SaveManager.GetMopSavePath();
                if (File.Exists(mopSave))
                {
                    zip.AddFile(mopSave, "Save");
                }

                zip.Save(lastZipFilePath);
            }

            // Now we are deleting all .txt files.
            foreach (string file in Directory.GetFiles(Paths.BugReportPath, "*.txt"))
            {
                File.Delete(file);
            }

            // Create the tutorial.
            using (StreamWriter sw = new StreamWriter($"{Paths.BugReportPath}/README.txt"))
            {
                sw.WriteLine(ReportFileMessage);
            }

            // We are asking the user if he wants to add his game save to the zip file.
            if (File.Exists(SaveManager.SavePath))
            {
#if PRO
                MSCLoader.ModPrompt.CreateYesNoPrompt("Would you like to include your save file?\n\n" +
                                            "This may greatly improve finding and fixing the bug.", "MOP - Bug Report",
                                            onYes: () => { IncludeZip(lastZipFilePath); OpenBugReportFolder(); },
                                            onNo: OpenBugReportFolder);
#else
                MSCLoader.ModUI.ShowYesNoMessage("Would you like to include your save file?\n\n" +
                                            "This may greatly improve finding and fixing the bug.", "MOP - Bug Report",
                                            () => IncludeZip(lastZipFilePath));
                StartCoroutine(WaitForPromptToClose());
#endif
            }
            else
            {
                OpenBugReportFolder();
            }
        }

        void OpenBugReportFolder()
        {
            Process.Start(Paths.BugReportPath);
            Process.Start($"{Paths.BugReportPath}/README.txt");
        }

        void IncludeZip(string zipFilePath)
        {
            using (ZipFile zip = ZipFile.Read(zipFilePath))
            {
                // Create folder called Save in the zip and get defaultES2Save.txt and items.txt.
                // Added try-catch block, because there is no way of checking if folder exists in the zip-file already.
                // If it does, this script will crash.
                try
                {
                    zip.AddDirectoryByName("Save");
                }
                catch { }

                if (File.Exists(SaveManager.SavePath))
                {
                    zip.AddFile(SaveManager.SavePath, "Save");
                }

                if (File.Exists(SaveManager.ItemsPath))
                {
                    zip.AddFile(SaveManager.ItemsPath, "Save");
                }

                if (File.Exists(SaveManager.OptionsPath))
                {
                    zip.AddFile(SaveManager.OptionsPath, "Save");
                }

                zip.Save();
            }
        }

        public void RestartGame()
        {
            StartCoroutine(DelayedRestart());
        }

        IEnumerator DelayedRestart()
        {
            yield return null;

            MSCLoader.ModConsole.Log("[MOP] Initializing the restart procedure...");

            GameObject buttonContinue = GameObject.Find("Interface").transform.Find("Buttons/ButtonContinue").gameObject;
            if (!buttonContinue.activeSelf)
            {
                yield break;
            }

            buttonContinue.SetActive(true);
            PlayMakerFSM fsm = buttonContinue.GetPlayMaker("SetSize");
            var state = fsm.GetState("Action");
            while (!state.ActionsLoaded)
                yield return null;

            var actions = state.Actions;
            actions[2].Active = false;
            actions[2].Enabled = false;
            state.Actions = actions;

            fsm.SendEvent("OVER");
            fsm.SendEvent("DOWN");
        }

#if !PRO
        IEnumerator WaitForPromptToClose()
        {
            GameObject promptCanvas = GameObject.Find("MSCLoader Canvas msgbox");
            GameObject prompt = promptCanvas.transform.GetChild(promptCanvas.transform.childCount - 1)?.gameObject;
            
            if (prompt == null)
            {
                yield break;
            }

            while (prompt != null)
            {
                yield return null;
            }

            OpenBugReportFolder();
        }
#endif
    }
}
