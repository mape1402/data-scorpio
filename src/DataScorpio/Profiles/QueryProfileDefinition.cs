namespace DataScorpio.Profiles;

/// <summary>
/// Describes the query metadata produced by one typed profile.
/// </summary>
public sealed class QueryProfileDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryProfileDefinition"/> class.
    /// </summary>
    /// <param name="entityType">The configured entity type.</param>
    /// <param name="fields">The configured fields.</param>
    /// <param name="includes">The configured includes.</param>
    /// <param name="defaultSort">The default sort.</param>
    /// <param name="maxPageSize">The maximum page size.</param>
    public QueryProfileDefinition(
        Type entityType,
        IReadOnlyDictionary<string, QueryFieldDefinition> fields,
        IReadOnlyDictionary<string, QueryIncludeDefinition> includes,
        QuerySortDefinition defaultSort,
        int? maxPageSize)
    {
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        Fields = fields ?? throw new ArgumentNullException(nameof(fields));
        Includes = includes ?? throw new ArgumentNullException(nameof(includes));
        DefaultSort = defaultSort;
        MaxPageSize = maxPageSize;
    }

    /// <summary>
    /// Gets the configured entity type.
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    /// Gets configured fields by public name or alias.
    /// </summary>
    public IReadOnlyDictionary<string, QueryFieldDefinition> Fields { get; }

    /// <summary>
    /// Gets configured includes by public name or alias.
    /// </summary>
    public IReadOnlyDictionary<string, QueryIncludeDefinition> Includes { get; }

    /// <summary>
    /// Gets the default sort.
    /// </summary>
    public QuerySortDefinition DefaultSort { get; }

    /// <summary>
    /// Gets the maximum page size.
    /// </summary>
    public int? MaxPageSize { get; }

    /// <summary>
    /// Finds a configured field.
    /// </summary>
    /// <param name="name">The field name or alias.</param>
    /// <returns>The field definition if it exists; otherwise, null.</returns>
    public QueryFieldDefinition FindField(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return Fields.TryGetValue(name, out var field)
            ? field
            : null;
    }

    /// <summary>
    /// Finds a configured include.
    /// </summary>
    /// <param name="name">The include name or alias.</param>
    /// <returns>The include definition if it exists; otherwise, null.</returns>
    public QueryIncludeDefinition FindInclude(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return Includes.TryGetValue(name, out var include)
            ? include
            : null;
    }
}
