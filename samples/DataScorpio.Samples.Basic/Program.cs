namespace DataScorpio.Samples.Basic;

using DataScorpio.Samples.Basic.DependencyInjection;
using DataScorpio.Samples.Basic.Scenarios;
using Microsoft.Extensions.DependencyInjection;

internal static class Program
{
    private static void Main()
    {
        using var services = SampleServices.Build();

        foreach (var scenario in services.GetRequiredService<IEnumerable<IQueryScenario>>())
            scenario.Run();
    }
}

