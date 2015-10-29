using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace OrleansGeo.GrainInterfaces
{
    public interface ISearchGrain : IGrainWithIntegerKey
    {
        Task<SearchResult[]> Search(Position position, double radius);
    }
}
