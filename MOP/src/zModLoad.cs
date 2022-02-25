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

using MSCLoader;
using MOP.Common;

namespace MOP
{
#if !PRO
    public class zModLoad : Mod
    {
        // This class' purpose is to warn Mod Loader Pro users that they're trying to use MOP verison for MSCLoader on Mod Loader Pro.
        public override string ID => "MSCLoader_MOP Loader"; // We're prettinding that this mod is MSCLoader's component - otherwise MOP won't load.
        public override string Name => "You are using wrong version of MOP!";
        public override string Author => "Athlon"; //Your Username
        public override string Version => "1.0"; //Version
        public override string Description => "If you're seeing this message, that means you installed an incorrect version of MOP!";
        public override byte[] Icon => Properties.Resources.icon;

        public override void MenuOnLoad()
        {
            if (CompatibilityManager.IsModLoaderPro())
            {
                ModUI.ShowMessage("You are trying to use MOP version for <color=yellow>MSCLoader</color>.\n\nPlease install MOP version for <color=yellow>Mod Loader Pro</color>!", "MOP - Error");
            }
        }
    }
#endif
}
