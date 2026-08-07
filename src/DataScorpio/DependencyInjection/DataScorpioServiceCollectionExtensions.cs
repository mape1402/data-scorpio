namespace Microsoft.Extensions.DependencyInjection;

using DataScorpio.Execution;
using DataScorpio.Parsing;
using DataScorpio.Parsing.Sieve;
using DataScorpio.Profiles;
using DataScorpio.Validation;

/// <summary>
/// Provides DataScorpio dependency injection registration helpers.
/// </summary>
public static class DataScorpioServiceCollectionExtensions
{
    /// <summary>
    /// Registers DataScorpio core services using the Sieve-compatible parser.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureProfiles">The profile registry configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddDataScorpioSieveCompatibility(
        this IServiceCollection services,
        Action<QueryProfileRegistryBuilder> configureProfiles)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configureProfiles == null)
            throw new ArgumentNullException(nameof(configureProfiles));

        var profileBuilder = new QueryProfileRegistryBuilder();
        configureProfiles(profileBuilder);
        var registry = profileBuilder.Build();

        services.AddSingleton<IQueryProfileRegistry>(registry);
        services.AddSingleton<IQueryParser, SieveQueryParser>();
        services.AddSingleton<IQueryDescriptorValidator, QueryDescriptorValidator>();
        services.AddSingleton<IQueryableQueryApplier, QueryableQueryApplier>();
        services.AddSingleton<IQueryProcessor, QueryProcessor>();

        return services;
    }
}
