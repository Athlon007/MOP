namespace MOP
{
    class MopSettings
    {
        public static bool IsModActive { get; set; }

        public static int ActiveDistance { get; set; }
        public static float ActiveDistanceMultiplicationValue { get; set; }
        
        public static bool SafeMode { get; set; }
        public static bool ToggleVehicles { get; set; }
        public static bool ToggleItems { get; set; }

        public static bool EnableObjectOcclusion { get; set; }
        public static int OcclusionSamples = 120;
        public static int ViewDistance = 400;
        public static int OcclusionSampleDelay = 1;
        public static int OcclusionHideDelay = 3;
        public static int MinOcclusionDistance = 50;
        public static int OcclusionMethod = 1;

        public static void UpdateValues()
        {
            ActiveDistance = int.Parse(MOP.activeDistance.GetValue().ToString());
            ActiveDistanceMultiplicationValue = GetActiveDistanceMultiplicationValue();
            
            SafeMode = (bool)MOP.safeMode.GetValue();
            ToggleVehicles = (bool)MOP.toggleVehicles.GetValue();
            ToggleItems = (bool)MOP.toggleItems.GetValue();

            EnableObjectOcclusion = (bool)MOP.enableObjectOcclusion.GetValue();
            OcclusionSamples = int.Parse(MOP.occlusionSamples.GetValue().ToString());
            ViewDistance = int.Parse(MOP.occlusionDistance.GetValue().ToString());
            OcclusionSampleDelay = int.Parse(MOP.occlusionSampleDelay.GetValue().ToString());
            OcclusionHideDelay = int.Parse(MOP.occlusionHideDelay.GetValue().ToString());
            MinOcclusionDistance = int.Parse(MOP.minOcclusionDistance.GetValue().ToString());

            OcclusionMethod = GetOcclusionMethod();
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

        static int GetOcclusionMethod()
        {
            if ((bool)MOP.occlusionNormal.GetValue())
                return 0;

            if ((bool)MOP.occlusionChequered.GetValue())
                return 1;

            if ((bool)MOP.occlusionDouble.GetValue())
                return 2;

            return 1;
        }
    }
}
