using HutongGames.PlayMaker;

using MOP.Vehicles.Managers.SatsumaManagers;

namespace MOP.FSM.Actions
{
    class WindscreenRepairJob : FsmStateAction
    {
        SatsumaWindscreenFixer satsumaWindscreenFixer;

        public WindscreenRepairJob(SatsumaWindscreenFixer satsumaWindscreenFixer)
        {
            this.satsumaWindscreenFixer = satsumaWindscreenFixer;
        }

        public override void OnEnter()
        {
            satsumaWindscreenFixer.FixWindscreen();
        }
    }
}
