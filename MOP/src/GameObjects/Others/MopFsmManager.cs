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
        static FsmBool order;
        static FsmString playerCurrentVehicle;
        static FsmInt farmJobStage;

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
            if (order == null)
                order = GameObject.Find("REPAIRSHOP/Order").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("_Order");

            return order.Value;
        }

        /// <summary>
        /// Checks PlayerCurrentVehicle global value. If it says "Satsuma", that means player sits in Satsuma.
        /// </summary>
        /// <returns></returns>
        public static bool IsPlayerInSatsuma()
        {
            if (playerCurrentVehicle == null)
                playerCurrentVehicle = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");

            return playerCurrentVehicle.Value == "Satsuma";
        }

        public static bool IsPlayerInCar()
        {
            if (playerCurrentVehicle == null)
                playerCurrentVehicle = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");

            return playerCurrentVehicle.Value.Length > 0;
        }

        /// <summary>
        /// Checks if the JobStage of Farm Job has rechead 3rd stage.
        /// On 3rd stage, the combine is available to the player.
        /// </summary>
        /// <returns></returns>
        public static bool IsCombineAvailable()
        {
            if (farmJobStage == null)
                farmJobStage = GameObject.Find("JOBS/Farm/Job").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("JobStage");

            return farmJobStage.Value >= 3;
        }
    }
}
