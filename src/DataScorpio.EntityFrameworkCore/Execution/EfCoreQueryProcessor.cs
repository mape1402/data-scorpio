namespace DataScorpio.EntityFrameworkCore.Execution;

using DataScorpio.Execution;
using DataScorpio.Parsing;
using DataScorpio.Profiles;
using DataScorpio.Querying;
using DataScorpio.Validation;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Default Entity Framework Core query processor.
/// </summary>
public sealed class EfCoreQueryProcessor : IEfCoreQueryProcessor
{
    private readonly IQueryParser parser;
    private readonly IQueryDescriptorValidator validator;
    private readonly IQueryableQueryApplier applier;
    private readonly IQueryProfileRegistry profiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfCoreQueryProcessor"/> class.
    /// </summary>
    public EfCoreQueryProcessor(
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
    public Task<QueryExecutionResult<TEntity>> ExecuteAsync<TEntity>(
        IQueryable<TEntity> source,
        QueryRequest request,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(source, request, profiles.GetProfile<TEntity>(), cancellationToken);

    /// <inheritdoc/>
    public Task<QueryExecutionResult<TEntity>> ExecuteAsync<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(source, descriptor, profiles.GetProfile<TEntity>(), cancellationToken);

    /// <inheritdoc/>
    public async Task<QueryExecutionResult<TEntity>> ExecuteAsync<TEntity>(
        IQueryable<TEntity> source,
        QueryRequest request,
        QueryProfileDefinition profile,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        var descriptor = parser.Parse(request);
        return await ExecuteAsync(source, descriptor, profile, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<QueryExecutionResult<TEntity>> ExecuteAsync<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfileDefinition profile,
        CancellationToken cancellationToken = default)
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
        var rowCount = await filteredAndSorted.LongCountAsync(cancellationToken);
        var pageCount = CalculatePageCount(rowCount, descriptor.Page.PageSize);
        var pageNumber = CalculatePageNumber(descriptor.Page.PageNumber, pageCount);
        var pageSize = descriptor.Page.PageSize ?? Convert.ToInt32(rowCount);
        var items = await applier.Apply(source, descriptor, profile).ToListAsync(cancellationToken);

        return QueryExecutionResult<TEntity>.Success(new QueryResult<TEntity>
        {
            Items = items,
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
