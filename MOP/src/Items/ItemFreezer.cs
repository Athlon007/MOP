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

using UnityEngine;

namespace MOP.Items
{
    class ItemFreezer : MonoBehaviour
    {
        // It's purpose is to literally freeze the item in place while saving the game, so the motherfucker won't move.
        Vector3 position;
        Quaternion rotation;

        private readonly Rigidbody rb;

        // This fixes a problem of items that have been moving going to stop, and teleporting back to the position initialy saved.
        bool hasBeenMoving;

        public ItemFreezer()
        {
            position = transform.position;
            rotation = transform.rotation;

            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (rb != null && rb.velocity.magnitude > 0.1f)
            {
                hasBeenMoving = true;
                return;
            }

            if (hasBeenMoving)
            {
                position = transform.position;
                rotation = transform.rotation;
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
