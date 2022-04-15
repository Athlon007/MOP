using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MOP.Items;

namespace MOP.Stacks
{
    internal class ItemStack
    {
        ItemBehaviour[] items;
        int count;

        public ItemStack()
        {
            items = new ItemBehaviour[4096];
        }

        public void Pull(ItemBehaviour behaviour)
        {
            if (count >= items.Length)
            {
                ModConsole.Log("[MOP] ItemStack Full");
                return;
            }

            items[count] = behaviour;
            count++;
        }

        public ItemBehaviour Pop()
        {
            if (count <= 0) return null;

            count--;
            return items[count];
        }

        public int Count => count;
        public bool IsEmpty => Count == 0;
    }
}
