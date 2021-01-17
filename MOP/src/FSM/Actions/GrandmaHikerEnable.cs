using HutongGames.PlayMaker;
using UnityEngine;

namespace MOP.FSM.Actions
{
    class GrandmaHikerEnable : FsmStateAction
    {
        Animator animator;

        public GrandmaHikerEnable(GameObject skeleton)
        {
            animator = skeleton.GetComponent<Animator>();
        }

        public override void OnEnter()
        {
            animator.enabled = true;
        }
    }

    class GrandmaHikerDisable : FsmStateAction
    {
        Animator animator;

        public GrandmaHikerDisable(GameObject skeleton)
        {
            animator = skeleton.GetComponent<Animator>();
        }

        public override void OnEnter()
        {
            animator.enabled = false;
        }
    }
}
