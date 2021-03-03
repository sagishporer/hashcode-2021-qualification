using System;
using System.Collections.Generic;
using System.Text;

namespace hashcode2021
{
    class Street
    {
        public int UniqueID { get; private set; }
        public int StartIntersection { get; private set; }
        public int EndIntersection { get; private set; }
        public string Name { get; private set; }
        public int Length { get; private set; }

        public int IncomingUsageCount;
        public int CarsOnStart;

        public Street(int uniqueId, int startIntersection, int endIntersection, string name, int length)
        {
            this.UniqueID = uniqueId;
            this.StartIntersection = startIntersection;
            this.EndIntersection = endIntersection;
            this.Name = name;
            this.Length = length;
            this.IncomingUsageCount = 0;
            this.CarsOnStart = 0;
        }
    }
}
