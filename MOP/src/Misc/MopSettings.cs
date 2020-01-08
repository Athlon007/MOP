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
        public static int ActiveDistance { get; set; }
        public static float ActiveDistanceMultiplicationValue { get; set; }

        public static bool SafeMode { get; set; }
        public static bool ToggleVehicles { get; set; }
        public static bool ToggleItems { get; set; }

        //
        // OTHERS
        //
        static bool removeEmptyBeerBottles = false;
        public static bool RemoveEmptyBeerBottles { get; }

        static bool enableAutoUpdate = true;
        public static bool EnableAutoUpdate { get; }

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

        // OcclusionHideDelay is calculated automatically
        public static int OcclusionHideDelay = -1;

        static int minOcclusionDistance = 50;
        public static int MinOcclusionDistance { get => minOcclusionDistance; }

        static OcclusionMethods occlusionMethod = OcclusionMethods.Chequered;
        public static OcclusionMethods OcclusionMethod { get => occlusionMethod; }

        // others
        static bool warningShowed;
        public const int UnityCarActiveDistance = 5; 

        public static void UpdateAll()
        {
            ActiveDistance = int.Parse(MOP.activeDistance.GetValue().ToString());
            ActiveDistanceMultiplicationValue = GetActiveDistanceMultiplicationValue();
            
            SafeMode = (bool)MOP.safeMode.GetValue();
            ToggleVehicles = (bool)MOP.toggleVehicles.GetValue();
            ToggleItems = (bool)MOP.toggleItems.GetValue();

            removeEmptyBeerBottles = (bool)MOP.removeEmptyBeerBottles.GetValue();

            if (viewDistance < minOcclusionDistance)
            {
                MOP.occlusionDistance.Value = 400;
                MOP.minOcclusionDistance.Value = 50;

                if (!warningShowed)
                {
                    MSCLoader.ModUI.ShowMessage("Occlusion Distance cannot be lower than Minimum Occlusion Distance." +
                        "\n\nIf you don't change it, both values will be reset to default!", "Error");

                    warningShowed = true;
                }
            }

            EnableObjectOcclusion = (bool)MOP.enableObjectOcclusion.GetValue();
            occlusionSamples = GetOcclusionSamples();
            viewDistance = int.Parse(MOP.occlusionDistance.GetValue().ToString());
            minOcclusionDistance = int.Parse(MOP.minOcclusionDistance.GetValue().ToString());

            occlusionMethod = GetOcclusionMethod();
            enableAutoUpdate = (bool)MOP.enableAutoUpdate.GetValue();
        }

        static float GetActiveDistanceMultiplicationValue()
        {
            switch (ActiveDistance)
            {
                default:
                    return 1;
                case 0:
                    return 0.5f;
                case 2:
                    return 2;
                case 3:
                    return 4;
            }
        }

        static OcclusionMethods GetOcclusionMethod()
        {
            if ((bool)MOP.occlusionDouble.GetValue())
                return OcclusionMethods.Double;

            return OcclusionMethods.Chequered;
        }

        static int GetOcclusionSamples()
        {
            if ((bool)MOP.occlusionSamplesLowest.GetValue())
                return 10;

            if ((bool)MOP.occlusionSamplesLower.GetValue())
                return 30;

            if ((bool)MOP.occlusionSamplesLow.GetValue())
                return 60;

            if ((bool)MOP.occlusionSamplesVeryDetailed.GetValue())
                return 240;

            return 120;
        }
    }
}
 