namespace DataScorpio.AspNetCore.QueryRequestBinding;

using DataScorpio.Querying;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Converts ASP.NET Core query strings into DataScorpio query requests.
/// </summary>
public static class DataScorpioQueryCollectionExtensions
{
    /// <summary>
    /// Creates a DataScorpio query request from an ASP.NET Core query collection.
    /// </summary>
    /// <param name="query">The ASP.NET Core query collection.</param>
    /// <returns>The DataScorpio query request.</returns>
    public static QueryRequest ToDataScorpioQueryRequest(this IQueryCollection query)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        return new QueryRequest
        {
            Filters = GetFirst(query, "filters", "filter"),
            Sorts = GetFirst(query, "sorts", "sort", "orderBy"),
            Search = GetFirst(query, "search", "q"),
            PageNumber = GetInt(query, "pageNumber", "page", "currentPage"),
            PageSize = GetInt(query, "pageSize", "size", "take")
        };
    }

    private static string GetFirst(IQueryCollection query, params string[] names)
    {
        foreach (var name in names)
        {
            if (query.TryGetValue(name, out var value) && value.Count > 0)
                return value[0];
        }

        return null;
    }

    private static int? GetInt(IQueryCollection query, params string[] names)
    {
        var value = GetFirst(query, names);

        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.TryParse(value, out var parsed)
            ? parsed
            : null;
    }
}
