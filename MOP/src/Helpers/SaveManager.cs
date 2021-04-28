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

using System.IO;
using UnityEngine;
using MSCLoader;
using System.Linq;

using MOP.Rules;
using HutongGames.PlayMaker;
using System;

namespace MOP.Helpers
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

        public static string GetDefaultES2SavePosition()
        {
            return Path.Combine(Application.persistentDataPath, "defaultES2File.txt").Replace('\\', '/');
        }

        public static string GetItemsPosition()
        {
            return Application.persistentDataPath + "/items.txt";
        }

        public static void VerifySave()
        {
            if (!File.Exists(GetDefaultES2SavePosition())) return;

            // Passenger bucket seat.
            // Check if driver bucket seat is bought and check the same for passenger one.
            // If they do not match, fix it.
            try
            {
                ES2Settings setting = new ES2Settings();
                bool bucketPassengerSeat = ES2.Load<bool>(GetDefaultES2SavePosition() + "?tag=bucket seat passenger(Clone)Purchased", setting);
                bool bucketDriverSeat = ES2.Load<bool>(GetDefaultES2SavePosition() + "?tag=bucket seat driver(Clone)Purchased", setting);
                if (bucketDriverSeat != bucketPassengerSeat)
                {
                    ModPrompt.CreateYesNoPrompt($"MOP found an issue with your save file. Following items are affected:\n\n" +
                        $"<color=yellow>bucket seats</color>\n\nWould you like it to fix it?", "MOP - Save Integrity Verification", FixBucketSeats);
                }
            }
            catch (Exception e)
            {
                ModConsole.Error(e.ToString());
            }
        }

        static void FixBucketSeats()
        {
            ES2.Save(true, GetDefaultES2SavePosition() + "?tag=bucket seat passenger(Clone)Purchased");
            ES2.Save(true, GetDefaultES2SavePosition() + "?tag=bucket seat driver(Clone)Purchased");
        }

        internal static Transform GetRadiatorHose3Transform()
        {
            ES2Settings settings = new ES2Settings();
            return ES2.Load<Transform>(GetDefaultES2SavePosition() + "?tag=radiator hose3(xxxxx)");
        }
    }
}
