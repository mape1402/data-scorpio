namespace DataScorpio.Samples.Basic.DependencyInjection;

using DataScorpio.Samples.Basic;
using DataScorpio.Samples.Basic.Output;
using DataScorpio.Samples.Basic.Scenarios;
using Microsoft.Extensions.DependencyInjection;

internal static class SampleServices
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();

        services.AddDataScorpio(options => options.FromAssemblyOf<SampleAssemblyMarker>());

        services.AddSingleton<ConsoleResultWriter>();
        services.AddSingleton<IQueryScenario, BasicFilteringScenario>();
        services.AddSingleton<IQueryScenario, SearchScenario>();
        services.AddSingleton<IQueryScenario, OrFilterScenario>();
        services.AddSingleton<IQueryScenario, AliasScenario>();
        services.AddSingleton<IQueryScenario, CustomConventionScenario>();
        services.AddSingleton<IQueryScenario, NullFilterScenario>();
        services.AddSingleton<IQueryScenario, NativeJsonScenario>();
        services.AddSingleton<IQueryScenario, ValidationScenario>();

        return services.BuildServiceProvider();
    }
}
