namespace DataScorpio.Parsing.Json;

using DataScorpio.Querying;

/// <summary>
/// Parses native JSON query descriptors.
/// </summary>
public interface IJsonQueryDescriptorParser
{
    /// <summary>
    /// Parses JSON into a provider-neutral query descriptor.
    /// </summary>
    /// <param name="json">The native query descriptor JSON.</param>
    /// <returns>The parsed query descriptor.</returns>
    QueryDescriptor Parse(string json);
}
