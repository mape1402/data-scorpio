namespace Microsoft.Extensions.DependencyInjection;

using DataScorpio.DynaBee;

/// <summary>
/// Provides DataScorpio DynaBee registration helpers.
/// </summary>
public static class DataScorpioDynaBeeServiceCollectionExtensions
{
    /// <summary>
    /// Enables opt-in DynaBee acceleration services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddDataScorpioDynaBee(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<IDataScorpioDynaBeeAccelerator, DataScorpioDynaBeeAccelerator>();

        return services;
    }
}
