using System.Collections.Generic;
using System.Linq;

namespace hashcode2021
{
    class Car
    {
        public List<Street> Streets { get; private set; }

        public Car(List<Street> streets)
        {
            this.Streets = streets;
        }

        public int TimeNeedToDrive()
        {
            return this.Streets.Sum(o => o.Length);
        }
    }
}
