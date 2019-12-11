namespace MOP
{
    class MopSettings
    {
        public static int ActiveDistance { get; set; }
        public static float ActiveDistanceMultiplicationValue;

        public static bool EnableObjectOcclusion { get; set; }

        public static int OcclusionSamples = 120;
        public static int ViewDistance = 400;
        public static float OcclusionSampleDelay = 1;
        public static int OcclusionHideDelay = 3;
        public static int MinOcclusionDistance = 50;

        public static void UpdateValues()
        {
            ActiveDistance = int.Parse(MOP.activeDistance.GetValue().ToString());
            ActiveDistanceMultiplicationValue = GetActiveDistanceMultiplicationValue();

            EnableObjectOcclusion = (bool)MOP.enableObjectOcclusion.GetValue();
            OcclusionSamples = (int)MOP.occlusionSamples.GetValue();
            ViewDistance = (int)MOP.occlusionDistance.GetValue();
            OcclusionSampleDelay = (float)MOP.occlusionSampleDelay.GetValue();
            OcclusionHideDelay = (int)MOP.occlusionHideDelay.GetValue();
            MinOcclusionDistance = (int)MOP.minOcclusionDistance.GetValue();
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
