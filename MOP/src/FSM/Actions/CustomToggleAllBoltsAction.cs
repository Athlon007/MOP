using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace MOP.FSM.Actions
{
    class CustomToggleAllBoltsAction : FsmStateAction
    {
        List<GameObject> bolts;
        bool isEnabled;

        public CustomToggleAllBoltsAction(GameObject boltsParent, bool isEnabled)
        {
            this.isEnabled = isEnabled;
            bolts = new List<GameObject>();
            bolts.Add(boltsParent.gameObject);
            foreach (Transform child in boltsParent.GetComponentsInChildren<Transform>())
            {
                bolts.Add(child.gameObject);
            }
        }

        public override void OnEnter()
        {
            foreach (GameObject obj in bolts)
            {
                obj.SetActive(isEnabled);
            }

            Finish();
        }
    }
}
