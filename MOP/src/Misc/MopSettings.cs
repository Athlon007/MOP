namespace MOP
{
    public enum OcclusionMethods { Chequered, Double }

    class MopSettings
    {
        // This is the master switch of MOP. If deactivated, all functions will freeze.
        public static bool IsModActive { get; set; }

        //
        // ACTIVATING OBJECTS
        //
        public static int ActiveDistance { get; private set; }
        public static float ActiveDistanceMultiplicationValue { get; private set; }

        public static bool SafeMode { get; set; }

        public static bool ToggleVehicles { get; private set; }

        public static bool ToggleItems { get; private set; }

        //
        // OTHERS
        //
        static bool removeEmptyBeerBottles = false;
        public static bool RemoveEmptyBeerBottles { get => removeEmptyBeerBottles; }

        static bool satsumaTogglePhysicsOnly = false;
        public static bool SatsumaTogglePhysicsOnly { get => satsumaTogglePhysicsOnly; }

        //
        // OCCLUSION CULLING
        //
        public static bool EnableObjectOcclusion { get; set; }

        static int occlusionSamples = 120;
        public static int OcclusionSamples { get => occlusionSamples; }
        
        static int viewDistance = 400;
        public static int ViewDistance { get => viewDistance; }

        static int occlusionSampleDelay = 1;
        public static int OcclusionSampleDelay { get => occlusionSampleDelay; }

        static int minOcclusionDistance = 50;
        public static int MinOcclusionDistance { get => minOcclusionDistance; }

        static OcclusionMethods occlusionMethod = OcclusionMethods.Chequered;
        public static OcclusionMethods OcclusionMethod { get => occlusionMethod; }

        //
        // MISCELLANEOUS
        //

        // OcclusionHideDelay is calculated automatically
        public static int OcclusionHideDelay = -1;

        // Checked after the warning about maximum distance and minimum distance of occlusion culling is showed,
        // so it won't be displayed again.
        static bool occlusionDistanceWarningShowed;

        // Distance after which car physics are toggled
        public const int UnityCarActiveDistance = 5;

        // If true, vehicles physics won't be disabled on small distances, but only on larger ones
        static bool overridePhysicsToggling = false;
        public static bool OverridePhysicsToggling { get => overridePhysicsToggling; }

        public static void UpdateAll()
        {
            // Activating Objects
            ActiveDistance = int.Parse(MOP.activeDistance.GetValue().ToString());
            ActiveDistanceMultiplicationValue = GetActiveDistanceMultiplicationValue();
            SafeMode = (bool)MOP.safeMode.GetValue();
            ToggleVehicles = (bool)MOP.toggleVehicles.GetValue();
            ToggleItems = (bool)MOP.toggleItems.GetValue();

            // Others
            removeEmptyBeerBottles = (bool)MOP.removeEmptyBeerBottles.GetValue();
            satsumaTogglePhysicsOnly = (bool)MOP.satsumaTogglePhysicsOnly.GetValue();
            
            // Occlusion Culling
            if (viewDistance < minOcclusionDistance)
            {
                MOP.occlusionDistance.Value = 400;
                MOP.minOcclusionDistance.Value = 50;

                if (!occlusionDistanceWarningShowed)
                {
                    MSCLoader.ModUI.ShowMessage("Occlusion Distance cannot be lower than Minimum Occlusion Distance." +
                        "\n\nIf you don't change it, both values will be reset to default!", "Error");

                    occlusionDistanceWarningShowed = true;
                }
            }

            EnableObjectOcclusion = (bool)MOP.enableObjectOcclusion.GetValue();
            occlusionSamples = GetOcclusionSamples();
            viewDistance = int.Parse(MOP.occlusionDistance.GetValue().ToString());
            minOcclusionDistance = int.Parse(MOP.minOcclusionDistance.GetValue().ToString());
            occlusionMethod = GetOcclusionMethod();
        }

        /// <summary>
        /// Returns the value that is used to multiplify the active distance of an object.
        /// So for example, if the default active distance of the object is 200 units, 
        /// and the multiplication value is 0.5, the actual active distance will be 100 units.
        /// </summary>
        static float GetActiveDistanceMultiplicationValue()
        {
            switch (ActiveDistance)
            {
                case 0:
                    return 0.5f;
                default: // 1
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 4;
            }
        }

        /// <summary>
        /// Returns the occlusion method used
        /// </summary>
        static OcclusionMethods GetOcclusionMethod()
        {
            return (bool)MOP.occlusionDouble.GetValue() ? OcclusionMethods.Double : OcclusionMethods.Chequered;
        }

        /// <summary>
        /// Returns the the occlusion sampling value
        /// </summary>
        static int GetOcclusionSamples()
        {
            if ((bool)MOP.occlusionSamplesLower.GetValue())
                return 30;

            if ((bool)MOP.occlusionSamplesLow.GetValue())
                return 60;

            if ((bool)MOP.occlusionSamplesVeryDetailed.GetValue())
                return 240;

            return 120;
        }
        
        /// <summary>
        /// Toggles on overridePhysicsToggling.
        /// </summary>
        public static void DisablePhysicsToggling()
        {
            MSCLoader.ModConsole.Print("With dedication to James ;)");
            overridePhysicsToggling = true;
        }

        /// <summary>
        /// Toggles off overridePhysicsToggling
        /// </summary>
        public static void ResetPhysicsToggling()
        {
            overridePhysicsToggling = false;
        }
    }
}
 