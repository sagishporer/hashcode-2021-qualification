using System;

namespace hashcode2021
{
    class GreenLightCycle : ICloneable
    {
        public int Duration;
        public Street Street { get; private set; }

        // Temporary used during simulation
        public bool GreenLightUsed;

        public GreenLightCycle(Street street)
        {
            this.Street = street;
            this.Duration = 0;
        }

        public object Clone()
        {
            GreenLightCycle newCycle = new GreenLightCycle(this.Street);
            newCycle.Duration = this.Duration;

            return newCycle;
        }
    }
}
