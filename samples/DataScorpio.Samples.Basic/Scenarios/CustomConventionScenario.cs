namespace DataScorpio.Samples.Basic.Scenarios;

using DataScorpio.Execution;
using DataScorpio.Querying;
using DataScorpio.Samples.Basic.Data;
using DataScorpio.Samples.Basic.Output;

internal sealed class CustomConventionScenario : IQueryScenario
{
    private readonly IQueryProcessor processor;
    private readonly ConsoleResultWriter writer;

    public CustomConventionScenario(IQueryProcessor processor, ConsoleResultWriter writer)
    {
        this.processor = processor;
        this.writer = writer;
    }

    public void Run()
    {
        var result = processor.Execute(SampleData.Customers(), new QueryRequest
        {
            Filters = "ForTenant==atlas,InRegion==South",
            Sorts = "-RecentlyCreated"
        });

        writer.Write("Contract custom filters and sort: tenant + region + recently created", result);
    }
}

