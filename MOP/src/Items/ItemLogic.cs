using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MSCLoader;
using MSCLoader.Helper;
using MSCLoader.PartMagnet;

using MOP.Managers;

namespace MOP.Items
{
    class ItemLogic : MonoBehaviour
    {
        // Compared to ItemBehaviour, ItemLogic attaches to the PREFABS of the object.
        // It does that on PreLoad().

        Rigidbody rigidbody;
        Renderer renderer;
        PartMagnet partMagnet;
        BoltMagnet boltMagnet;

        public ItemLogic()
        {
            rigidbody = GetComponent<Rigidbody>();
            renderer = GetComponent<Renderer>();
            partMagnet = GetComponent<PartMagnet>();
            boltMagnet = GetComponent<BoltMagnet>();

            // If the item is a shopping bag, hook the RemoveSelf to "Is garbage" FsmState
            if (gameObject.name.Contains("shopping bag"))
            {
                FsmHook.FsmInject(this.gameObject, "Is garbage", OnDestroy);

                // Destroys empty shopping bags appearing in the back of the yard.
                PlayMakerArrayListProxy list = gameObject.GetComponent<PlayMakerArrayListProxy>();
                if (list.arrayList.Count == 0)
                {
                    Destroy(this.gameObject);
                }
            }

            if (this.gameObject.name == "helmet(itemx)")
            {
                return;
            }
        }

        void Awake()
        {

        }

        // https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDestroy.html
        void OnDestroy()
        {
            ItemsManager.Instance.Remove(this);
        }
    }
}
