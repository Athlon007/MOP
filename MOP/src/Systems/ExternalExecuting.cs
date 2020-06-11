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
using System.Diagnostics;

namespace MOP
{
    class ExternalExecuting
    {
        // This class manages executing third-party stuff, such as websites.

        static string url;
        const int CharacterLimit = 45;

        static string GetDialog()
        {
            string urlDisplayed = url;
            if (urlDisplayed.Length > CharacterLimit)
                urlDisplayed = urlDisplayed.Substring(0, CharacterLimit - 3) + "...";
            return $"This will open following link:\n<color=yellow>{urlDisplayed}</color>\n\nAre you sure you want to continue?";
        }

        static void OpenWebsite()
        {
            Process.Start(url);
        }

        public static void ShowDialog(string newUrl)
        {
            url = newUrl;
            ModUI.ShowYesNoMessage(GetDialog(), "MOP", OpenWebsite);
        }

        public static void OpenFAQDialog()
        {
            ShowDialog("https://github.com/Athlon007/MOP/blob/master/FAQ.md");
        }

        public static void OpenWikiDialog()
        {
            ShowDialog("https://github.com/Athlon007/MOP/wiki");
        }

        public static void OpenDonateDialog()
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
    }
}
