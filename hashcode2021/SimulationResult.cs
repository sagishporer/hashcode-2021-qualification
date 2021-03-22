using System;
using System.Collections.Generic;
using System.Linq;
using static hashcode2021.Problem;

namespace hashcode2021
{
    class SimulationResult
    {
        public int Score;
        public IntersectionResult[] IntersectionResults;
        public List<CarSimultionPosition> CarsNotFinished;

        public SimulationResult(int numberOfIntersections)
        {
            this.Score = 0;
            this.IntersectionResults = new IntersectionResult[numberOfIntersections];
            for (int i = 0; i < numberOfIntersections; i++)
                this.IntersectionResults[i] = new IntersectionResult(i);

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

            public Dictionary<int, int> MaxWaitOnGreenLightByStreet;
            public int MaxWaitOnGreenLight;
            public int MaxWaitOnGreenLightStreetID;

            public IntersectionResult(int id)
            {
                this.ID = id;
                this.BlockedTrafficPerStreet = new Dictionary<string, int>();
                this.BlockedTrafficPerStreet.Add("", 0);
                this.MaxStreetBlockedTraffic = 0;
                this.TotalBlockedTraffic = 0;

                this.MaxWaitOnGreenLight = 0;
                this.MaxWaitOnGreenLightByStreet = new Dictionary<int, int>();
            }

            public void UpdateMaxWaitOnGreenLight(int streetId, int waitOnGreenLight)
            {
                if (this.MaxWaitOnGreenLight < waitOnGreenLight)
                {
                    this.MaxWaitOnGreenLight = waitOnGreenLight;
                    this.MaxWaitOnGreenLightStreetID = streetId;
                }

                int maxWait;
                if (MaxWaitOnGreenLightByStreet.TryGetValue(streetId, out maxWait))
                    MaxWaitOnGreenLightByStreet[streetId] = Math.Max(maxWait, waitOnGreenLight);
                else
                    MaxWaitOnGreenLightByStreet.Add(streetId, waitOnGreenLight);
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
