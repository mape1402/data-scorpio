namespace DataScorpio.Parsing.Sieve;

using System.Text;
using DataScorpio.Parsing;
using DataScorpio.Querying;

/// <summary>
/// Parses Sieve-compatible filter and sort strings into DataScorpio descriptors.
/// </summary>
public sealed class SieveQueryParser : IQueryParser
{
    private static readonly IReadOnlyList<(string Token, string Name)> Operators =
    [
        ("!_-=*", SieveOperatorNames.NotEndsWithInsensitive),
        ("!@=*", SieveOperatorNames.NotContainsInsensitive),
        ("!_=*", SieveOperatorNames.NotStartsWithInsensitive),
        ("_-=*", SieveOperatorNames.EndsWithInsensitive),
        ("@=*", SieveOperatorNames.ContainsInsensitive),
        ("_=*", SieveOperatorNames.StartsWithInsensitive),
        ("==*", SieveOperatorNames.EqualInsensitive),
        ("!=*", SieveOperatorNames.NotEqualsInsensitive),
        ("!_-=", SieveOperatorNames.NotEndsWith),
        ("!@=", SieveOperatorNames.NotContains),
        ("!_=", SieveOperatorNames.NotStartsWith),
        (">=", SieveOperatorNames.GreaterThanOrEqual),
        ("<=", SieveOperatorNames.LessThanOrEqual),
        ("_=", SieveOperatorNames.StartsWith),
        ("_-=", SieveOperatorNames.EndsWith),
        ("@=", SieveOperatorNames.Contains),
        ("==", SieveOperatorNames.Equal),
        ("!=", SieveOperatorNames.NotEquals),
        (">", SieveOperatorNames.GreaterThan),
        ("<", SieveOperatorNames.LessThan)
    ];

    /// <inheritdoc/>
    public QueryDescriptor Parse(QueryRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return new QueryDescriptor
        {
            FilterGroups = ParseFilters(request.Filters),
            Sorts = ParseSorts(request.Sorts),
            Search = string.IsNullOrWhiteSpace(request.Search)
                ? SearchDescriptor.Empty
                : new SearchDescriptor { Term = request.Search },
            Page = new PageDescriptor
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            }
        };
    }

    private static IReadOnlyList<SortDescriptor> ParseSorts(string sorts)
    {
        if (string.IsNullOrWhiteSpace(sorts))
            return Array.Empty<SortDescriptor>();

        var descriptors = new List<SortDescriptor>();

        foreach (var segment in SplitEscaped(sorts, ','))
        {
            var value = segment.Trim();

            if (value.Length == 0)
                continue;

            var direction = value[0] == '-'
                ? SortDirection.Descending
                : SortDirection.Ascending;

            var field = direction == SortDirection.Descending
                ? value[1..].Trim()
                : value;

            if (field.Length == 0)
                throw new FormatException("Sieve sort field cannot be empty.");

            descriptors.Add(new SortDescriptor
            {
                Field = Unescape(field),
                Direction = direction
            });
        }

        return descriptors;
    }

    private static IReadOnlyList<FilterGroupDescriptor> ParseFilters(string filters)
    {
        if (string.IsNullOrWhiteSpace(filters))
            return Array.Empty<FilterGroupDescriptor>();

        var groups = new List<FilterGroupDescriptor>();

        foreach (var segment in SplitEscaped(filters, ','))
        {
            var filter = segment.Trim();

            if (filter.Length == 0)
                continue;

            groups.Add(ParseFilterGroup(filter));
        }

        return groups;
    }

    private static FilterGroupDescriptor ParseFilterGroup(string filter)
    {
        var operatorMatch = FindOperator(filter);

        if (operatorMatch == null)
            throw new FormatException($"Sieve filter '{filter}' does not contain a supported operator.");

        var fieldPart = filter[..operatorMatch.Value.Index].Trim();
        var valuePart = filter[(operatorMatch.Value.Index + operatorMatch.Value.Token.Length)..].Trim();

        if (fieldPart.Length == 0)
            throw new FormatException($"Sieve filter '{filter}' does not contain a field name.");

        var fields = ParseFieldNames(fieldPart);
        var values = SplitEscaped(valuePart, '|');
        var descriptors = new List<FilterDescriptor>();

        foreach (var field in fields)
        {
            foreach (var rawValue in values)
            {
                var trimmedValue = rawValue.Trim();

                descriptors.Add(new FilterDescriptor
                {
                    Field = Unescape(field),
                    Operator = operatorMatch.Value.Name,
                    RawValue = Unescape(trimmedValue),
                    Value = ParseValue(trimmedValue)
                });
            }
        }

        return new FilterGroupDescriptor
        {
            LogicalOperator = descriptors.Count > 1 ? QueryLogicalOperator.Or : QueryLogicalOperator.And,
            Filters = descriptors
        };
    }

    private static IReadOnlyList<string> ParseFieldNames(string fieldPart)
    {
        if (fieldPart.StartsWith("(", StringComparison.Ordinal) &&
            fieldPart.EndsWith(")", StringComparison.Ordinal))
        {
            var inner = fieldPart[1..^1];
            return SplitEscaped(inner, '|')
                .Select(field => field.Trim())
                .Where(field => field.Length > 0)
                .ToArray();
        }

        return [fieldPart];
    }

    private static QueryValue ParseValue(string rawValue)
    {
        if (string.Equals(rawValue, "null", StringComparison.OrdinalIgnoreCase))
            return QueryValue.Null;

        return QueryValue.From(Unescape(rawValue));
    }

    private static (int Index, string Token, string Name)? FindOperator(string filter)
    {
        foreach (var (token, name) in Operators)
        {
            var index = filter.IndexOf(token, StringComparison.Ordinal);

            if (index >= 0)
                return (index, token, name);
        }

        return null;
    }

    private static IReadOnlyList<string> SplitEscaped(string value, char separator)
    {
        var segments = new List<string>();
        var builder = new StringBuilder();
        var escaped = false;

        foreach (var current in value)
        {
            if (escaped)
            {
                builder.Append('\\');
                builder.Append(current);
                escaped = false;
                continue;
            }

            if (current == '\\')
            {
                escaped = true;
                continue;
            }

            if (current == separator)
            {
                segments.Add(builder.ToString());
                builder.Clear();
                continue;
            }

            builder.Append(current);
        }

        if (escaped)
            builder.Append('\\');

        segments.Add(builder.ToString());

        return segments;
    }

    private static string Unescape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var builder = new StringBuilder(value.Length);
        var escaped = false;

        foreach (var current in value)
        {
            if (escaped)
            {
                builder.Append(current);
                escaped = false;
                continue;
            }

            if (current == '\\')
            {
                escaped = true;
                continue;
            }

            builder.Append(current);
        }

        if (escaped)
            builder.Append('\\');

        return builder.ToString();
    }
}
