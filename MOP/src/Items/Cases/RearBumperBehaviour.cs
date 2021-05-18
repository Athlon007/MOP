using MOP.Vehicles.Cases;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOP.Items.Cases
{
    class RearBumperBehaviour : MonoBehaviour
    {
        Collider coll;
        List<Collider> ignored;

        bool seekColliders;

        void Start()
        {
            coll = GetComponent<Collider>();
            ignored = new List<Collider>();

            if (transform.root == Satsuma.Instance.transform) 
                seekColliders = true;
        }

        void OnCollisionEnter(Collision other)
        {
            if (!seekColliders) return;

            if (other.transform.root == Satsuma.Instance.transform)
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

            ignored.Clear();

            seekColliders = false;
        }

        internal void OnAttach()
        {
            seekColliders = true;
        }
    }
}
