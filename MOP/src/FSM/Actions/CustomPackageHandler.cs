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

using System.Linq;
using UnityEngine;

using MOP.Items;

namespace MOP.FSM.Actions
{
    class CustomPackageHandler : HutongGames.PlayMaker.FsmStateAction
    {
        Transform[] items;

        public CustomPackageHandler(GameObject gm)
        {
            Transform parts = gm.transform.Find("Parts");
            items = parts.GetComponentsInChildren<Transform>(true).Where(t => t.parent == parts).ToArray();
            MSCLoader.ModConsole.Log(string.Join(", ", items.Select(g => g.name).ToArray()));
        }

        public override void OnEnter()
        {
            for (int j = 0; j < items.Length; ++j)
            {
                items[j].gameObject.SetActive(true);
                items[j].gameObject.AddComponent<ItemBehaviour>();
                items[j].gameObject.SetActive(false);
            }
            Finish();
        }
    }
}
