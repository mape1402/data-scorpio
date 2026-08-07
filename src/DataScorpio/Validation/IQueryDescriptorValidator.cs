namespace DataScorpio.Validation;

using DataScorpio.Profiles;
using DataScorpio.Querying;

/// <summary>
/// Validates parsed query descriptors against configured profile metadata.
/// </summary>
public interface IQueryDescriptorValidator
{
    /// <summary>
    /// Validates a query descriptor.
    /// </summary>
    /// <param name="descriptor">The parsed query descriptor.</param>
    /// <param name="profile">The profile definition.</param>
    /// <returns>The validation result.</returns>
    QueryValidationResult Validate(QueryDescriptor descriptor, QueryProfileDefinition profile);
}
