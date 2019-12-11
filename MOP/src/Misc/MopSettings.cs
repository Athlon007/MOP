namespace MOP
{
    class MopSettings
    {
        public static int ActiveDistance { get; set; }
        public static float ActiveDistanceMultiplicationValue;

        public static bool EnableObjectOcclusion { get; set; }

        public static int OcclusionSamples = 120;
        public static int ViewDistance = 400;
        public static int OcclusionSampleDelay = 1;
        public static int OcclusionHideDelay = 3;
        public static int MinOcclusionDistance = 50;

        public static void UpdateValues()
        {
            ActiveDistance = int.Parse(MOP.activeDistance.GetValue().ToString());
            ActiveDistanceMultiplicationValue = GetActiveDistanceMultiplicationValue();

            EnableObjectOcclusion = (bool)MOP.enableObjectOcclusion.GetValue();
            OcclusionSamples = int.Parse(MOP.occlusionSamples.GetValue().ToString());
            ViewDistance = int.Parse(MOP.occlusionDistance.GetValue().ToString());
            OcclusionSampleDelay = int.Parse(MOP.occlusionSampleDelay.GetValue().ToString());
            OcclusionHideDelay = int.Parse(MOP.occlusionHideDelay.GetValue().ToString());
            MinOcclusionDistance = int.Parse(MOP.minOcclusionDistance.GetValue().ToString());
        }

        static float GetActiveDistanceMultiplicationValue()
        {
            switch (ActiveDistance)
            {
                default:
                    return 1f;
                case 0:
                    return 0.5f;
                case 2:
                    return 2f;
            }
        }
    }
}
