// Modern Optimization Plugin
// Copyright(C) 2019-2021 Athlon

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

using System.Collections.Generic;
using UnityEngine;
using MSCLoader;
using HutongGames.PlayMaker.Actions;
using MOP.Common;

namespace MOP.Places
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

        readonly string[] blackList =
        {
            "STORE", "SpawnToStore", "BikeStore", "BikeHome", "Inventory", "Collider", "TeimoInShop", "Bicycle",
            "bicycle_pedals", "Pedal", "Teimo", "bodymesh", "skeleton", "pelvs", "spine", "collar", "shoulder",
            "hand", "ItemPivot", "finger", "collar", "arm", "fingers", "HeadPivot", "head", "eye_glasses_regular",
            "teimo_hat", "thig", "knee", "ankle", "OriginalPos", "TeimoInBike", "Pivot", "pelvis",
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
            "Hatch", "HookSlot", "Disabled", "slot_machine_off", "Money", "Lock", "Cash", "Accessories", "VideoPoker", "videopoker",
            "Monitor", "screen", "button_", "BreakableWindows", "BreakableWindows"
        };

        /// <summary>
        /// Initialize the Store class
        /// </summary>
        public Teimo() : base("STORE")
        {
            // Fix for items bought via envelope
            transform.Find("Boxes").parent = null;

            // Fix for advertisement pile disappearing when taken
            transform.Find("AdvertSpawn").transform.parent = null;
            
            // We're nulling the parent of the fucking video poker game,
            // because that's much easier than hooking it...
            Transform videoPoker = transform.Find("LOD/VideoPoker/HookSlot");
            if (videoPoker != null)
                FsmHook.FsmInject(videoPoker.gameObject, "Activate cable", RemoveVideoPokerParent);

            GameObjectBlackList.AddRange(blackList);
            DisableableChilds = GetDisableableChilds();

            // Remove video poker meshes.
            DisableableChilds.Remove(transform.Find("LOD/VideoPoker/Hatch/Pivot/mesh"));

            // Fix for Z-fighting of slot machine glass.
            transform.Find("LOD/GFX_Store/SlotMachine/slot_machine 1/slot_machine_glass")
                .gameObject.GetComponent<Renderer>().material.renderQueue = 3001;

            PlayMakers.AddRange(transform.Find("TeimoInShop").GetComponents<PlayMakerFSM>());
            PlayMakers.AddRange(transform.Find("TeimoInShop").GetComponents<PlayMakerFSM>());

            List<Transform> teimoShit = new List<Transform>();
            teimoShit.Add(transform.Find("TeimoInShop/Pivot/Speak"));
            teimoShit.Add(transform.Find("TeimoInShop/Pivot/FacePissTrigger"));
            teimoShit.Add(transform.Find("TeimoInShop/Pivot/TeimoCollider"));
            teimoShit.Add(transform.Find("GasolineFire"));
            DisableableChilds.AddRange(teimoShit);

            // Disable resetting of the breakable windows.
            Transform storeBreakableWindow = GameObject.Find("STORE").transform.Find("LOD/GFX_Store/BreakableWindows/BreakableWindow");
            if (storeBreakableWindow != null)
            {
                foreach (PlayMakerFSM fsm in storeBreakableWindow.gameObject.GetComponents<PlayMakerFSM>())
                    fsm.Fsm.RestartOnEnable = false;
            }
            Transform storeBreakableWindowPub = GameObject.Find("STORE").transform.Find("LOD/GFX_Pub/BreakableWindowsPub/BreakableWindowPub");
            if (storeBreakableWindowPub != null)
            {
                foreach (PlayMakerFSM fsm in storeBreakableWindowPub.gameObject.GetComponents<PlayMakerFSM>())
                    fsm.Fsm.RestartOnEnable = false;
            }

            // Disables the idiotic distance limit, below which if player is too close to the Teimo's, the restocking won't be executed.
            FloatCompare checkDistance = transform.Find("Inventory").gameObject.GetComponent<PlayMakerFSM>().FindFsmState("Check distance").Actions[1] as FloatCompare;
            checkDistance.float2 = 0;
        }

        /// <summary>
        /// If triggered, sets the VideoPoker parent object to null.
        /// This fixes issues with it disappearing while towing it too far from store.
        /// </summary>
        void RemoveVideoPokerParent()
        {
            Transform poker = GameObject.Find("VideoPoker").transform;
            DisableableChilds.Remove(poker);
            poker.transform.parent = null;
        }
    }
}
