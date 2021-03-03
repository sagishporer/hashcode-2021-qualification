using System;
using System.Collections.Generic;
using System.Linq;

namespace hashcode2021
{
    class SimulationResult
    {
        public int Score;
        public List<IntersectionResult> IntersectionResults;
        public List<Car> CarsNotFinished;

        public SimulationResult(int numberOfIntersections)
        {
            this.Score = 0;
            this.IntersectionResults = new List<IntersectionResult>();
            for (int i = 0; i < numberOfIntersections; i++)
                this.IntersectionResults.Add(new IntersectionResult(i));

            this.CarsNotFinished = new List<Car>();
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

            public IntersectionResult(int id)
            {
                this.ID = id;
                this.BlockedTrafficPerStreet = new Dictionary<string, int>();
                this.BlockedTrafficPerStreet.Add("", 0);
                this.MaxStreetBlockedTraffic = 0;
            }

            public void AddBlockedTraffic(string streetName)
            {
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
