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
using UnityEngine;

using MOP.Items;
using MOP.Managers;

namespace MOP.FSM.Actions
{
    /// <summary>
    /// This FsmAction role is to instantiate a gameobject.
    /// </summary>
    class CustomCreateObject : FsmStateAction
    {
        GameObject parent;
        GameObject prefab;

        public CustomCreateObject(GameObject parent, GameObject prefab)
        {
            this.parent = parent;
            this.prefab = prefab;
        }

        public override void OnEnter()
        {
            GameObject newObject = GameObject.Instantiate(prefab);
            newObject.transform.position = parent.transform.position;
            newObject.name = newObject.name.Replace("(Clone)(Clone)", "(Clone)");
            newObject.SetActive(true);
            newObject.AddComponent<ItemBehaviour>();

            ItemsManager.Instance.SetCurrentRadiatorHose(newObject);
        }
    }
}
