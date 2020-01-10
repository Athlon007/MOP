using HutongGames.PlayMaker;
using UnityEngine;

namespace MOP
{
    class MopFsmManager
    {
        static PlayMakerFSM unclePlaymaker;

        /// <summary>
        /// Checks if the player has the keys to the Hayosiko.
        /// </summary>
        /// <returns></returns>
        public static bool PlayerHasHayosikoKey()
        {
            if (unclePlaymaker == null)
            {
                unclePlaymaker = GameObject.Find("UNCLE").GetComponent<PlayMakerFSM>();
            }

            return unclePlaymaker.FsmVariables.GetFsmInt("UncleStage").Value == 5;
        }
    }
}
