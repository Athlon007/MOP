using HutongGames.PlayMaker;
using MOP.Items.Cases;

namespace MOP.FSM.Actions
{
    class CustomSatsumaBumperDetach : FsmStateAction
    {
        RearBumperBehaviour behaviour;

        public CustomSatsumaBumperDetach(RearBumperBehaviour behaviour)
        {
            this.behaviour = behaviour;
        }

        public override void OnEnter()
        {
            behaviour.OnDetach();
            Finish();
        }
    }

    class CustomSatsumaBumperAttach : FsmStateAction
    {
        RearBumperBehaviour behaviour;

        public CustomSatsumaBumperAttach(RearBumperBehaviour behaviour)
        {
            this.behaviour = behaviour;
        }

        public override void OnEnter()
        {
            behaviour.OnAttach();
            Finish();
        }
    }
}
