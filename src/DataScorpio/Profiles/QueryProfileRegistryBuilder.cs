namespace DataScorpio.Profiles;

/// <summary>
/// Builds a query profile registry.
/// </summary>
public sealed class QueryProfileRegistryBuilder
{
    private readonly List<QueryProfileDefinition> profiles = [];

    /// <summary>
    /// Adds a profile by type.
    /// </summary>
    /// <typeparam name="TProfile">The profile type.</typeparam>
    /// <returns>The same builder.</returns>
    public QueryProfileRegistryBuilder AddProfile<TProfile>()
        where TProfile : IQueryProfile, new()
        => AddProfile(new TProfile().BuildDefinition());

    /// <summary>
    /// Adds a profile instance.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="profile">The profile instance.</param>
    /// <returns>The same builder.</returns>
    public QueryProfileRegistryBuilder AddProfile<TEntity>(QueryProfile<TEntity> profile)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        profiles.Add(profile.BuildDefinition());

        return this;
    }

    /// <summary>
    /// Adds a profile definition.
    /// </summary>
    /// <param name="profile">The profile definition.</param>
    /// <returns>The same builder.</returns>
    public QueryProfileRegistryBuilder AddProfile(QueryProfileDefinition profile)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        profiles.Add(profile);

        return this;
    }

    /// <summary>
    /// Builds the registry.
    /// </summary>
    /// <returns>The profile registry.</returns>
    public QueryProfileRegistry Build()
        => new(profiles);
}
