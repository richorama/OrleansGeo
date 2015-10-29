using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace OrleansGeo.GrainInterfaces
{
    public interface IItemGrain : IGrainWithStringKey
    {
        Task<Tuple<Position, string>> GetPosition();

        Task<string> SetValueAndPosition(string value, Position position, string eTag);

        Task<Tuple<string, string>> GetValue();

        Task Delete();

    }
}
