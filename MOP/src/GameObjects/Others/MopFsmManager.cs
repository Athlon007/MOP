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

using HutongGames.PlayMaker;
using UnityEngine;

namespace MOP
{
    static class MopFsmManager
    {
        static FsmInt uncleStage;
        static FsmBool gtGrilleInstalled;
        static FsmBool order;
        static FsmString playerCurrentVehicle;
        static FsmInt farmJobStage;
        static FsmBool hoodBolted;
        static FsmBool fiberHoodBolted;
        static FsmBool suskiLarge;
        static GameObject triggerHood;
        static GameObject kekmetTrailerRemove;
        static FsmFloat battery1;
        static FsmFloat battery2;
        static FsmBool playerHelmet;

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

        public static bool IsStockHoodBolted()
        {
            if (hoodBolted == null)
                hoodBolted = GameObject.Find("Database/DatabaseBody/Hood").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Bolted");

            return hoodBolted.Value;
        }

        public static bool IsFiberHoodBolted()
        {
            if (fiberHoodBolted == null)
                fiberHoodBolted = GameObject.Find("Database/DatabaseOrders/Fiberglass Hood").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Bolted");

            return fiberHoodBolted.Value;
        }

        public static void ForceHoodAssemble()
        {
            if (triggerHood == null)
                triggerHood = GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Body/trigger_hood").gameObject;

            triggerHood.SetActive(true);
            triggerHood.GetComponent<PlayMakerFSM>().SendEvent("ASSEMBLE");
        }

        public static bool IsSuskiLargeCall()
        {
            if (suskiLarge == null)
                suskiLarge = GameObject.Find("YARD/Building/LIVINGROOM/Telephone/Logic/PhoneLogic").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("SuskiLarge");

            return suskiLarge.Value;
        }

        public static bool IsTrailerAttached()
        {
            if (kekmetTrailerRemove == null)
                kekmetTrailerRemove = GameObject.Find("KEKMET(350-400psi)").transform.Find("Trailer/Remove").gameObject;

            return kekmetTrailerRemove.activeSelf;
        }

        public static bool IsBatteryInstalled()
        {
            if (battery1 == null)
                battery1 = GameObject.Find("Database/PartsStatus/Battery").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("Bolt1");

            if (battery2 == null)
                battery2 = GameObject.Find("Database/PartsStatus/Battery").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("Bolt2");

            return battery1.Value > 0 || battery2.Value > 0;
        }

        public static bool PlayerHelmetOn()
        {
            if (playerHelmet == null)
                playerHelmet = PlayMakerGlobals.Instance.Variables.GetFsmBool("PlayerHelmet");

            return playerHelmet.Value;
        }
    }
}
