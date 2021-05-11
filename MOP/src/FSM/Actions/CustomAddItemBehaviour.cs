using HutongGames.PlayMaker;
using MOP.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            MSCLoader.ModConsole.Log("ADDED TO " + go.Value.name);
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
