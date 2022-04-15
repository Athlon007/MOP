using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MOP.Vehicles;

namespace MOP.Stacks
{
    internal class VehicleStack
    {
        Vehicle[] items;
        int count;

        public VehicleStack()
        {
            items = new Vehicle[4096];
        }

        public void Pull(Vehicle behaviour)
        {
            if (count >= items.Length)
            {
                ModConsole.Log("[MOP] VehicleStack Full");
                return;
            }

            items[count] = behaviour;
            count++;
        }

        public Vehicle Pop()
        {
            if (count <= 0) return null;

            count--;
            return items[count];
        }
    }
}
