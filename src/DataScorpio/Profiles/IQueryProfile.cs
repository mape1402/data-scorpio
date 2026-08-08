namespace DataScorpio.Profiles;

/// <summary>
/// Represents a query profile that can build immutable query metadata.
/// </summary>
public interface IQueryProfile
{
    /// <summary>
    /// Builds the immutable profile definition.
    /// </summary>
    /// <returns>The profile definition.</returns>
    QueryProfileDefinition BuildDefinition();
}

