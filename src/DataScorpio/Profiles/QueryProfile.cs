namespace DataScorpio.Profiles;

/// <summary>
/// Base class for typed query profiles.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public abstract class QueryProfile<TEntity> : IQueryProfile
{
    /// <summary>
    /// Configures the query profile.
    /// </summary>
    /// <param name="builder">The profile builder.</param>
    public abstract void Configure(IQueryProfileBuilder<TEntity> builder);

    /// <summary>
    /// Builds the immutable profile definition.
    /// </summary>
    /// <returns>The profile definition.</returns>
    public QueryProfileDefinition BuildDefinition()
    {
        var builder = new QueryProfileBuilder<TEntity>();
        Configure(builder);

        return builder.Build();
    }
}
