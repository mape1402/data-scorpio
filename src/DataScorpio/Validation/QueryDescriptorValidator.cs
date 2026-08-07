namespace DataScorpio.Validation;

using DataScorpio.Profiles;
using DataScorpio.Querying;

/// <summary>
/// Validates query descriptors using deny-by-default profile metadata.
/// </summary>
public sealed class QueryDescriptorValidator : IQueryDescriptorValidator
{
    /// <inheritdoc/>
    public QueryValidationResult Validate(QueryDescriptor descriptor, QueryProfileDefinition profile)
    {
        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        var errors = new List<QueryValidationError>();

        ValidateFilters(descriptor, profile, errors);
        ValidateSorts(descriptor, profile, errors);
        ValidateSearch(descriptor, profile, errors);
        ValidatePage(descriptor, profile, errors);

        return errors.Count == 0
            ? QueryValidationResult.Success
            : new QueryValidationResult { Errors = errors };
    }

    private static void ValidateFilters(
        QueryDescriptor descriptor,
        QueryProfileDefinition profile,
        ICollection<QueryValidationError> errors)
    {
        foreach (var group in descriptor.FilterGroups)
        {
            foreach (var filter in group.Filters)
            {
                var field = profile.FindField(filter.Field);

                if (field == null)
                {
                    errors.Add(Error(
                        QueryValidationCodes.UnknownField,
                        filter.Field,
                        filter.Operator,
                        filter.RawValue,
                        $"The field '{filter.Field}' is not queryable."));
                    continue;
                }

                if (!field.CanFilter)
                {
                    errors.Add(Error(
                        QueryValidationCodes.FieldNotFilterable,
                        filter.Field,
                        filter.Operator,
                        filter.RawValue,
                        $"The field '{filter.Field}' cannot be filtered."));
                }

                if (string.IsNullOrWhiteSpace(filter.Operator))
                {
                    errors.Add(Error(
                        QueryValidationCodes.MissingOperator,
                        filter.Field,
                        filter.Operator,
                        filter.RawValue,
                        $"The field '{filter.Field}' requires an operator."));
                }
            }
        }
    }

    private static void ValidateSorts(
        QueryDescriptor descriptor,
        QueryProfileDefinition profile,
        ICollection<QueryValidationError> errors)
    {
        foreach (var sort in descriptor.Sorts)
        {
            var field = profile.FindField(sort.Field);

            if (field == null)
            {
                errors.Add(Error(
                    QueryValidationCodes.UnknownField,
                    sort.Field,
                    null,
                    null,
                    $"The field '{sort.Field}' is not queryable."));
                continue;
            }

            if (!field.CanSort)
            {
                errors.Add(Error(
                    QueryValidationCodes.FieldNotSortable,
                    sort.Field,
                    null,
                    null,
                    $"The field '{sort.Field}' cannot be sorted."));
            }
        }
    }

    private static void ValidateSearch(
        QueryDescriptor descriptor,
        QueryProfileDefinition profile,
        ICollection<QueryValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(descriptor.Search.Term))
            return;

        foreach (var fieldName in descriptor.Search.Fields)
        {
            var field = profile.FindField(fieldName);

            if (field == null)
            {
                errors.Add(Error(
                    QueryValidationCodes.UnknownField,
                    fieldName,
                    null,
                    descriptor.Search.Term,
                    $"The field '{fieldName}' is not queryable."));
                continue;
            }

            if (!field.CanSearch)
            {
                errors.Add(Error(
                    QueryValidationCodes.FieldNotSearchable,
                    fieldName,
                    null,
                    descriptor.Search.Term,
                    $"The field '{fieldName}' cannot be searched."));
            }
        }
    }

    private static void ValidatePage(
        QueryDescriptor descriptor,
        QueryProfileDefinition profile,
        ICollection<QueryValidationError> errors)
    {
        if (descriptor.Page.PageNumber.HasValue && descriptor.Page.PageNumber.Value <= 0)
        {
            errors.Add(Error(
                QueryValidationCodes.InvalidPageNumber,
                null,
                null,
                descriptor.Page.PageNumber.Value.ToString(),
                "Page number must be greater than zero."));
        }

        if (descriptor.Page.PageSize.HasValue && descriptor.Page.PageSize.Value <= 0)
        {
            errors.Add(Error(
                QueryValidationCodes.InvalidPageSize,
                null,
                null,
                descriptor.Page.PageSize.Value.ToString(),
                "Page size must be greater than zero."));
            return;
        }

        if (descriptor.Page.PageSize.HasValue &&
            profile.MaxPageSize.HasValue &&
            descriptor.Page.PageSize.Value > profile.MaxPageSize.Value)
        {
            errors.Add(Error(
                QueryValidationCodes.PageSizeExceeded,
                null,
                null,
                descriptor.Page.PageSize.Value.ToString(),
                $"Page size cannot exceed {profile.MaxPageSize.Value}."));
        }
    }

    private static QueryValidationError Error(
        string code,
        string field,
        string @operator,
        string rawValue,
        string message)
        => new()
        {
            Code = code,
            Field = field,
            Operator = @operator,
            RawValue = rawValue,
            Message = message,
            Severity = QueryDiagnosticSeverity.Error
        };
}
