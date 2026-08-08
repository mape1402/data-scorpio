namespace DataScorpio.Samples.Basic.Scenarios;

using DataScorpio.Execution;
using DataScorpio.Querying;
using DataScorpio.Samples.Basic.Data;
using DataScorpio.Samples.Basic.Output;

internal sealed class SearchScenario : IQueryScenario
{
    private readonly IQueryProcessor processor;
    private readonly ConsoleResultWriter writer;

    public SearchScenario(IQueryProcessor processor, ConsoleResultWriter writer)
    {
        this.processor = processor;
        this.writer = writer;
    }

    public void Run()
    {
        var result = processor.Execute(SampleData.Customers(), new QueryRequest
        {
            Filters = "Status==Active",
            Search = "apollo",
            Sorts = "Name"
        });

        writer.Write("Search with structured filter: search=apollo & Status==Active", result);
    }
}

