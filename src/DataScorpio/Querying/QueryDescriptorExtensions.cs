namespace DataScorpio.Querying;

/// <summary>
/// Provides query descriptor helper methods.
/// </summary>
public static class QueryDescriptorExtensions
{
    /// <summary>
    /// Creates a copy of the descriptor with a different page descriptor.
    /// </summary>
    /// <param name="descriptor">The source descriptor.</param>
    /// <param name="page">The replacement page descriptor.</param>
    /// <returns>The copied descriptor.</returns>
    public static QueryDescriptor WithPage(this QueryDescriptor descriptor, PageDescriptor page)
    {
        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        return new QueryDescriptor
        {
            FilterGroups = descriptor.FilterGroups,
            Sorts = descriptor.Sorts,
            Search = descriptor.Search,
            Page = page,
            Presets = descriptor.Presets
        };
    }
}
