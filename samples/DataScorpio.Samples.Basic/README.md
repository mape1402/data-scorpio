# DataScorpio Basic Sample

Run this sample to see DataScorpio execute both supported request styles through the normal DI-registered processor:

- Sieve-compatible query strings with `filters`, `sorts`, `search`, and paging.
- Native JSON query descriptors parsed into `QueryDescriptor`.

```bash
dotnet run --project samples/DataScorpio.Samples.Basic/DataScorpio.Samples.Basic.csproj
```

The sample uses an in-memory `IQueryable<Customer>` and registers `CustomerQueryProfile` once with `profiles.AddProfile<CustomerQueryProfile>()`.
