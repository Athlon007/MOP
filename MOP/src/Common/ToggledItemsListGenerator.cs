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

using System.Collections.Generic;
using System.IO;
using UnityEngine;

using MOP.Items;
using MOP.Places;
using MOP.Vehicles;
using MOP.WorldObjects;

namespace MOP.Common
{
    class ToggledItemsListGenerator
    {
        const string ListFolder = "MOP_Lists";

        static void Write(string filename, string input)
        {
            Directory.CreateDirectory(ListFolder);
            string path = Path.Combine(ListFolder, filename);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            StreamWriter writer = new StreamWriter(path);
            writer.Write(input);
            writer.Close();
        }

        public static void CreateWorldList(List<GenericObject> list)
        {
            string output = "";
            foreach (var obj in list)
            {
                if (!output.Contains(obj.GetName()))
                {
                    output += $"{obj.GetName()}, ";
                }
            }

            Write("world.txt", output);
        }

        public static void CreateVehicleList(List<Vehicle> list)
        {
            string output = "";
            foreach (var obj in list)
            {
                output += $"{obj.gameObject.name}, ";
            }

            Write("vehicle.txt", output);
        }

        public static void CreateItemsList(List<ItemBehaviour> list)
        {
            string output = "";
            foreach (var obj in list)
            {
                output += $"{obj.gameObject.name}, ";
            }

            Write("items.txt", output);
        }

        public static void CreatePlacesList(List<Place> list)
        {
            string output = "";
            foreach (var obj in list)
            {
                output += $"{obj.GetName()}, ";
            }

            Write("places.txt", output);
        }

        public static void CreateSectorList(List<GameObject> list)
        {
            string output = "";
            foreach (var obj in list)
            {
                output += $"{obj.name}, ";
            }

            Write("sector.txt", output);
        }

        public static void CreateSatsumaList(Transform[] list)
        {
            string output = "";
            foreach (var obj in list)
            {
                output += $"{obj.gameObject.name}, ";
            }

            Write("satsuma.txt", output);
        }

        public void OpenFolder()
        {
            System.Diagnostics.Process.Start(ListFolder);
        }
    }
}
