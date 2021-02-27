using System.Collections.Generic;

namespace hashcode2021
{
    class Intersection
    {
        public int ID { get; private set; }
        public List<Street> IncomingStreets { get; private set; }
        public List<Street> OutgoingStreets { get; private set; }

        public Intersection(int id, List<Street> incomingStreets, List<Street> outgoingStreets)
        {
            this.ID = id;
            this.IncomingStreets = incomingStreets;
            this.OutgoingStreets = outgoingStreets;
        }
    }
}
