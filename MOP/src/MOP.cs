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

            if ((bool)enableObjectOcclusion.GetValue() == true)
                worldManager.AddComponent<Occlusion>();
            worldManager.AddComponent<WorldManager>();
        }

        public static Settings activeDistance = new Settings("activeDistance", "Active Distance", 1, MopSettings.UpdateValues);
        
        // Occlusion
        public static Settings enableObjectOcclusion = new Settings("enableObjectOcclusion", "Object Occlusion", false, MopSettings.UpdateValues);
        public static Settings occlusionDistance = new Settings("occlusionDistance", "Occlusion Distance", 400, MopSettings.UpdateValues);
        public static Settings occlusionSamples = new Settings("occlusionSamples", "Occlusion Samples", 120, MopSettings.UpdateValues);
        public static Settings occlusionSampleDelay = new Settings("occlusionSampleDelay", "Occlusion Sample Delay", 1, MopSettings.UpdateValues);
        public static Settings occlusionHideDelay = new Settings("occlusionHideDelay", "Occlusion Hide Delay", 3, MopSettings.UpdateValues);
        public static Settings minOcclusionDistance = new Settings("minOcclusionDistance", "Minimum Occlusion Distance", 50, MopSettings.UpdateValues);

        /// <summary>
        /// All settings should be created here. 
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
            Settings.AddHeader(this, "Activating Objects", new Color32(29, 29, 29, 255));
            Settings.AddSlider(this, activeDistance, 0, 2);
            Settings.AddText(this, "From how far objects are disabled.\n - 0: Close\n - 1: Normal (Default)\n - 2: Far");

            // Occlusion
            Settings.AddHeader(this, "(EXPERIMENTAL) Occlusion Culling", new Color32(29, 29, 29, 255));
            Settings.AddText(this, "Occlusion Culling at current stage is highly experimental. Graphical glitches may occure, " +
                "or the mod can crash. It is recommended to try that feature on low end GPUs.");
            Settings.AddCheckBox(this, enableObjectOcclusion);
            Settings.AddSlider(this, occlusionDistance, 200, 3000);
            Settings.AddText(this, "How far the program will send rays which check visible objects.\nWARNING: large values" +
                " may cause lag.");
            Settings.AddSlider(this, occlusionSamples, 1, 120);
            Settings.AddText(this, "How many samples of the screen will be checked for visible objects. " +
                "Lower values mean more detail, but may cause significant performance impact.");
            Settings.AddSlider(this, occlusionSampleDelay, 1, 10);
            Settings.AddText(this, "How often the program will objects check if they're visible.");
            Settings.AddSlider(this, occlusionHideDelay, 1, 10);
            Settings.AddText(this, "After how long the objects will hide, when not visible anymore.");
            Settings.AddSlider(this, minOcclusionDistance, 10, 500);
            Settings.AddText(this, "Minimum distance after which the object's visiblity in camera is checked." +
                " If you're closer to the object than this value, the object will not be hidden, even when you not look at it.");

            // Changelog
            Settings.AddHeader(this, "Changelog", new Color32(29, 29, 29, 255));
            Settings.AddText(this, Properties.Resources.changelog);
        }
    }
}
