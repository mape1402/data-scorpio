namespace DataScorpio.Execution;

using DataScorpio.Parsing;
using DataScorpio.Profiles;
using DataScorpio.Querying;
using DataScorpio.Validation;

/// <summary>
/// Default synchronous query processor for provider-neutral <see cref="IQueryable{T}"/> sources.
/// </summary>
public sealed class QueryProcessor : IQueryProcessor
{
    private readonly IQueryParser parser;
    private readonly IQueryDescriptorValidator validator;
    private readonly IQueryableQueryApplier applier;
    private readonly IQueryProfileRegistry profiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryProcessor"/> class.
    /// </summary>
    /// <param name="parser">The query parser.</param>
    /// <param name="validator">The descriptor validator.</param>
    /// <param name="applier">The query applier.</param>
    /// <param name="profiles">The profile registry.</param>
    public QueryProcessor(
        IQueryParser parser,
        IQueryDescriptorValidator validator,
        IQueryableQueryApplier applier,
        IQueryProfileRegistry profiles)
    {
        this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
        this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        this.applier = applier ?? throw new ArgumentNullException(nameof(applier));
        this.profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
    }

    /// <inheritdoc/>
    public QueryExecutionResult<TEntity> Execute<TEntity>(
        IQueryable<TEntity> source,
        QueryRequest request)
        => Execute(source, request, profiles.GetProfile<TEntity>());

    /// <inheritdoc/>
    public QueryExecutionResult<TEntity> Execute<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor)
        => Execute(source, descriptor, profiles.GetProfile<TEntity>());

    /// <inheritdoc/>
    public QueryExecutionResult<TEntity> Execute<TEntity>(
        IQueryable<TEntity> source,
        QueryRequest request,
        QueryProfileDefinition profile)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        var descriptor = parser.Parse(request);
        return Execute(source, descriptor, profile);
    }

    /// <inheritdoc/>
    public QueryExecutionResult<TEntity> Execute<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfileDefinition profile)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        var validation = validator.Validate(descriptor, profile);

        if (!validation.IsValid)
            return QueryExecutionResult<TEntity>.Rejected(validation);

        var unpagedDescriptor = descriptor.WithPage(PageDescriptor.Unpaged);
        var filteredAndSorted = applier.Apply(source, unpagedDescriptor, profile);
        var rowCount = filteredAndSorted.LongCount();
        var pageCount = CalculatePageCount(rowCount, descriptor.Page.PageSize);
        var pageNumber = CalculatePageNumber(descriptor.Page.PageNumber, pageCount);
        var pageSize = descriptor.Page.PageSize ?? Convert.ToInt32(rowCount);
        var paged = applier.Apply(source, descriptor, profile).ToArray();

        return QueryExecutionResult<TEntity>.Success(new QueryResult<TEntity>
        {
            Items = paged,
            PageNumber = pageNumber,
            PageSize = pageSize,
            RowCount = rowCount,
            PageCount = pageCount
        });
    }

    private static int CalculatePageCount(long rowCount, int? pageSize)
    {
        if (!pageSize.HasValue || pageSize.Value <= 0)
            return rowCount == 0 ? 0 : 1;

        return Convert.ToInt32(rowCount / pageSize.Value + (rowCount % pageSize.Value > 0 ? 1 : 0));
    }

    private static int CalculatePageNumber(int? requestedPageNumber, int pageCount)
    {
        if (!requestedPageNumber.HasValue)
            return pageCount == 0 ? 0 : 1;

        if (pageCount == 0)
            return 0;

        return requestedPageNumber.Value > pageCount
            ? pageCount
            : requestedPageNumber.Value;
    }
}
