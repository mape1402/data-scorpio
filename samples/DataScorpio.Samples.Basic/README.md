# DataScorpio Basic Sample

Run this sample to see DataScorpio execute both supported request styles:

- Sieve-compatible query strings with `filters`, `sorts`, `search`, and paging.
- Native JSON query descriptors parsed into `QueryDescriptor`.

```bash
dotnet run --project samples/DataScorpio.Samples.Basic/DataScorpio.Samples.Basic.csproj
```

The sample uses an in-memory `IQueryable<Customer>` and a typed `QueryProfile<Customer>` so only configured fields can be queried.
