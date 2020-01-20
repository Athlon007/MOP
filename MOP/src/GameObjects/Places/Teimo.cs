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

        string[] blackList = { 
            "STORE", "SpawnToStore", "BikeStore", "BikeHome", "Inventory", "Collider", "TeimoInShop", "Bicycle",
            "bicycle_pedals", "Pedal", "Teimo", "bodymesh", "skeleton", "pelvs", "spine", "collar", "shoulder",
            "hand", "ItemPivot", "finger", "collar", "arm", "fingers", "HeadPivot", "head", "eye_glasses_regular",
            "teimo_hat", "thig", "knee", "ankle", "TeimoCollider", "OriginalPos", "TeimoInBike", "Pivot", "pelvis",
            "bicycle", "Collider", "collider", "StoreCashRegister", "cash_register", "Register", "store_", "MESH",
            "LOD", "tire", "rim", "MailBox", "GeneralItems", "(Clone)", "(itemx)", "Boxes", "n2obottle", "RagDoll",
            "thigh", "shopping bag", "Advert", "advert", "ActivateBar", "FoodSpawnPoint", "BagCreator",
            "ShoppingBagSpawn", "Post", "PayMoneyAdvert", "Name", "LOTTO", "Lottery", "lottery", "Products",
            "PRODUCST", "Microwave", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "WoodSheetStore", "WoodSheetPub",
            "Bolt", "Pin", "pin", "Handle", "Explosion", "Fire", "GasolineFire", "Smoke", "Trail", "Fireball",
            "Shower", "Dust", "Shockwave", "Force", "Sound", "Point light", "smoke", "Flame", "Dynamics", "bottle",
            "needle", "Parts", "_gfx", "LookTarget", "Speak", "Functions", "Bottle", "GrillBox", "Food",
            "BeerBottle", "VodkaShot", "CoffeeCup", "Cigarettes" };

        /// <summary>
        /// Initialize the Store class
        /// </summary>
        public Teimo() : base("STORE")
        {
            GameObjectBlackList = blackList;
            DisableableChilds = GetDisableableChilds();
        }
    }
}
