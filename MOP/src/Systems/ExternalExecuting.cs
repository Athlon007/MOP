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
using System;
using System.Diagnostics;

namespace MOP
{
    class ExternalExecuting
    {
        // This class manages executing third-party stuff, such as websites.

        static string url;

        static string GetDialog()
        {
            string urlDisplayed = url;
            if (urlDisplayed.Length > 45)
                urlDisplayed = urlDisplayed.Substring(0, 42) + "...";
            return $"This will open this link:\n<color=yellow>{urlDisplayed}</color>\n\nAre you sure you want to continue?";
        }

        static void OpenWebsite()
        {
            Process.Start(url);
        }

        public static void OpenFAQDialog()
        {
            url = "https://github.com/Athlon007/MOP/blob/master/FAQ.md";
            ModUI.ShowYesNoMessage(GetDialog(), "MOP", OpenWebsite);
        }

        public static void OpenWikiDialog()
        {
            url = "https://github.com/Athlon007/MOP/wiki";
            ModUI.ShowYesNoMessage(GetDialog(), "MOP", OpenWebsite);
        }

        public static void OpenDonateDialog()
        {
            url = "https://paypal.me/figurakonrad";
            ModUI.ShowYesNoMessage(GetDialog(), "MOP", OpenWebsite);
        }

        public static void OpenRulesWebsiteDialog()
        {
            url = "http://athlon.kkmr.pl/mop";
            ModUI.ShowYesNoMessage(GetDialog(), "MOP", OpenWebsite);
        }

        public static void OpenHomepageDialog()
        {
            url = "http://athlon.kkmr.pl/";
            ModUI.ShowYesNoMessage(GetDialog(), "MOP", OpenWebsite);
        }
    }
}
