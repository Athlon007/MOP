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
using System.Diagnostics;
using UnityEngine;

namespace MOP.Common
{
    class ExternalExecuting
    {
        // This class manages executing third-party stuff, such as websites.

        static string url;
        const int CharacterLimit = 45;

        static string GetDialogMessage()
        {
            string urlDisplayed = url;
#if !PRO
            if (urlDisplayed.Length > CharacterLimit)
                urlDisplayed = urlDisplayed.Substring(0, CharacterLimit - 3) + "...";
#endif
            return $"This will open the following link:\n<color=yellow>{urlDisplayed}</color>\n\nAre you sure you want to continue?";
        }

        static void OpenWebsite()
        {
            Process.Start(url);
        }

        public static void ShowDialog(string newUrl)
        {
            url = newUrl;
            ModUI.ShowYesNoMessage(GetDialogMessage(), "MOP", OpenWebsite);
        }

        public static void OpenFAQDialog()
        {
            ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/faq");
        }

        public static void OpenWikiDialog()
        {
            ShowDialog("http://athlon.kkmr.pl/mop/wiki/#/");
        }

        public static void OpenPaypalDialog()
        {
            ShowDialog("https://paypal.me/figurakonrad");
        }

        public static void OpenRulesWebsiteDialog()
        {
            ShowDialog("http://athlon.kkmr.pl/mop");
        }

        public static void OpenHomepageDialog()
        {
            ShowDialog("http://athlon.kkmr.pl/");
        }

        public static void OpenPatreonDialog()
        {
            ShowDialog("http://patreon.com/athlon");
        }

        public static void ModLoaderPro()
        {
            Process.Start("https://mscloaderpro.github.io/docs/#/Download");
        }

        public static void DownloadMOPPro()
        {
            Process.Start("https://github.com/Athlon007/MOP/releases/latest");
        }

        public static void OpenNexus()
        {
            Process.Start("https://www.nexusmods.com/mysummercar/mods/146");
        }
    }
}
