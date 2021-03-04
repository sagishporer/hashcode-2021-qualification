using System;
using System.Collections.Generic;
using System.Linq;
using static hashcode2021.Problem;

namespace hashcode2021
{
    class SimulationResult
    {
        public int Score;
        public List<IntersectionResult> IntersectionResults;
        public List<CarSimultionPosition> CarsNotFinished;

        public SimulationResult(int numberOfIntersections)
        {
            this.Score = 0;
            this.IntersectionResults = new List<IntersectionResult>();
            for (int i = 0; i < numberOfIntersections; i++)
                this.IntersectionResults.Add(new IntersectionResult(i));

            this.CarsNotFinished = new List<CarSimultionPosition>();
        }

        public int GetMaxBlockedTraffic()
        {
            return this.IntersectionResults.Max(o => o.BlockedTrafficPerStreet.Max(s => s.Value));
        }

        public class IntersectionResult
        {
            public int ID;
            public Dictionary<string, int> BlockedTrafficPerStreet;
            public String MaxStreetName;
            public int MaxStreetBlockedTraffic;
            public int TotalBlockedTraffic;

            public IntersectionResult(int id)
            {
                this.ID = id;
                this.BlockedTrafficPerStreet = new Dictionary<string, int>();
                this.BlockedTrafficPerStreet.Add("", 0);
                this.MaxStreetBlockedTraffic = 0;
                this.TotalBlockedTraffic = 0;
            }

            public void AddBlockedTraffic(string streetName)
            {
                this.TotalBlockedTraffic++;

                int value;
                if (!BlockedTrafficPerStreet.TryGetValue(streetName, out value))
                    value = 1;
                else
                    value++;

                BlockedTrafficPerStreet[streetName] = value;

                if (value > MaxStreetBlockedTraffic)
                {
                    MaxStreetBlockedTraffic = value;
                    MaxStreetName = streetName;
                }
            }
        }
    }
}
