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
    public class ItemGrainState : GrainState
    {
        public string Value { get; set; }
        public string CurrentQuad { get; set; }
        public Position CurrentPosition { get; set; }
    }

    [Reentrant]
    [StorageProvider(ProviderName = "Storage")]
    public class ItemGrain : Grain<ItemGrainState>, IItemGrain
    {
        string key = null;

        public override Task OnActivateAsync()
        {
            this.GetPrimaryKey(out key);
            return base.OnActivateAsync();
        }

        public Task<Tuple<Position, string>> GetPosition()
        {
            return Task.FromResult(new Tuple<Position, string>(this.State.CurrentPosition, this.State.Etag));
        }


        public Task<Tuple<string, string>> GetValue()
        {
            return Task.FromResult(new Tuple<string, string>(this.State.Value, this.State.Etag));
        }

        public Task Delete()
        {
            this.State.Value = null;
            this.DeactivateOnIdle();
            return SetValueAndPosition(null, null, null);
        }


        public async Task<string> SetValueAndPosition(string value, Position newPosition, string eTag)
        {
            if (null != eTag && eTag != this.State.Etag) throw new ArgumentException("eTag");
            if (this.State.CurrentPosition == newPosition) return this.State.Etag;

            var oldPosition = this.State.CurrentPosition;
            this.State.CurrentPosition = newPosition;
            this.State.Value = value;
            await this.WriteStateAsync();

            // think about what to do when failure happens here.
            // is there a way to wrap these calls in a transaction?
            var kvp = new KeyValuePosition
            {
                ETag = this.State.Etag,
                Key = key,
                Position = this.State.CurrentPosition,
                Value = this.State.Value
            };

            var promises = new List<Task>();
            foreach (var item in oldPosition.GetDelta(newPosition))
            {

                var grain = GrainFactory.GetGrain<IQuadKeyGrain>(item.QuadKey);
                switch (item.Delta)
                {
                    case ExtensionMethods.Delta.Join:
                        promises.Add(grain.AddItem(kvp));
                        break;
                    case ExtensionMethods.Delta.Leave:
                        promises.Add(grain.RemoveItem(key));
                        break;
                }
            }
            await Task.WhenAll(promises);
            return this.State.Etag;
        }
    }
}
