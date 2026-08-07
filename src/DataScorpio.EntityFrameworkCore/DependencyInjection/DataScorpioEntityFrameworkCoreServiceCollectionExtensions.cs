namespace Microsoft.Extensions.DependencyInjection;

using DataScorpio.EntityFrameworkCore.Execution;

/// <summary>
/// Provides DataScorpio Entity Framework Core registration helpers.
/// </summary>
public static class DataScorpioEntityFrameworkCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers DataScorpio EF Core execution services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddDataScorpioEntityFrameworkCore(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<IEfCoreQueryProcessor, EfCoreQueryProcessor>();

        return services;
    }
}
