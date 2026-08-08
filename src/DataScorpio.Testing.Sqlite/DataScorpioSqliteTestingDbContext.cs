namespace DataScorpio.Testing.Sqlite;

using Microsoft.EntityFrameworkCore;

internal sealed class DataScorpioSqliteTestingDbContext<TEntity> : DbContext
    where TEntity : class
{
    public DataScorpioSqliteTestingDbContext(DbContextOptions<DataScorpioSqliteTestingDbContext<TEntity>> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TEntity>();
    }
}

