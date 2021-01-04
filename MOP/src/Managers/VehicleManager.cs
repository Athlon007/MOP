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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOP.Vehicles;
using MOP.Common;
using MOP.Common.Enumerations;

namespace MOP.Managers
{
    class VehicleManager
    {
        static VehicleManager instance;
        public static VehicleManager Instance { get => instance; }

        public Vehicle this[int index] => vehicles[index];

        // Vehicles object names of My Summer Car.
        readonly string[] vehicleArrayMSC =
        {
            "SATSUMA(557kg, 248)",
            "HAYOSIKO(1500kg, 250)",
            "JONNEZ ES(Clone)",
            "KEKMET(350-400psi)",
            "RCO_RUSCKO12(270)",
            "FERNDALE(1630kg)",
            "FLATBED",
            "GIFU(750/450psi)",
            "BOAT",
            "COMBINE(350-400psi)"
        };

        // owo, what's this?
        readonly string[] vehicleArrayMWC = { };

        List<Vehicle> vehicles;

        public VehicleManager()
        {
            instance = this;

            // Loading vehicles
            vehicles = new List<Vehicle>();
            string[] activeVehicleArray = MopSettings.IsMySummerCar() ? vehicleArrayMSC : vehicleArrayMWC;
            
            foreach (string vehicle in activeVehicleArray)
            {
                try
                {
                    if (GameObject.Find(vehicle) == null) continue;

                    Vehicle newVehicle = null;
                    switch (vehicle)
                    {
                        default:
                            newVehicle = new Vehicle(vehicle);
                            break;
                        case "SATSUMA(557kg, 248)":
                            newVehicle = new Satsuma(vehicle);
                            break;
                        case "BOAT":
                            newVehicle = new Boat(vehicle);
                            break;
                        case "COMBINE(350-400psi)":
                            newVehicle = new Combine(vehicle);
                            break;
                    }

                    vehicles.Add(newVehicle);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, $"VEHICLE_LOAD_ERROR_{vehicle}");
                }
            }

            ModConsole.Print("[MOP] Vehicles initialized");
        }

        public void Add(Vehicle vehicle)
        {
            vehicles.Add(vehicle);
        }

        public int Count => vehicles.Count;

        public List<Vehicle> List()
        {
            return vehicles;
        }

        public void RemoveAt(int i)
        {
            vehicles.RemoveAt(i);
        }

        public Vehicle GetVehicle(VehiclesTypes vehicleType)
        {
            if (vehicleType == VehiclesTypes.Generic)
            {
                throw new Exception("Can't get generic type.");
            }

            return vehicles.FirstOrDefault(f => f.VehicleType == vehicleType);
        }
    }
}
