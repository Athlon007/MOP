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

using System;
using System.Collections.Generic;

using MOP.Places;
using MOP.Common;
using MOP.Common.Interfaces;

namespace MOP.Managers
{
    class PlaceManager : IManager<Place>
    {
        private static PlaceManager instance;
        public static PlaceManager Instance { get => instance; }

        public Place this[int index] => places[index];

        readonly List<Place> places;

        public PlaceManager()
        {
            instance = this;

            try
            {
                places = new List<Place>();
                places.Add(new Yard());
                places.Add(new Teimo());
                places.Add(new RepairShop());
                places.Add(new Inspection());
                places.Add(new Farm());

                ModConsole.Log("[MOP] Places initialized");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "PLACES_INITIALIZATION_FAILURE");
            }
        }

        public int Count => places.Count;

        public List<Place> GetAll => places;

        public Place Add(Place obj)
        {
            places.Add(obj);
            return obj;
        }

        public void Remove(Place obj)
        {
            if (places.Contains(obj))
            {
                places.Remove(obj);
            }
        }

        public void RemoveAt(int index)
        {
            places.RemoveAt(index);
        }

        public int EnabledCount
        {
            get
            {
                int enabled = 0;
                foreach (Place place in places)
                {
                    if (place.IsActive)
                    {
                        enabled++;
                    }
                }
                return enabled;
            }
        }
    }
}
