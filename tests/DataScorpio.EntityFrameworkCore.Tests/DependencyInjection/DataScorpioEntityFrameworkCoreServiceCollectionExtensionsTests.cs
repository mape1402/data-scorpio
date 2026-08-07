namespace DataScorpio.EntityFrameworkCore.Tests.DependencyInjection;

using DataScorpio.EntityFrameworkCore.Execution;
using Microsoft.Extensions.DependencyInjection;

public sealed class DataScorpioEntityFrameworkCoreServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDataScorpioEntityFrameworkCore_registers_processor()
    {
        var services = new ServiceCollection();

        services.AddDataScorpioEntityFrameworkCore();

        Assert.Contains(services, service => service.ServiceType == typeof(IEfCoreQueryProcessor));
    }
}
