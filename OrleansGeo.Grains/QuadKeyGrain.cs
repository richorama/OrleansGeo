using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using OrleansGeo.GrainInterfaces;
using Orleans.Concurrency;
using Orleans.Providers;

namespace OrleansGeo.Grains
{
    public class QuadKeyGrainState : GrainState
    {
        public Dictionary<string, KeyValuePosition> Members { get; set; }
    }

    [Reentrant]
    [StorageProvider(ProviderName = "Storage")]
    public class QuadKeyGrain : Orleans.Grain<QuadKeyGrainState>, IQuadKeyGrain
    {
        string key = null;

        public override Task OnActivateAsync()
        {
            if (this.State.Members == null) this.State.Members = new Dictionary<string, KeyValuePosition>();
            this.GetPrimaryKey(out key);
            return base.OnActivateAsync();
        }

        public Task AddItem(KeyValuePosition grain)
        {
            if (this.State.Members.ContainsKey(grain.Key))
            {
                this.State.Members[grain.Key] = grain;
            }
            else
            {
                this.State.Members.Add(grain.Key, grain);
            }
            return this.WriteStateAsync();
        }

        public Task RemoveItem(string grain)
        {
            if (!this.State.Members.ContainsKey(grain)) return TaskDone.Done;

            this.State.Members.Remove(grain);
            return this.WriteStateAsync();
        }

        public Task<KeyValuePosition[]> GetItems()
        {
            return Task.FromResult(this.State.Members.Values.ToArray());
        }
    }
}
