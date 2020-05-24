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

using MSCLoader;
using UnityEngine;

namespace MOP
{
    class Teimo : Place
    {
        // Store Class
        //
        // Extends Place.cs
        //
        // It is responsible for loading and unloading parts of the store, that are safe to be unloaded or loaded again.
        // It gives some performance bennefit, while still letting the shop and Teimo routines running without any issues.
        // Some objects on the WhiteList can be removed, but that needs testing.
        //
        // NOTE: That script DOES NOT disable the store itself, rather some of its childrens.

        // Objects from that whitelist will not be disabled
        // It is so to prevent from restock script and Teimo's bike routine not working

        readonly string[] blackList = {
            "STORE", "SpawnToStore", "BikeStore", "BikeHome", "Inventory", "Collider", "TeimoInShop", "Bicycle",
            "bicycle_pedals", "Pedal", "Teimo", "bodymesh", "skeleton", "pelvs", "spine", "collar", "shoulder",
            "hand", "ItemPivot", "finger", "collar", "arm", "fingers", "HeadPivot", "head", "eye_glasses_regular",
            "teimo_hat", "thig", "knee", "ankle", "TeimoCollider", "OriginalPos", "TeimoInBike", "Pivot", "pelvis",
            "bicycle", "Collider", "collider", "StoreCashRegister", "cash_register", "Register", "store_", "MESH",
            "tire", "rim", "MailBox", "GeneralItems", "(Clone)", "(itemx)", "Boxes", "n2obottle", "RagDoll",
            "thigh", "shopping bag", "Advert", "advert", "ActivateBar", "FoodSpawnPoint", "BagCreator",
            "ShoppingBagSpawn", "Post", "PayMoneyAdvert", "Name", "LOTTO", "Lottery", "lottery", "Products",
            "PRODUCST", "Microwave", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "WoodSheetStore", "WoodSheetPub",
            "Bolt", "Pin", "pin", "Handle", "Explosion", "Fire", "Smoke", "Trail", "Fireball",
            "Shower", "Dust", "Shockwave", "Force", "Sound", "Point light", "smoke", "Flame", "Dynamics", "bottle",
            "needle", "Parts", "_gfx", "LookTarget", "Speak", "Functions", "Bottle", "GrillBox", "Food",
            "BeerBottle", "VodkaShot", "CoffeeCup", "Cigarettes", "Fighter2", "TargetPoint", "RayPivot",
            "TargetSelf", "AudioClips", "HitPosition", "HumanCol", "Ray", "bodymesh_fighter", "Char", "ThrowBody",
            "PlayerRigid", "GrillboxMicro", "PhysHead", "Shades", "hat", "glasses", "FighterFist", "GameLogic",
            "Buttons", "Bet", "Double", "Hold", "InsertCoin", "Deal", "TakeWin", "Pokeri", "CashSound", "videopoker_on",
            "Hatch", "HookSlot", "Disabled", "slot_machine_off", "Money", "Lock", "Cash" };

        /// <summary>
        /// Initialize the Store class
        /// </summary>
        public Teimo() : base("STORE")
        {
            // Fix for items bought via envelope
            gameObject.transform.Find("Boxes").parent = null;

            // Fix for advertisement pile disappearing when taken
            gameObject.transform.Find("AdvertSpawn").transform.parent = null;

            GameObjectBlackList.AddRange(blackList);
            DisableableChilds = GetDisableableChilds();

            // Fix for the bar fighter
            Transform fighter = gameObject.transform.Find("Fighter2/Pivot");
            if (fighter)
                DisableableChilds.Remove(fighter);

            // Injecting HookSlot of VideoPoker object
            Transform videoPoker = gameObject.transform.Find("LOD/VideoPoker/HookSlot");
            if (videoPoker)
                FsmHook.FsmInject(videoPoker.gameObject, "Activate cable", RemoveVideoPokerParent);

            // Fix for Z-fighting of slot machine glass.
            transform.Find("LOD/GFX_Store/SlotMachine/slot_machine 1/slot_machine_glass").gameObject.GetComponent<Renderer>().material.renderQueue = 3001;
        }

        /// <summary>
        /// If triggered, sets the VideoPoker parent object to null.
        /// This fixes issues with it disappearing while towing it too far from store.
        /// </summary>
        void RemoveVideoPokerParent()
        {
            Transform poker = gameObject.transform.Find("LOD/VideoPoker");
            if (poker == null)
                return;

            DisableableChilds.Remove(poker);
            poker.transform.parent = null;
        }
    }
}
