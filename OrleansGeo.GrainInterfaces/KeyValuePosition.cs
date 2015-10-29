using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansGeo.GrainInterfaces
{
    public class KeyValuePosition
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public string ETag { get; set; }

        public Position Position { get; set; }
    }
}
