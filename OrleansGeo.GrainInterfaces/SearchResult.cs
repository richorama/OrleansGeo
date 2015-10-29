using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansGeo.GrainInterfaces
{
    public class SearchResult
    {
        public double Distance { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public Position Position { get; set; }

        public string ETag { get; set; }

    }
}
