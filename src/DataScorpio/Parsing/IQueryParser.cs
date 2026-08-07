namespace DataScorpio.Parsing;

using DataScorpio.Querying;

/// <summary>
/// Parses raw query input into a structured query descriptor.
/// </summary>
public interface IQueryParser
{
    /// <summary>
    /// Parses a query request.
    /// </summary>
    /// <param name="request">The raw query request.</param>
    /// <returns>The parsed query descriptor.</returns>
    QueryDescriptor Parse(QueryRequest request);
}
