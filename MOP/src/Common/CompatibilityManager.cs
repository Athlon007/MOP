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
using UnityEngine;
using MOP.Items;

namespace MOP.Common
{
    static class CompatibilityManager
    {
        // This script manages the compatibility with other mods
       
        // Use this method of ensuring compatibility ONLY in specific cases,
        // that are not supported by Rule Files.

        // CarryMore
        // https://www.racedepartment.com/downloads/carry-more-backpack-alternative.22396/
        static bool CarryMore { get; set; }
        
        // Advanced Backpack
        static bool AdvancedBackpack { get; set; }
        static readonly Vector3 AdvancedBackpackPosition = new Vector3(630f, 10f, 1140f);
        const int AdvancedBackpackDistance = 30;

        static readonly string[] incompatibleMods = { "KruFPS", "ImproveFPS", "OptimizeMSC", "ZZDisableAll", "DisableAll" };

        public static void Initialize()
        {
            CarryMore = ModLoader.GetMod("CarryMore") != null;
            AdvancedBackpack = ModLoader.GetMod("AdvancedBackpack") != null;
        }

        public static bool IsInBackpack(ItemBehaviour behaviour)
        {
            if (CarryMore)
            {
                return behaviour.transform.position.y < -900;
            }
            else if (AdvancedBackpack)
            {
                return Vector3.Distance(behaviour.transform.position, AdvancedBackpackPosition) < AdvancedBackpackDistance;
            }

            return false;
        }

        public static bool IsConfilctingModPresent(out string conflictingModName)
        {
            foreach (string a in incompatibleMods)
            {
                if (ModLoader.GetMod(a) != null)
                {
                    conflictingModName = ModLoader.GetMod(a).Name;
                    return true;
                }
            }

            conflictingModName = "";
            return false;
        }

        public static bool IsMySummerCar => Application.productName == "My Summer Car";

#if PRO
        // Compatibility layer between MSCLoader's and Mod Loader Pro settings.
        public static int GetValue(this SettingSlider slider)
        {
            return (int)slider.Value;
        }

        public static bool GetValue(this SettingToggle toggle)
        {
            return toggle.Value;
        }


        public static void AddTooltip(this ModSetting setting, string text)
        {
            setting.gameObject.AddComponent<UITooltip>().toolTipText = text;
        }

        public static void SetValue(this SettingText settingText, string text)
        {
            settingText.Enabled = !string.IsNullOrEmpty(text);
            settingText.Text = text;
        }
#endif

        public static bool IsMSCLoader()
        {
            return GameObject.Find("MSCLoader Canvas menu") != null;
        }

        public static bool IsModLoaderPro()
        {
            return GameObject.Find("MSCLoader Canvas menu") == null;
        }
    }
}
