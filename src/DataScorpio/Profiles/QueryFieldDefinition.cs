namespace DataScorpio.Profiles;

/// <summary>
/// Describes one public query field exposed by a profile.
/// </summary>
public sealed class QueryFieldDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFieldDefinition"/> class.
    /// </summary>
    /// <param name="name">The public field name or alias.</param>
    /// <param name="memberPath">The entity member path.</param>
    /// <param name="fieldType">The field value type.</param>
    /// <param name="capabilities">The allowed query operations.</param>
    public QueryFieldDefinition(
        string name,
        string memberPath,
        Type fieldType,
        QueryFieldCapabilities capabilities)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MemberPath = memberPath ?? throw new ArgumentNullException(nameof(memberPath));
        FieldType = fieldType ?? throw new ArgumentNullException(nameof(fieldType));
        Capabilities = capabilities;
    }

    /// <summary>
    /// Gets the public query field name or alias.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the entity member path.
    /// </summary>
    public string MemberPath { get; }

    /// <summary>
    /// Gets the field value type.
    /// </summary>
    public Type FieldType { get; }

    /// <summary>
    /// Gets the allowed query operations.
    /// </summary>
    public QueryFieldCapabilities Capabilities { get; }

    /// <summary>
    /// Gets a value indicating whether the field can be filtered.
    /// </summary>
    public bool CanFilter => Capabilities.HasFlag(QueryFieldCapabilities.Filter);

    /// <summary>
    /// Gets a value indicating whether the field can be sorted.
    /// </summary>
    public bool CanSort => Capabilities.HasFlag(QueryFieldCapabilities.Sort);

    /// <summary>
    /// Gets a value indicating whether the field can be searched.
    /// </summary>
    public bool CanSearch => Capabilities.HasFlag(QueryFieldCapabilities.Search);

    internal QueryFieldDefinition WithCapabilities(QueryFieldCapabilities capabilities)
        => new(Name, MemberPath, FieldType, Capabilities | capabilities);
}
