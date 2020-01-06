namespace MOP
{
    class Fury : Vehicle
    {
        // Fury class - made by Konrad "Athlon" Figura
        // 
        // Adds support for disabling and enabling car from Drivable Fury mod.

        /// <summary>
        /// Initialize class
        /// </summary>
        /// <param name="gameObject"></param>
        public Fury(string gameObject) : base(gameObject)
        {
            furyScript = this;
            Toggle = ToggleUnityCar;
        }
    }
}
