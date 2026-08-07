namespace DataScorpio.Profiles;

using DataScorpio.Querying;

/// <summary>
/// Describes the default sort configured for a profile.
/// </summary>
public sealed class QuerySortDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySortDefinition"/> class.
    /// </summary>
    /// <param name="fieldName">The public field name or alias.</param>
    /// <param name="direction">The sort direction.</param>
    public QuerySortDefinition(string fieldName, SortDirection direction)
    {
        FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
        Direction = direction;
    }

    /// <summary>
    /// Gets the public field name or alias.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// Gets the sort direction.
    /// </summary>
    public SortDirection Direction { get; }
}
