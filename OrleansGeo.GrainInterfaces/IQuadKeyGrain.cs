using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace OrleansGeo.GrainInterfaces
{
    public interface IQuadKeyGrain : IGrainWithStringKey
    {
        Task AddItem(KeyValuePosition grain);

        Task RemoveItem(string grain);

        Task<KeyValuePosition[]> GetItems();
    }
}
