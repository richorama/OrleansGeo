# OrleansGeo

A prototype geospatial index in Orleans.

## Usage

The index stores a key/value/position triple.

```cs
// store a key, value and position

var grain = GrainClient.GrainFactory.GetGrain<IItemGrain>("MY_KEY");
await grain.SetValueAndPosition("MY_VALUE", new Position(52.0999542, 1.0969174), null);

// search for keys and values within a radius

var search = GrainClient.GrainFactory.GetGrain<ISearchGrain>(0);
var results = await search.Search(new Position(52.0999542, 1.0969222), 1000);

// results is an array of all key/value/position triples within a 1000m radius

results[0].Key 		// "MY_KEY"
results[0].Value 	// "MY_VALUE"
results[0].Position // 52.0999542, 1.0969174
results[0].Distance // distance from the search point
```

## How it works

When the item grain is used to store a value and position, it also updates a hierarchy 
of grains organised in a [quad tree](https://en.wikipedia.org/wiki/Quadtree). Each grain
in the quad tree keeps track of the items contained under it.

When a position is updated, the grain informs the grains in the quad tree hierarchy, but only walks up the hierarchy as far as needed  (sometimes a change in position will only ripple a small way up the hierarchy, if at all).

When the search grain is called, it calculates an appropriate level in the quad tree hierarchy, based on the supplied radius, to build a list of all grains which could be within the radius. It then interrogates all of these grains to get their values and positions, and returns these as the results.

## Problems

Moving a position involved updating several grains. There is no transaction capability in Orleans, and failure to contact these grains could result in corrupt results.

It's unclear how this would scale (if at all).

## License

MIT
