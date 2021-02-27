using System;
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
