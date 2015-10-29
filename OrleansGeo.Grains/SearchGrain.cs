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
    [Reentrant]
    [StatelessWorker]
    public class SearchGrain : Orleans.Grain, ISearchGrain
    {
        public async Task<SearchResult[]> Search(Position position, double radius)
        {
            var tasks = new List<Task<KeyValuePosition[]>>();
            foreach (var key in position.GetQuadKeysInRadius(radius))
            {
                var quadKeyGrain = GrainFactory.GetGrain<IQuadKeyGrain>(key);
                tasks.Add(quadKeyGrain.GetItems());
            }

            await Task.WhenAll(tasks);

            var results = new List<SearchResult>();
            foreach (var item in tasks)
            {
                results.AddRange(item.Result.Select(x =>
                {
                    return new SearchResult
                    {
                        ETag = x.ETag,
                        Key = x.Key,
                        Position = x.Position,
                        Value = x.Value,
                        Distance = x.Position.DistanceTo(position)
                    };
                }));
            }
            return results.Where(x => x.Distance <= radius).ToArray();
        }


    }
}
