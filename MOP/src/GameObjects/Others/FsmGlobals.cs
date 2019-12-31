using HutongGames.PlayMaker;

namespace MOP
{
    class FsmGlobals
    {
        /// <summary>
        /// Checks if the player has the keys to the Hayosiko.
        /// </summary>
        /// <returns></returns>
        public static bool PlayerHasHayosikoKey()
        {
            return FsmVariables.GlobalVariables.GetFsmInt("PlayerKeyHayosiko").Value == 1;
        }
    }
}
