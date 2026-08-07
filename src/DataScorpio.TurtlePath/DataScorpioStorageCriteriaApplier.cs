namespace DataScorpio.TurtlePath;

using DataScorpio.Execution;
using DataScorpio.Parsing;
using DataScorpio.Profiles;
using DataScorpio.Querying;
using DataScorpio.Validation;
using global::TurtlePath.Domain.Contracts;
using global::TurtlePath.Persistence;

/// <summary>
/// Applies DataScorpio string filters and sorts to TurtlePath storage criteria.
/// </summary>
public sealed class DataScorpioStorageCriteriaApplier : IStorageCriteriaApplier
{
    private readonly IQueryParser parser;
    private readonly IQueryDescriptorValidator validator;
    private readonly IQueryableQueryApplier applier;
    private readonly IQueryProfileRegistry profiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataScorpioStorageCriteriaApplier"/> class.
    /// </summary>
    public DataScorpioStorageCriteriaApplier(
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
    public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, GetManyCriteria<TEntity> criteria)
        where TEntity : class, IEntity
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (criteria == null)
            throw new ArgumentNullException(nameof(criteria));

        if (!criteria.UseFilters() && !criteria.UseSorts())
            return source;

        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = criteria.Filters,
            Sorts = criteria.Sorts
        });

        var profile = profiles.GetProfile<TEntity>();
        var validation = validator.Validate(descriptor, profile);

        if (!validation.IsValid)
            throw new DataScorpioTurtlePathQueryException(validation);

        return applier.Apply(source, descriptor.WithPage(PageDescriptor.Unpaged), profile);
    }
}
