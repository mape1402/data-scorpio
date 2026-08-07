namespace DataScorpio.Profiles;

/// <summary>
/// Resolves query profile definitions by entity type.
/// </summary>
public interface IQueryProfileRegistry
{
    /// <summary>
    /// Gets the profile definition for an entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The profile definition.</returns>
    QueryProfileDefinition GetProfile<TEntity>();

    /// <summary>
    /// Tries to get the profile definition for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="profile">The resolved profile definition.</param>
    /// <returns>True when a profile exists; otherwise, false.</returns>
    bool TryGetProfile(Type entityType, out QueryProfileDefinition profile);
}
