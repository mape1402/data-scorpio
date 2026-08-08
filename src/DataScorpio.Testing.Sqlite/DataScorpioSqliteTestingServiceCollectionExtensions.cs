namespace Microsoft.Extensions.DependencyInjection;

using DataScorpio.Profiles;
using DataScorpio.Testing.Sqlite;

/// <summary>
/// Registers SQLite-backed DataScorpio testing helpers.
/// </summary>
public static class DataScorpioSqliteTestingServiceCollectionExtensions
{
    /// <summary>
    /// Registers SQLite-backed DataScorpio testing helpers using already registered DataScorpio services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddDataScorpioSqliteTesting(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddScoped(typeof(IDataScorpioSqliteTesting<>), typeof(DataScorpioSqliteTesting<>));

        return services;
    }

    /// <summary>
    /// Registers DataScorpio services and SQLite-backed testing helpers in one call.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureProfiles">The profile registry configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddDataScorpioSqliteTesting(
        this IServiceCollection services,
        Action<QueryProfileRegistryBuilder> configureProfiles)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddDataScorpioTesting(configureProfiles);
        services.AddDataScorpioSqliteTesting();

        return services;
    }
}

