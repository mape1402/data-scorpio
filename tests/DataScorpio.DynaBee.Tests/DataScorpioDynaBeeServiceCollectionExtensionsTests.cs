namespace DataScorpio.DynaBee.Tests;

using Microsoft.Extensions.DependencyInjection;

public sealed class DataScorpioDynaBeeServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDataScorpioDynaBee_registers_accelerator()
    {
        var services = new ServiceCollection();

        services.AddDataScorpioDynaBee();

        using var provider = services.BuildServiceProvider();
        var accelerator = provider.GetRequiredService<IDataScorpioDynaBeeAccelerator>();

        Assert.True(accelerator.IsEnabled);
        Assert.IsType<DataScorpioDynaBeeAccelerator>(accelerator);
    }
}
