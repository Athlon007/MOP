using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MSCLoader;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;

namespace MOP.Misc
{
    class Update
    {
#if DEBUG
        //const string ReleaseInfo = "file://C:/Users/aathl/source/repos/YAOS/releaseinfo.txt";
        //const string UpdateDownloadUrl = "file://C:/Users/aathl/source/repos/YAOS/update.zip";
        const string ReleaseInfo = "https://raw.githubusercontent.com/Athlon007/MOP/development/releaseinfo.txt";
        const string UpdateDownloadUrl = "https://raw.githubusercontent.com/Athlon007/MOP/development/update.zip";
#else
        const string ReleaseInfo = "https://raw.githubusercontent.com/Athlon007/MOP/releaseinfo.txt";
        const string UpdateDownloadUrl = "https://raw.githubusercontent.com/Athlon007/MOP/update.zip";
#endif

        static bool IsUpdateAvailable { get; set; }

        const string updaterScript =
            "@echo off\n" +
            "echo === MOP Update Manager ===\n" + 
            "echo Extracting now...\n" +
            "tar -xf \"%cd%\\mop.zip\" -C \"%cd%\\mopupdate\"\n" + 
            "echo Installing the update...\n" +
            "TASKKILL /IM \"mysummercar.exe\"\n" + 
            "xcopy /s /y \"%cd%\\mopupdate\" \"%cd%\"\n" +
            "echo Updating complete! Starting My Summer Car...\n" +
            "start \"\" \"steam://rungameid/516750\"\n" +
            "exit";


        public static void CheckForUpdate()
        {
#if RELEASE
            if (!ModLoader.CheckSteam()) 
            {
                ModConsole.Error("[MOP] Sorry Boss! MOP auto update feature is exclusive to non pirate game owners!");
            }
#endif

            if (IsUpdateAvailable)
            {
                ModUI.ShowYesNoMessage("There's a new update ready to download. Would you like to download it now?", "MOP Update", DownloadUpdate);
                return;
            }

            ModConsole.Print("[MOP] Checking for update now...");

            try
            {
                using (WebClient client = new WebClient())
                {
                    string latestString = client.DownloadString(ReleaseInfo);
                    List<int> latest = new List<int>();
                    foreach (string str in latestString.Split('.'))
                        latest.Add(int.Parse(str));

                    Version version = Assembly.GetExecutingAssembly().GetName().Version;
                    string currentString = version.Major + "." + version.Minor + "." + version.Build;
                    List<int> current = new List<int>();
                    current.Add(version.Major);
                    current.Add(version.Minor);
                    current.Add(version.Build);

                    IsUpdateAvailable = false;

                    for (int i = 0; i < latest.Count; i++)
                    {
                        if (latest[i] > current[i])
                        {
                            IsUpdateAvailable = true;
                            break;
                        }
                    }

                    if (IsUpdateAvailable)
                    {
                        ModUI.ShowYesNoMessage("There's a new update ready to download. Would you like to download it now?\n\n" +
                            $"Your version: {currentString}\nNewest version: {latestString}", "MOP Update", DownloadUpdate);

                        ModConsole.Print("[MOP] New update found!");
                    }
                    else
                    {
                        ModConsole.Print("[MOP] No new updates found");
                    }
                }
            }
            catch (Exception ex)
            {
                ModConsole.Error("[MOP] Couldn't download the latest version info.\n\n" + ex.ToString());
                return;
            }
        }

        static void DownloadUpdate()
        {
            ModConsole.Print("[MOP] Downloading an update...");

            try
            {
                using (WebClient client = new WebClient())
                {
                    string modFolderPath = MOP.ModsFolderPath;
                    client.DownloadFile(new Uri(UpdateDownloadUrl), modFolderPath + "mop.zip");

                    Directory.CreateDirectory(modFolderPath + "mopupdate");
                    File.WriteAllText(modFolderPath + "updater.bat", updaterScript);

                    Process process = new Process();
                    process.StartInfo.WorkingDirectory = modFolderPath;
                    //process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = modFolderPath + "updater.bat";
                    process.Start();
                    Application.Quit();
                }
            }
            catch (Exception ex)
            {
                ModConsole.Print("[MOP] Couldn't download the newest update\n\n" + ex.ToString());
            }
        }

        public static bool IsAutoUpdateCompatible()
        {
            string productName = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "").ToString();
            int releaseId = int.Parse(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString());

            if (!productName.Contains("Windows 10"))
            {
                return false;
            }

            return releaseId > 1803;
        }
    }
}
