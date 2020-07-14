// Modern Optimization Plugin
// Copyright(C) 2019-2020 Athlon

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

namespace MOP
{
    class TrailerLogUnderFloor : MonoBehaviour
    {
        // This objects hooks right above the log that is supposd to hold the trailer on ground.
        // If the object collides with map, resets trailer rotation to default.

        readonly GameObject log;
        readonly Transform flatbed;

        public TrailerLogUnderFloor()
        {
            flatbed = transform.parent;
            log = flatbed.Find("Log").gameObject;

            transform.localPosition = new Vector3(0, 0.5f, 3.3f);
            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(0.5f, 0.5f, 0.5f);
        }

        void OnTriggerStay(Collider other)
        {
            if (!log.activeSelf) return;

            if (other.gameObject.transform.root.gameObject.name == "MAP")
            {
                Vector3 angles = flatbed.localEulerAngles;
                angles.x = 0;
                flatbed.localEulerAngles = angles;
            }
        }
    }
}
