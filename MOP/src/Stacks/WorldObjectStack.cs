using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MOP.WorldObjects;

namespace MOP.Stacks
{
    internal class WorldObjectStack
    {
        GenericObject[] items;
        int count;

        public WorldObjectStack()
        {
            items = new GenericObject[4096];
        }

        public void Pull(GenericObject behaviour)
        {
            if (count >= items.Length)
            {
                ModConsole.Log("[MOP] VehicleStack Full");
                return;
            }

            items[count] = behaviour;
            count++;
        }

        public GenericObject Pop()
        {
            if (count <= 0) return null;

            count--;
            return items[count];
        }
    }
}
