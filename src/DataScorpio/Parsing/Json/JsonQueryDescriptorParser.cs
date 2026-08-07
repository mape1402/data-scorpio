namespace DataScorpio.Parsing.Json;

using System.Text.Json;
using DataScorpio.Querying;

/// <summary>
/// Default parser for native JSON query descriptors.
/// </summary>
public sealed class JsonQueryDescriptorParser : IJsonQueryDescriptorParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <inheritdoc/>
    public QueryDescriptor Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return QueryDescriptor.Empty;

        var model = JsonSerializer.Deserialize<JsonQueryDescriptor>(json, SerializerOptions);

        if (model == null)
            return QueryDescriptor.Empty;

        return new QueryDescriptor
        {
            FilterGroups = BuildFilterGroups(model),
            Sorts = BuildSorts(model.Sorts),
            Search = BuildSearch(model.Search),
            Page = BuildPage(model.Page),
            Presets = BuildPresets(model.Presets)
        };
    }

    private static IReadOnlyList<FilterGroupDescriptor> BuildFilterGroups(JsonQueryDescriptor model)
    {
        if (model.FilterGroups is { Count: > 0 })
            return model.FilterGroups.Select(BuildFilterGroup).ToArray();

        if (model.Filters is { Count: > 0 })
            return
            [
                new FilterGroupDescriptor
                {
                    LogicalOperator = QueryLogicalOperator.And,
                    Filters = model.Filters.Select(BuildFilter).ToArray()
                }
            ];

        return Array.Empty<FilterGroupDescriptor>();
    }

    private static FilterGroupDescriptor BuildFilterGroup(JsonFilterGroup group)
        => new()
        {
            LogicalOperator = ParseLogicalOperator(group.LogicalOperator),
            Filters = (group.Filters ?? Array.Empty<JsonFilter>()).Select(BuildFilter).ToArray()
        };

    private static FilterDescriptor BuildFilter(JsonFilter filter)
        => new()
        {
            Field = filter.Field,
            Operator = filter.Operator,
            RawValue = filter.Value.ValueKind == JsonValueKind.Undefined ? null : filter.Value.GetRawText(),
            Value = BuildValue(filter.Value)
        };

    private static IReadOnlyList<SortDescriptor> BuildSorts(IReadOnlyList<JsonSort> sorts)
        => sorts == null
            ? Array.Empty<SortDescriptor>()
            : sorts.Select(sort => new SortDescriptor
            {
                Field = sort.Field,
                Direction = ParseSortDirection(sort.Direction)
            }).ToArray();

    private static SearchDescriptor BuildSearch(JsonSearch search)
        => search == null || string.IsNullOrWhiteSpace(search.Term)
            ? SearchDescriptor.Empty
            : new SearchDescriptor
            {
                Term = search.Term,
                Fields = search.Fields ?? Array.Empty<string>()
            };

    private static PageDescriptor BuildPage(JsonPage page)
        => page == null
            ? PageDescriptor.Unpaged
            : new PageDescriptor
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize
            };

    private static IReadOnlyList<QueryPresetDescriptor> BuildPresets(IReadOnlyList<JsonPreset> presets)
        => presets == null
            ? Array.Empty<QueryPresetDescriptor>()
            : presets.Select(preset => new QueryPresetDescriptor
            {
                Name = preset.Name,
                Arguments = BuildArguments(preset.Arguments)
            }).ToArray();

    private static IReadOnlyDictionary<string, QueryValue> BuildArguments(IReadOnlyDictionary<string, JsonElement> arguments)
        => arguments == null
            ? new Dictionary<string, QueryValue>()
            : arguments.ToDictionary(pair => pair.Key, pair => BuildValue(pair.Value));

    private static QueryValue BuildValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Undefined => QueryValue.Missing,
            JsonValueKind.Null => QueryValue.Null,
            JsonValueKind.String => QueryValue.From(value.GetString()),
            JsonValueKind.True => QueryValue.From(true),
            JsonValueKind.False => QueryValue.From(false),
            JsonValueKind.Number when value.TryGetInt64(out var longValue) => QueryValue.From(longValue),
            JsonValueKind.Number when value.TryGetDecimal(out var decimalValue) => QueryValue.From(decimalValue),
            JsonValueKind.Number when value.TryGetDouble(out var doubleValue) => QueryValue.From(doubleValue),
            JsonValueKind.Array => QueryValue.From(value.EnumerateArray().Select(BuildValue).ToArray()),
            _ => QueryValue.From(value.GetRawText())
        };
    }

    private static QueryLogicalOperator ParseLogicalOperator(string value)
        => string.Equals(value, "or", StringComparison.OrdinalIgnoreCase)
            ? QueryLogicalOperator.Or
            : QueryLogicalOperator.And;

    private static SortDirection ParseSortDirection(string value)
        => string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(value, "descending", StringComparison.OrdinalIgnoreCase)
            ? SortDirection.Descending
            : SortDirection.Ascending;

    private sealed class JsonQueryDescriptor
    {
        public IReadOnlyList<JsonFilter> Filters { get; init; }

        public IReadOnlyList<JsonFilterGroup> FilterGroups { get; init; }

        public IReadOnlyList<JsonSort> Sorts { get; init; }

        public JsonSearch Search { get; init; }

        public JsonPage Page { get; init; }

        public IReadOnlyList<JsonPreset> Presets { get; init; }
    }

    private sealed class JsonFilterGroup
    {
        public string LogicalOperator { get; init; }

        public IReadOnlyList<JsonFilter> Filters { get; init; }
    }

    private sealed class JsonFilter
    {
        public string Field { get; init; }

        public string Operator { get; init; }

        public JsonElement Value { get; init; }
    }

    private sealed class JsonSort
    {
        public string Field { get; init; }

        public string Direction { get; init; }
    }

    private sealed class JsonSearch
    {
        public string Term { get; init; }

        public IReadOnlyList<string> Fields { get; init; }
    }

    private sealed class JsonPage
    {
        public int? PageNumber { get; init; }

        public int? PageSize { get; init; }
    }

    private sealed class JsonPreset
    {
        public string Name { get; init; }

        public IReadOnlyDictionary<string, JsonElement> Arguments { get; init; }
    }
}
