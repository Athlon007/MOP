namespace MOP
{
    class MopSettings
    {
        public static int ActiveDistance { get; set; }
        public static float ActiveDistanceMultiplicationValue;

        public static void UpdateValues()
        {
            ActiveDistance = int.Parse(MOP.activeDistance.GetValue().ToString());
            ActiveDistanceMultiplicationValue = GetActiveDistanceMultiplicationValue();
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
