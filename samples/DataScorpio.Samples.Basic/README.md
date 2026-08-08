# DataScorpio Basic Sample

Run this sample to see DataScorpio execute common querying cases through the normal DI-registered processor:

- Sieve-compatible query strings with filters, sorts, search, and paging.
- Sieve-compatible OR filters.
- Optional public aliases, such as `customerName`.
- Contract-based custom filters and sorts registered once with `AddConventions<T>()`.
- Null filtering with `DeletedAt==null`.
- Native JSON query descriptors parsed into `QueryDescriptor`.
- Validation failures for unknown fields and page size limits.

```bash
dotnet run --project samples/DataScorpio.Samples.Basic/DataScorpio.Samples.Basic.csproj
```

The sample uses an in-memory `IQueryable<Customer>`. Classes are split into `Domain`, `Contracts`, `Data`, `Querying`, `Scenarios`, and `Output` folders.
