namespace Microsoft.Extensions.DependencyInjection;

using DataScorpio.Profiles;
using DataScorpio.Testing;

/// <summary>
/// Registers DataScorpio testing helpers.
/// </summary>
public static class DataScorpioTestingServiceCollectionExtensions
{
    /// <summary>
    /// Registers DataScorpio testing helpers using already registered DataScorpio services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddDataScorpioTesting(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddScoped(typeof(IDataScorpioTesting<>), typeof(DataScorpioTesting<>));

        return services;
    }

    /// <summary>
    /// Registers DataScorpio services and testing helpers in one call.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureProfiles">The profile registry configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddDataScorpioTesting(
        this IServiceCollection services,
        Action<QueryProfileRegistryBuilder> configureProfiles)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddDataScorpio(configureProfiles);
        services.AddDataScorpioTesting();

        return services;
    }
}


