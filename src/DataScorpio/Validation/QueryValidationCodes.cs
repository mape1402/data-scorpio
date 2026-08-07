namespace DataScorpio.Validation;

/// <summary>
/// Stable query validation diagnostic codes.
/// </summary>
public static class QueryValidationCodes
{
    /// <summary>
    /// The requested field is not configured.
    /// </summary>
    public const string UnknownField = "query.unknown_field";

    /// <summary>
    /// The requested field cannot be filtered.
    /// </summary>
    public const string FieldNotFilterable = "query.field_not_filterable";

    /// <summary>
    /// The requested field cannot be sorted.
    /// </summary>
    public const string FieldNotSortable = "query.field_not_sortable";

    /// <summary>
    /// The requested field cannot be searched.
    /// </summary>
    public const string FieldNotSearchable = "query.field_not_searchable";

    /// <summary>
    /// The requested include is not configured.
    /// </summary>
    public const string UnknownInclude = "query.unknown_include";

    /// <summary>
    /// The requested operator is missing.
    /// </summary>
    public const string MissingOperator = "query.missing_operator";

    /// <summary>
    /// The requested page number is invalid.
    /// </summary>
    public const string InvalidPageNumber = "query.invalid_page_number";

    /// <summary>
    /// The requested page size is invalid.
    /// </summary>
    public const string InvalidPageSize = "query.invalid_page_size";

    /// <summary>
    /// The requested page size exceeds the configured maximum.
    /// </summary>
    public const string PageSizeExceeded = "query.page_size_exceeded";
}
