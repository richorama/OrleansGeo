using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansGeo.GrainInterfaces
{
    public class Position
    {
        public Position(double lat, double lon)
        {
            this.Latitude = lat;
            this.Longitude = lon;
        }

        public Position()
        { }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
