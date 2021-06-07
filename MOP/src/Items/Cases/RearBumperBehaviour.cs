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
using System.Collections.Generic;
using UnityEngine;
using MSCLoader.Helper;

using MOP.Vehicles.Cases;

namespace MOP.Items.Cases
{
    class RearBumperBehaviour : MonoBehaviour
    {
        Collider coll;
        List<Collider> ignored;

        bool seekColliders;

        PlayMakerFSM fsm;

        void Start()
        {
            coll = GetComponent<Collider>();
            ignored = new List<Collider>();
            fsm = gameObject.GetPlayMakerFSM("BoltCheck");

            seekColliders = transform.root == Satsuma.Instance.transform;
            if (seekColliders)
            {
                Collider[] colls = Physics.OverlapSphere(transform.position, 5);
                foreach (Collider coll in colls)
                {
                    if (coll.transform.root == Satsuma.Instance.transform)
                    {
                        Physics.IgnoreCollision(this.coll, coll);
                        ignored.Add(coll);
                    }
                }

                StartCoroutine(ReloadBolts());
            }
        }

        IEnumerator ReloadBolts()
        {
            // A small hack. For some reason, bolts of the rear bumper sometimes don't want to load properly.
            // We basically allow the rear bumper to reset the FSM, so it reloads the bolt stages.
            fsm.Fsm.RestartOnEnable = true;
            yield return null;
            fsm.Fsm.RestartOnEnable = false;
        }

        void OnCollisionEnter(Collision other)
        {
            if (!seekColliders) return;

            if (other.transform.root == Satsuma.Instance.transform && !ignored.Contains(other.collider))
            {
                Physics.IgnoreCollision(coll, other.collider);
                ignored.Add(other.collider);
            }
        }

        internal void OnDetach()
        {
            StartCoroutine(DetachAction());
        }

        IEnumerator DetachAction()
        {
            yield return new WaitForSeconds(1);
            for (int i = 0; i < ignored.Count; i++)
            {
                if (ignored[i] != null)
                {
                    Physics.IgnoreCollision(coll, ignored[i], false);
                }
            }

            seekColliders = false;
        }

        internal void OnAttach()
        {
            seekColliders = true;
        }
    }
}
