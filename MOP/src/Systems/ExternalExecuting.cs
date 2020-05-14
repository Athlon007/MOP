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

        public static void OpenFAQDialog()
        {
            ModUI.ShowYesNoMessage("This will open a new web browser window. Are you sure you want to continue?", OpenFAQ);
        }

        public static void OpenFAQ()
        {
            Process.Start("https://github.com/Athlon007/MOP/blob/master/FAQ.md");
        }

        public static void OpenWikiDialog()
        {
            ModUI.ShowYesNoMessage("This will open a new web browser window. Are you sure you want to continue?", OpenWiki);
        }

        public static void OpenWiki()
        {
            Process.Start("https://github.com/Athlon007/MOP/wiki");
        }

        public static void OpenDonateDialog()
        {
            ModUI.ShowYesNoMessage("This will open a new web browser window. Are you sure you want to continue?", OpenDonate);
        }

        public static void OpenDonate()
        {
            Process.Start("https://paypal.me/figurakonrad");
        }

        public static void OpenRulesWebsiteDialog()
        {
            ModUI.ShowYesNoMessage("This will open a new web browser window. Are you sure you want to continue?", OpenRulesWebsite);
        }

        public static void OpenRulesWebsite()
        {
            Process.Start("http://athlon.kkmr.pl/mop");
        }
    }
}
