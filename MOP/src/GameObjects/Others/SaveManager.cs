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

using System.IO;
using UnityEngine;
using MSCLoader;

namespace MOP
{
    class SaveManager
    {
        /// <summary>
        /// For some reason, the save files get marked as read only files, not allowing MSC to save the game.
        /// This script is ran in PreSaveGame() script and removes ReadOnly attribute.
        /// </summary>
        public static void RemoveReadOnlyAttribute()
        {
            if (File.Exists(GetDefaultES2SavePosition()))
                File.SetAttributes(GetDefaultES2SavePosition(), File.GetAttributes(GetDefaultES2SavePosition()) & ~FileAttributes.ReadOnly);

            if (File.Exists(GetItemsPosition()))
                File.SetAttributes(GetItemsPosition(), File.GetAttributes(GetDefaultES2SavePosition()) & ~FileAttributes.ReadOnly);
        }

        public static void RemoveOldSaveFile()
        {
            if (Rules.instance.SpecialRules.ExperimentalSaveOptimization)
            {
                string defaultES2 = GetDefaultES2SavePosition();
                string items = GetItemsPosition();
                if (File.Exists(defaultES2))
                {
                    File.Copy(defaultES2, defaultES2 + ".mopbackup", true);
                    File.Delete(defaultES2);
                }

                if (File.Exists(items))
                {
                    File.Copy(items, items + ".mopbackup", true);
                    File.Delete(items);
                }
            }
        }

        public static string GetDefaultES2SavePosition()
        {
            return Application.persistentDataPath + "/defaultES2File.txt";
        }

        public static string GetItemsPosition()
        {
            return Application.persistentDataPath + "/items.txt";
        }

        /// <summary>
        /// If for some reason the defaultES2File and items are missing, but the backups are there, automatically restore the backups.
        /// </summary>
        public static void RestoreSaveInMainMenu()
        {
            if (ModLoader.GetCurrentScene() != CurrentScene.MainMenu)
                return;

            if (!Rules.instance.SpecialRules.ExperimentalSaveOptimization)
                return;
            
            if (!File.Exists(GetDefaultES2SavePosition()) && File.Exists(GetDefaultES2SavePosition() + ".mopbackup"))
            {
                File.Move(GetDefaultES2SavePosition() + ".mopbackup", GetDefaultES2SavePosition());
                ModConsole.Print("[MOP] Restored defaultES2File.txt");
            }

            if (!File.Exists(GetItemsPosition()) && File.Exists(GetItemsPosition() + ".mopbackup"))
            {
                File.Move(GetItemsPosition() + ".mopbackup", GetItemsPosition());
                ModConsole.Print("[MOP] Restored items.txt");
            }

            // Re-enable the continue button.
            GameObject.Find("Interface").transform.Find("Buttons/ButtonContinue").gameObject.SetActive(true);

            ModConsole.Print("[MOP] Save backup succesfully restored!");
        }
    }
}
