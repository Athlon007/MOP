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

using HutongGames.PlayMaker;
using MOP.Items;
using System.Collections;
using UnityEngine;

namespace MOP.FSM.Actions
{
    class CustomAddItemBehaviour : FsmStateAction
    {
        FsmGameObject go;

        public CustomAddItemBehaviour(FsmGameObject go)
        {
            this.go = go;
        }

        public override void OnEnter()
        {
            this.go.Value.AddComponent<DelayedItemBehaviour>();
        }
    }

    class DelayedItemBehaviour : MonoBehaviour
    { 
        void Start()
        {
            StartCoroutine(DelayAdd());
        }

        IEnumerator DelayAdd()
        {
            for (int i = 0; i < 60; ++i)
                yield return null;
            gameObject.AddComponent<ItemBehaviour>();
            Destroy(this);
        }
    }
}
