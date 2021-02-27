using System;
using System.Collections.Generic;
using System.Linq;

namespace hashcode2021
{
    class SolutionIntersection : ICloneable
    {
        public int ID { get; private set; }
        public List<GreenLightCycle> GreenLigths;

        // Temporary used during simulation
        public int CurrentGreenLigth;
        public int CurrentGreenLightChangeTime;

        public SolutionIntersection(int id)
        {
            this.ID = id;
        }

        public int CountGreenLights()
        {
            if (GreenLigths == null)
                return 0;

            return GreenLigths.Sum(o => (o.Duration > 0) ? 1 : 0);
        }

        public bool HasGreenLights()
        {
            return CountGreenLights() > 0;
        }

        public object Clone()
        {
            SolutionIntersection solutionIntersection = new SolutionIntersection(this.ID);
            solutionIntersection.GreenLigths = new List<GreenLightCycle>();
            foreach (GreenLightCycle cycle in this.GreenLigths)
                solutionIntersection.GreenLigths.Add((GreenLightCycle)cycle.Clone());

            return solutionIntersection;
        }
    }
}
