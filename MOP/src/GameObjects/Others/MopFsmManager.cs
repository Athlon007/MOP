// Modern Optimization Plugin
// Copyright(C) 2019-2020 Athlon

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

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
            // Store Uncle's PlayMakerFSM for later
            if (unclePlaymaker == null)
            {
                unclePlaymaker = GameObject.Find("UNCLE").GetComponent<PlayMakerFSM>();
            }

            return unclePlaymaker.FsmVariables.GetFsmInt("UncleStage").Value == 5;
        }

        static PlayMakerFSM databaseGTGrille;

        public static bool IsGTGrilleInstalled()
        {
            if (databaseGTGrille == null)
                databaseGTGrille = GameObject.Find("Database").transform.Find("DatabaseOrders/GrilleGT").gameObject.GetComponent<PlayMakerFSM>();
            
            return databaseGTGrille.FsmVariables.GetFsmBool("Installed").Value == true;
        }
    }
}
