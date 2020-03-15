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
using HutongGames.PlayMaker;

namespace MOP
{
    class MopFsmManager
    {
        static FsmInt uncleStage;
        static FsmBool gtGrilleInstalled;
        static FsmFloat orderTime;

        /// <summary>
        /// Checks if the player has the keys to the Hayosiko.
        /// </summary>
        /// <returns></returns>
        public static bool PlayerHasHayosikoKey()
        {
            // Store Uncle's PlayMakerFSM for later
            if (uncleStage == null)
                uncleStage = GameObject.Find("UNCLE").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("UncleStage");

            return uncleStage.Value == 5;
        }

        /// <summary>
        /// Checks if GT Grille is attached to the car.
        /// </summary>
        /// <returns></returns>
        public static bool IsGTGrilleInstalled()
        {
            if (gtGrilleInstalled == null)
                gtGrilleInstalled = GameObject.Find("Database").transform.Find("DatabaseOrders/GrilleGT").gameObject.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Installed");
            
            return gtGrilleInstalled.Value == true;
        }

        /// <summary>
        /// Checks if repair shop job has been ordered.
        /// </summary>
        /// <returns></returns>
        public static bool IsRepairshopJobOrdered()
        {
            if (orderTime == null)
                orderTime = GameObject.Find("REPAIRSHOP/Order").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("_OrderTime");

            return orderTime.Value > 0 && orderTime.Value != 5000;
        }
    }
}
