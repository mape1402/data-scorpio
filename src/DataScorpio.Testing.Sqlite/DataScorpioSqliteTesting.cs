namespace DataScorpio.Testing.Sqlite;

using DataScorpio.Execution;
using DataScorpio.Querying;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

internal sealed class DataScorpioSqliteTesting<TEntity> : IDataScorpioSqliteTesting<TEntity>, IDisposable
    where TEntity : class
{
    private readonly IQueryProcessor processor;
    private readonly SqliteConnection connection;
    private readonly DbContextOptions<DataScorpioSqliteTestingDbContext<TEntity>> options;

    public DataScorpioSqliteTesting(IQueryProcessor processor)
    {
        this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        options = new DbContextOptionsBuilder<DataScorpioSqliteTestingDbContext<TEntity>>()
            .UseSqlite(connection)
            .Options;
    }

    public async Task SeedAsync(IEnumerable<TEntity> items, CancellationToken cancellationToken = default)
    {
        await using var db = CreateDbContext();

        await db.Database.EnsureDeletedAsync(cancellationToken);
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (items != null)
        {
            db.Set<TEntity>().AddRange(items);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public Task<QueryExecutionResult<TEntity>> ApplyAsync(
        string filters = null,
        string sorts = null,
        int? pageNumber = null,
        int? pageSize = null,
        string search = null,
        CancellationToken cancellationToken = default)
    {
        var db = CreateDbContext();
        var query = db.Set<TEntity>().AsNoTracking();

        return ApplyAndDisposeAsync(db, query, filters, sorts, pageNumber, pageSize, search, cancellationToken);
    }

    public Task<QueryExecutionResult<TEntity>> ApplyAsync(
        IQueryable<TEntity> query,
        string filters = null,
        string sorts = null,
        int? pageNumber = null,
        int? pageSize = null,
        string search = null,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        cancellationToken.ThrowIfCancellationRequested();

        var result = processor.Execute(query, new QueryRequest
        {
            Filters = filters,
            Sorts = sorts,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search
        });

        return Task.FromResult(result);
    }

    public void Dispose()
        => connection.Dispose();

    private DataScorpioSqliteTestingDbContext<TEntity> CreateDbContext()
        => new(options);

    private async Task<QueryExecutionResult<TEntity>> ApplyAndDisposeAsync(
        DbContext db,
        IQueryable<TEntity> query,
        string filters,
        string sorts,
        int? pageNumber,
        int? pageSize,
        string search,
        CancellationToken cancellationToken)
    {
        await using (db)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return processor.Execute(query, new QueryRequest
            {
                Filters = filters,
                Sorts = sorts,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search
            });
        }
    }
}

