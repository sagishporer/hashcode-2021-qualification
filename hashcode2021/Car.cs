using System.Collections.Generic;
using System.Linq;

namespace hashcode2021
{
    class Car
    {
        public int UniqueID { get; private set; }

        public List<Street> Streets { get; private set; }

        public Car(int uniqueId, List<Street> streets)
        {
            this.UniqueID = uniqueId;
            this.Streets = streets;
        }

        public int TimeNeedToDrive()
        {
            int time = 0;
            for (int i = 1; i < this.Streets.Count; i++)
                time += this.Streets[i].Length;

            return time;
        }
    }
}
