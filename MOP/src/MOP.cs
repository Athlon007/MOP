using MSCLoader;
using UnityEngine;

namespace MOP
{
    public class MOP : Mod
    {
        public override string ID => "MOP"; //Your mod ID (unique)
        public override string Name => "Modern Optimizaiton Plugin"; //You mod name
        public override string Author => "Athlon"; //Your Username
        public override string Version => "Beta 1.0.0"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => false;

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void OnLoad()
        {
            MopSettings.UpdateValues();

            // Initialize the WorldManager class
            GameObject worldManager = new GameObject("WorldManager");
            worldManager.AddComponent<WorldManager>();
        }

        public static Settings activeDistance = new Settings("activeDistance", "Active Distance", 1, MopSettings.UpdateValues);

        /// <summary>
        /// All settings should be created here. 
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
            Settings.AddHeader(this, "Settings", new Color32(29, 29, 29, 255));
            Settings.AddSlider(this, activeDistance, 0, 2);
            Settings.AddText(this, "From how close objects are enabled.\n - 0: Close\n - 1: Normal (Default)\n - 2: Far");
            Settings.AddHeader(this, "Changelog", new Color32(29, 29, 29, 255));
            Settings.AddText(this, Properties.Resources.changelog);
        }
    }
}
