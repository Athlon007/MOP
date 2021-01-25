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
        bool doAddZip;
        bool waitEnd;

        string bugReportPath => $"{ExceptionManager.GetRootPath()}/MOP_bugreport";

        public BugReporter()
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        void Update()
        {
            if (!startWaiting) return;

            if (waitEnd)
            {
                startWaiting = false;
                waitEnd = false;
                ShowWindow();
            }
        }

        public static void FileBugReport()
        {
            instance.BugReport();
        }

        public void BugReport()
        {
            if (Directory.Exists(bugReportPath))
            {
                Directory.Delete(bugReportPath, true);
            }

            Directory.CreateDirectory(bugReportPath);

            // Get output_log.txt
            if (File.Exists($"{ExceptionManager.GetRootPath()}/output_log.txt"))
            {
                File.Copy($"{ExceptionManager.GetRootPath()}/output_log.txt", $"{bugReportPath}/output_log.txt");
            }

            // Now we are getting logs generated today.
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            foreach (string log in Directory.GetFiles(ExceptionManager.GetLogFolder(), $"*{today}*.txt"))
            {
                string pathToFile = log.Replace("\\", "/");
                string nameOfFile = log.Split('\\')[1];
                ModConsole.Print(nameOfFile);
                File.Copy(pathToFile, $"{bugReportPath}/{nameOfFile}");
            }

            using (StreamWriter sw = new StreamWriter($"{bugReportPath}/MOP_REPORT.txt"))
            {
                sw.WriteLine(ExceptionManager.GetGameInfo());
            }

            // Now we are packing up everything.
            using (ZipFile zip = new ZipFile())
            {
                foreach (string file in Directory.GetFiles(bugReportPath, "*.txt"))
                {
                    zip.AddFile(file, "");
                }

                lastZipFilePath = $"{bugReportPath}/MOP Bug Report - {DateTime.Now:yyyy-MM-dd_HH-mm}.zip";
                zip.Save(lastZipFilePath);
            }

            // Now we are deleting all .txt files.
            foreach (string file in Directory.GetFiles(bugReportPath, "*.txt"))
            {
                File.Delete(file);
            }

            // Create the tutorial.
            using (StreamWriter sw = new StreamWriter($"{bugReportPath}/README.txt"))
            {
                sw.WriteLine("A MOP report archive has been successfully generated.\n");
                sw.WriteLine("Upload .zip file to some file hosting site, such as https://www.mediafire.com/.");
            }

            // We are askign the user if he wants to add a zip file.
            startWaiting = true;
            if (File.Exists(SaveManager.GetDefaultES2SavePosition()))
            {
                ModUI.ShowYesNoMessage("Would you like to include save file? This may greatly improve fixing the bug.", "MOP", DoAddZip);
            }
            else
            {
                waitEnd = true;
            }
        }

        void DoAddZip()
        {
            try
            {
                using (ZipFile zip = ZipFile.Read(lastZipFilePath))
                {
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
            catch (Exception ex)
            {
                ModConsole.Error(ex.ToString());
            }

            waitEnd = true;
        }

        void ShowWindow()
        {
            Process.Start(bugReportPath);
            Process.Start($"{bugReportPath}/README.txt");
        }
    }
}
