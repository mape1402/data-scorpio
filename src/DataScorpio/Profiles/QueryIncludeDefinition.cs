namespace DataScorpio.Profiles;

/// <summary>
/// Describes one include path exposed by a profile.
/// </summary>
public sealed class QueryIncludeDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryIncludeDefinition"/> class.
    /// </summary>
    /// <param name="name">The public include name or alias.</param>
    /// <param name="memberPath">The entity member path.</param>
    public QueryIncludeDefinition(string name, string memberPath)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MemberPath = memberPath ?? throw new ArgumentNullException(nameof(memberPath));
    }

    /// <summary>
    /// Gets the public include name or alias.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the entity member path.
    /// </summary>
    public string MemberPath { get; }
}
