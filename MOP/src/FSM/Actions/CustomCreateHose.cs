﻿// Modern Optimization Plugin
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

using UnityEngine;

using MOP.Items;
using MOP.Managers;

namespace MOP.FSM.Actions
{
    class CustomCreateHose : CustomCreateObject
    {
        public CustomCreateHose(GameObject parent, GameObject prefab) : base(parent, prefab)
        {
            GameObject newPrefab = GameObject.Instantiate(prefab);
            newPrefab.name = newPrefab.name.Replace("(Clone)(Clone)", "(Clone)");
            this.prefab = newPrefab;
            Object.Destroy(this.prefab.GetComponent<ItemBehaviour>());
            this.prefab.SetActive(false);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            ItemsManager.Instance.SetCurrentRadiatorHose(newObject);
        }
    }
}
