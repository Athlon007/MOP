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
using System.IO;
using UnityEngine;
using MSCLoader;
using System.Collections.Generic;
using System.Linq;

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

        static List<SaveBugs> saveBugs;

        public static void VerifySave()
        {
            if (!File.Exists(GetDefaultES2SavePosition())) return;

            // Passenger bucket seat.
            // Check if driver bucket seat is bought and check the same for passenger one.
            // If they do not match, fix it.
            try
            {
                saveBugs = new List<SaveBugs>();

                ES2Settings setting = new ES2Settings();
                bool bucketPassengerSeat = ES2.Load<bool>(GetDefaultES2SavePosition() + "?tag=bucket seat passenger(Clone)Purchased", setting);
                bool bucketDriverSeat = ES2.Load<bool>(GetDefaultES2SavePosition() + "?tag=bucket seat driver(Clone)Purchased", setting);
                if (bucketDriverSeat != bucketPassengerSeat)
                {
                    saveBugs.Add(SaveBugs.New("Bucket Seats", "One bucket seat is present in the game world, while the other isn't - both should be in game world.", FixBucketSeats));
                }

                bool tractorTrailerAttached = ES2.Load<bool>(GetDefaultES2SavePosition() + "?tag=TractorTrailerAttached", setting);
                Transform flatbedTransform = ES2.Load<Transform>(GetDefaultES2SavePosition() + "?tag=FlatbedTransform", setting);
                Transform kekmetTransform = ES2.Load<Transform>(GetDefaultES2SavePosition() + "?tag=TractorTransform", setting);
                if (tractorTrailerAttached && Vector3.Distance(flatbedTransform.position, kekmetTransform.position) > 5.5f)
                {
                    saveBugs.Add(SaveBugs.New("Flatbed Trailer Attached", "Trailer and tractor are too far apart from each other - impossible for them to be attached.", FixDetachFlatbed));
                }


                if (saveBugs.Count > 0)
                {
                    ModPrompt.CreateYesNoPrompt($"MOP found <color=yellow>{saveBugs.Count}</color> problem{(saveBugs.Count > 1 ? "s" : "")} with your save:\n\n" +
                                      $"<color=yellow>{string.Join(", ", saveBugs.Select(f => f.BugName).ToArray())}</color>\n\n" +
                                      $"Would you like MOP to try and fix {((saveBugs.Count > 1) ? "them" : "it")}?", "MOP - Save Integrity Verification", FixAllProblems);
                }
                else
                {
                    ModConsole.Log("[MOP] MOP didn't find any problems with your save :)");
                }
            }
            catch (Exception e)
            {
                ModConsole.LogError(e.ToString());
            }
        }

        static void FixAllProblems()
        {
            int success = 0, fail = 0;

            foreach (SaveBugs bug in saveBugs)
            {
                try
                {
                    bug.Fix.Invoke();
                    success++;
                }
                catch
                {
                    fail++;
                }
            }

            string msg = $"<color=yellow>{success}</color> issue{(success > 1 ? "s have" : " has")} been fixed.";
            if (fail > 0)
            {
                msg += $"\n<color=red>{fail}</color> issue{(fail > 1 ? "s" : "")} couldn't be fixed.";
            }

            ModPrompt.CreatePrompt(msg, "MOP - Save Integrity Check");
        }

        static void FixBucketSeats()
        {
            ES2.Save(true, GetDefaultES2SavePosition() + "?tag=bucket seat passenger(Clone)Purchased");
            ES2.Save(true, GetDefaultES2SavePosition() + "?tag=bucket seat driver(Clone)Purchased");
        }

        static void FixDetachFlatbed()
        {
            ES2.Save(false, GetDefaultES2SavePosition() + "?tag=TractorTrailerAttached", new ES2Settings());
        }

        internal static Transform GetRadiatorHose3Transform()
        {
            ES2Settings settings = new ES2Settings();
            return ES2.Load<Transform>(GetDefaultES2SavePosition() + "?tag=radiator hose3(xxxxx)");
        }
    }

    struct SaveBugs
    {
        public string BugName;
        public string Description;
        public Action Fix;

        public static SaveBugs New(string bugName, string description, Action fix)
        {
            SaveBugs bug = new SaveBugs();
            bug.BugName = bugName;
            bug.Description = description;
            bug.Fix = fix;
            return bug;
        }
    }
}
