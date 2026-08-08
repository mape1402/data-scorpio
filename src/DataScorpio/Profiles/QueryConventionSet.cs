namespace DataScorpio.Profiles;

/// <summary>
/// Base class for reusable query conventions.
/// </summary>
public abstract class QueryConventionSet
{
    /// <summary>
    /// Configures reusable query conventions.
    /// </summary>
    /// <param name="builder">The convention builder.</param>
    public abstract void Configure(IQueryConventionBuilder builder);

    internal IReadOnlyList<IQueryProfileConvention> BuildConventions()
    {
        var builder = new QueryConventionBuilder();
        Configure(builder);

        return builder.Build();
    }
}

