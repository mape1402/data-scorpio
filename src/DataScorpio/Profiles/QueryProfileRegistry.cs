namespace DataScorpio.Profiles;

/// <summary>
/// In-memory registry of query profile definitions.
/// </summary>
public sealed class QueryProfileRegistry : IQueryProfileRegistry
{
    private readonly IReadOnlyDictionary<Type, QueryProfileDefinition> profiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryProfileRegistry"/> class.
    /// </summary>
    /// <param name="profiles">The profile definitions.</param>
    public QueryProfileRegistry(IEnumerable<QueryProfileDefinition> profiles)
    {
        if (profiles == null)
            throw new ArgumentNullException(nameof(profiles));

        this.profiles = profiles.ToDictionary(profile => profile.EntityType);
    }

    /// <inheritdoc/>
    public QueryProfileDefinition GetProfile<TEntity>()
    {
        if (TryGetProfile(typeof(TEntity), out var profile))
            return profile;

        throw new InvalidOperationException($"No DataScorpio query profile is registered for '{typeof(TEntity).FullName}'.");
    }

    /// <inheritdoc/>
    public bool TryGetProfile(Type entityType, out QueryProfileDefinition profile)
    {
        if (entityType == null)
            throw new ArgumentNullException(nameof(entityType));

        return profiles.TryGetValue(entityType, out profile);
    }
}
