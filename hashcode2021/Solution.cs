using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace hashcode2021
{
    class Solution : ICloneable
    {
        public SolutionIntersection[] Intersections { get; private set; }

        public Solution(int numberOfIntersections)
        {
            this.Intersections = new SolutionIntersection[numberOfIntersections];
            for (int i = 0; i < numberOfIntersections; i++)
                this.Intersections[i] = new SolutionIntersection(i);
        }

        public Solution(Solution other)
        {
            this.Intersections = new SolutionIntersection[other.Intersections.Length];
            for (int i = 0; i < this.Intersections.Length; i++)
                this.Intersections[i] = (SolutionIntersection)other.Intersections[i].Clone();
        }

        public static Solution LoadSolution(string fileName, Problem problem)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                int maxIntersection = -1;
                int numberOfIntersectionsWithGreenLight = int.Parse(sr.ReadLine());
                Dictionary<int, SolutionIntersection> intersections = new Dictionary<int, SolutionIntersection>();
                for (int i = 0; i < numberOfIntersectionsWithGreenLight; i++)
                {
                    int intersectionId = int.Parse(sr.ReadLine());
                    maxIntersection = Math.Max(intersectionId, maxIntersection);

                    int numberOfStreets = int.Parse(sr.ReadLine());
                    SolutionIntersection solutionIntersection = new SolutionIntersection(intersectionId);
                    solutionIntersection.GreenLigths = new List<GreenLightCycle>();
                    for (int s = 0; s < numberOfStreets; s++)
                    {
                        string[] parts = sr.ReadLine().Split(" ");
                        string streetName = parts[0];
                        int duration = int.Parse(parts[1]);
                        Street street = problem.Streets[streetName];

                        GreenLightCycle greenLightCycle = new GreenLightCycle(street);
                        greenLightCycle.Duration = duration;
                        solutionIntersection.GreenLigths.Add(greenLightCycle);
                    }

                    intersections.Add(intersectionId, solutionIntersection);
                }

                Solution solution = new Solution(maxIntersection + 1);
                for (int i = 0; i < maxIntersection + 1; i++)
                {
                    if (intersections.ContainsKey(i))
                        solution.Intersections[i] = intersections[i];
                    else
                        solution.Intersections[i].GreenLigths = new List<GreenLightCycle>();
                }

                return solution;
            }
        }

        public int CountIntersectionsWithGreenLights()
        {
            return this.Intersections.Sum(o => o.HasGreenLights() ? 1 : 0);
        }

        public object Clone()
        {
            return new Solution(this);            
        }
    }
}
