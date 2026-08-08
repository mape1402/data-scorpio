namespace Microsoft.Extensions.DependencyInjection;

using DataScorpio.Profiles;
using DataScorpio.Testing;

/// <summary>
/// Registers Sieve-compatible testing helpers.
/// </summary>
public static class SieveTestingServiceCollectionExtensions
{
    /// <summary>
    /// Registers Sieve-compatible testing helpers using already registered DataScorpio services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddSieveTesting(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddScoped(typeof(ISieveTesting<>), typeof(SieveTesting<>));

        return services;
    }

    /// <summary>
    /// Registers DataScorpio and Sieve-compatible testing helpers in one call.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureProfiles">The profile registry configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddSieveTesting(
        this IServiceCollection services,
        Action<QueryProfileRegistryBuilder> configureProfiles)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddDataScorpio(configureProfiles);
        services.AddSieveTesting();

        return services;
    }
}

