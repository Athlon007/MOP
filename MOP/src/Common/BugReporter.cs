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

using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using Ionic.Zip;
using UnityEngine;
using MSCLoader;

using MOP.Helpers;
using MSCLoader.Helper;

namespace MOP.Common
{
    class BugReporter : MonoBehaviour
    {
        static BugReporter instance;
        public static BugReporter Instance => instance;

        string BugReportPath => $"{ExceptionManager.RootPath}/MOP_bugreport";

        public BugReporter()
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            StartCoroutine(CheckSaveSlots());
        }

        IEnumerator CheckSaveSlots()
        {
            yield return null;

            if (GameObject.Find("SaveSlotsCanvas(Clone)") != null)
            {
                GameObject.Find("SaveSlotsCanvas(Clone)").transform.Find("SaveSlotsUI/Saves/SavesList").gameObject.AddComponent<SaveSlotsSupport>();
            }
        }

        public static void FileBugReport()
        {
            if (!MopSettings.LoadedOnce)
            {
                ModPrompt.CreateContinueAbortPrompt("It is recommended to start the game at least once before filing bug report.\n\n" +
                                                    "If you can't load the game, press Continue to generate mod report anyway.", 
                                                    "MOP - Bug Report", 
                                                    () => instance.BugReport());
                return;
            }

            instance.BugReport();
        }

        public void BugReport()
        {
            if (Directory.Exists(BugReportPath))
            {
                Directory.Delete(BugReportPath, true);
            }

            Directory.CreateDirectory(BugReportPath);

            // Get output_log.txt
            if (File.Exists($"{ExceptionManager.RootPath}/output_log.txt"))
            {
                File.Copy($"{ExceptionManager.RootPath}/output_log.txt", $"{BugReportPath}/output_log.txt");
            }

            // Now we are getting logs generated today.
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            foreach (string log in Directory.GetFiles(ExceptionManager.LogFolder, $"*{today}*.txt"))
            {
                string pathToFile = log.Replace("\\", "/");
                string nameOfFile = log.Split('\\')[1];
                File.Copy(pathToFile, $"{BugReportPath}/{nameOfFile}");
            }

            // Generate a MOP report.
            using (StreamWriter sw = new StreamWriter($"{BugReportPath}/MOP_REPORT.txt"))
            {
                sw.WriteLine(ExceptionManager.GetGameInfo());
            }

            // Now we are packing up everything.
            string lastZipFilePath = $"{BugReportPath}/MOP Bug Report - {DateTime.Now:yyyy-MM-dd_HH-mm}.zip";
            using (ZipFile zip = new ZipFile())
            {
                foreach (string file in Directory.GetFiles(BugReportPath, "*.txt"))
                {
                    zip.AddFile(file, "");
                }

                zip.Save(lastZipFilePath);
            }

            // Now we are deleting all .txt files.
            foreach (string file in Directory.GetFiles(BugReportPath, "*.txt"))
            {
                File.Delete(file);
            }

            // Create the tutorial.
            using (StreamWriter sw = new StreamWriter($"{BugReportPath}/README.txt"))
            {
                sw.WriteLine("A MOP report archive has been successfully generated.\n");
                sw.WriteLine("Upload .zip file to some file hosting site, such as https://www.mediafire.com/. \n\n" +
                             "Remember to describe how you came acorss the error!");
            }

            // We are asking the user if he wants to add his game save to the zip file.
            if (File.Exists(SaveManager.SavePath))
            {
                ModPrompt.CreateYesNoPrompt("Would you like to your include save file?\n\n" +
                                            "This may greatly improve finding and fixing the bug.", "MOP - Bug Report",
                                            () => IncludeZip(lastZipFilePath),
                                            onPromptClose: () => { Process.Start(BugReportPath); Process.Start($"{BugReportPath}/README.txt"); });
            }
        }

        void IncludeZip(string zipFilePath)
        {
            using (ZipFile zip = ZipFile.Read(zipFilePath))
            {
                // Create folder called Save in the zip and get defaultES2Save.txt and items.txt.
                zip.AddDirectoryByName("Save");
                if (File.Exists(SaveManager.SavePath))
                {
                    zip.AddFile(SaveManager.SavePath, "Save");
                }

                if (File.Exists(SaveManager.ItemsPath))
                {
                    zip.AddFile(SaveManager.ItemsPath, "Save");
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

            GameObject buttonContinue = GameObject.Find("Interface").transform.Find("Buttons/ButtonContinue").gameObject;
            if (!buttonContinue.activeSelf)
            {
                yield break;
            }

            buttonContinue.SetActive(true);
            PlayMakerFSM fsm = buttonContinue.GetPlayMakerFSM("SetSize");
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
    }
}
