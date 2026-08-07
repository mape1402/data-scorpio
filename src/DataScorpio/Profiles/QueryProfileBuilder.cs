namespace DataScorpio.Profiles;

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using DataScorpio.Querying;

/// <summary>
/// Builds typed query profile metadata.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public sealed class QueryProfileBuilder<TEntity> : IQueryProfileBuilder<TEntity>
{
    private readonly Dictionary<string, QueryFieldDefinition> fields = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, QueryIncludeDefinition> includes = new(StringComparer.OrdinalIgnoreCase);
    private QuerySortDefinition defaultSort;
    private int? maxPageSize;

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowFilter(Expression<Func<TEntity, object>> field)
        => AllowFilter(GetDefaultName(field), field);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowFilter(string name, Expression<Func<TEntity, object>> field)
        => Allow(name, field, QueryFieldCapabilities.Filter);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowSort(Expression<Func<TEntity, object>> field)
        => AllowSort(GetDefaultName(field), field);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowSort(string name, Expression<Func<TEntity, object>> field)
        => Allow(name, field, QueryFieldCapabilities.Sort);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowSearch(Expression<Func<TEntity, object>> field)
        => AllowSearch(GetDefaultName(field), field);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowSearch(string name, Expression<Func<TEntity, object>> field)
        => Allow(name, field, QueryFieldCapabilities.Search);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowInclude(Expression<Func<TEntity, object>> include)
        => AllowInclude(GetDefaultName(include), include);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowInclude(string name, Expression<Func<TEntity, object>> include)
    {
        if (include == null)
            throw new ArgumentNullException(nameof(include));

        var normalizedName = NormalizeName(name);
        var member = QueryMemberPath.From(include);

        if (includes.TryGetValue(normalizedName, out var existing) &&
            !string.Equals(existing.MemberPath, member.Path, StringComparison.Ordinal))
            throw new InvalidOperationException($"Query include '{normalizedName}' is already mapped to '{existing.MemberPath}'.");

        includes[normalizedName] = new QueryIncludeDefinition(normalizedName, member.Path);

        return this;
    }

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> DefaultSort(
        Expression<Func<TEntity, object>> field,
        SortDirection direction = SortDirection.Ascending)
        => DefaultSort(GetDefaultName(field), field, direction);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> DefaultSort(
        string name,
        Expression<Func<TEntity, object>> field,
        SortDirection direction = SortDirection.Ascending)
    {
        AllowSort(name, field);
        defaultSort = new QuerySortDefinition(NormalizeName(name), direction);

        return this;
    }

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> MaxPageSize(int maxPageSize)
    {
        if (maxPageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPageSize), "Max page size must be greater than zero.");

        this.maxPageSize = maxPageSize;

        return this;
    }

    /// <summary>
    /// Builds the immutable profile definition.
    /// </summary>
    /// <returns>The profile definition.</returns>
    public QueryProfileDefinition Build()
        => new(
            typeof(TEntity),
            new ReadOnlyDictionary<string, QueryFieldDefinition>(new Dictionary<string, QueryFieldDefinition>(fields, fields.Comparer)),
            new ReadOnlyDictionary<string, QueryIncludeDefinition>(new Dictionary<string, QueryIncludeDefinition>(includes, includes.Comparer)),
            defaultSort,
            maxPageSize);

    private IQueryProfileBuilder<TEntity> Allow(
        string name,
        Expression<Func<TEntity, object>> field,
        QueryFieldCapabilities capabilities)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var normalizedName = NormalizeName(name);
        var member = QueryMemberPath.From(field);

        if (fields.TryGetValue(normalizedName, out var existing))
        {
            if (!string.Equals(existing.MemberPath, member.Path, StringComparison.Ordinal) || existing.FieldType != member.Type)
                throw new InvalidOperationException($"Query field '{normalizedName}' is already mapped to '{existing.MemberPath}'.");

            fields[normalizedName] = existing.WithCapabilities(capabilities);
            return this;
        }

        fields[normalizedName] = new QueryFieldDefinition(
            normalizedName,
            member.Path,
            member.Type,
            capabilities);

        return this;
    }

    private static string GetDefaultName(Expression<Func<TEntity, object>> field)
        => QueryMemberPath.From(field).LeafName;

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Query field name cannot be empty.", nameof(name));

        return name.Trim();
    }
}
