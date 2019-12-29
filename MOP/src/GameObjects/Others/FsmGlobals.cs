using HutongGames.PlayMaker;

namespace MOP
{
    class FsmGlobals
    {
        public static bool PlayerHasHayosikoKey()
        {
            return FsmVariables.GlobalVariables.GetFsmInt("PlayerKeyHayosiko").Value == 1;
        }
    }
}
