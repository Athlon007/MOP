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
using Ionic.Zip;
using UnityEngine;
using MSCLoader;

using MOP.Helpers;

namespace MOP.Common
{
    class BugReporter : MonoBehaviour
    {
        static BugReporter instance;
        public static BugReporter Instance => instance;

        string lastZipFilePath;
        
        bool startWaiting;
        GameObject mscloaderMB;

        string BugReportPath => $"{ExceptionManager.GetRootPath()}/MOP_bugreport";

        public BugReporter()
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        void Update()
        {
            if (!startWaiting) return;

            if (!mscloaderMB)
            {
                startWaiting = false;
                ShowWindow();
            }
        }

        public static void FileBugReport()
        {
            if (!MopSettings.LoadedOnce)
            {
                ModUI.ShowMessage("Please load the game fully at least once, before reporting a bug!");
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
            if (File.Exists($"{ExceptionManager.GetRootPath()}/output_log.txt"))
            {
                File.Copy($"{ExceptionManager.GetRootPath()}/output_log.txt", $"{BugReportPath}/output_log.txt");
            }

            // Now we are getting logs generated today.
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            foreach (string log in Directory.GetFiles(ExceptionManager.GetLogFolder(), $"*{today}*.txt"))
            {
                string pathToFile = log.Replace("\\", "/");
                string nameOfFile = log.Split('\\')[1];
                ModConsole.Print(nameOfFile);
                File.Copy(pathToFile, $"{BugReportPath}/{nameOfFile}");
            }

            // Generate a MOP report.
            using (StreamWriter sw = new StreamWriter($"{BugReportPath}/MOP_REPORT.txt"))
            {
                sw.WriteLine(ExceptionManager.GetGameInfo());
            }

            // Now we are packing up everything.
            using (ZipFile zip = new ZipFile())
            {
                foreach (string file in Directory.GetFiles(BugReportPath, "*.txt"))
                {
                    zip.AddFile(file, "");
                }

                lastZipFilePath = $"{BugReportPath}/MOP Bug Report - {DateTime.Now:yyyy-MM-dd_HH-mm}.zip";
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
                             "Remember to describe how you stumbled uppon the error!");
            }

            // We are asking the user if he wants to add his game save to the zip file.
            startWaiting = true;
            if (File.Exists(SaveManager.GetDefaultES2SavePosition()))
            {
                ModUI.ShowYesNoMessage("Would you like to your include save file?\n\nThis may greatly improve finding and fixing the bug.", "MOP", DoAddZip);

                // MOP will wait for this transform to be destroyed.
                Transform messageBoxTransform = GameObject.Find("MSCLoader Canvas").transform.Find("MSCLoader MB(Clone)");
                if (messageBoxTransform)
                {
                    mscloaderMB = messageBoxTransform.gameObject;
                }
            }
        }

        void DoAddZip()
        {
            using (ZipFile zip = ZipFile.Read(lastZipFilePath))
            {
                // Create folder called Save in the zip and get defaultES2Save.txt and items.txt.
                zip.AddDirectoryByName("Save");
                if (File.Exists(SaveManager.GetDefaultES2SavePosition()))
                {
                    zip.AddFile(SaveManager.GetDefaultES2SavePosition(), "Save");
                }

                if (File.Exists(SaveManager.GetItemsPosition()))
                {
                    zip.AddFile(SaveManager.GetItemsPosition(), "Save");
                }

                zip.Save();
            }
        }

        void ShowWindow()
        {
            Process.Start(BugReportPath);
            Process.Start($"{BugReportPath}/README.txt");
        }
    }
}
