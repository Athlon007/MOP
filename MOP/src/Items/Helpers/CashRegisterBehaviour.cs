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

using System.Collections;
using System.Linq;
using UnityEngine;

using MOP.Common;
using MOP.Managers;

namespace MOP.Items.Helpers
{
    class CashRegisterBehaviour : MonoBehaviour
    {
        public void InitializeList()
        {
            // Find shopping bags in the list
            GameObject[] items = FindObjectsOfType<GameObject>()
                                .Where(gm => gm.name.ContainsAny(ItemsManager.Instance.NameList) && gm.name.ContainsAny("(itemx)", "(Clone)") & gm.GetComponent<ItemBehaviour>() == null)
                                .ToArray();

            if (items.Length > 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == null) continue;

                    try
                    {
                        items[i].AddComponent<ItemBehaviour>();
                    }
                    catch { }
                }
            }
        }

        IEnumerator packagesRoutine;
        public void Packages()
        {
            if (packagesRoutine != null)
            {
                StopCoroutine(packagesRoutine);
            }

            packagesRoutine = PackagesCoroutine();
            StartCoroutine(packagesRoutine);
        }

        IEnumerator PackagesCoroutine()
        {
            yield return new WaitForSeconds(2);

            var packages = GameObject.FindGameObjectsWithTag("ITEM").Where(g => g.name == "amis-auto ky package(xxxxx)" && g.activeSelf).ToArray();
            for (int i = 0; i < packages.Length; ++i)
            {
                packages[i].AddComponent<ItemBehaviour>();
                Transform parts = packages[i].transform.Find("Parts");
                Transform[] items = parts.GetComponentsInChildren<Transform>(true).Where(t => t.parent == parts).ToArray();
                for (int j = 0; j < items.Length; ++j)
                {
                    items[j].gameObject.AddComponent<ItemBehaviour>();
                }
            }
        }
    }
}
