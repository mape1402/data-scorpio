namespace DataScorpio.Testing.Sqlite;

using DataScorpio.Testing;

/// <summary>
/// Executes DataScorpio tests against a SQLite-backed query provider.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IDataScorpioSqliteTesting<TEntity> : IDataScorpioTesting<TEntity>
    where TEntity : class
{
}

