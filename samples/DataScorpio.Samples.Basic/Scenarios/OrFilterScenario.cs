namespace DataScorpio.Samples.Basic.Scenarios;

using DataScorpio.Execution;
using DataScorpio.Querying;
using DataScorpio.Samples.Basic.Data;
using DataScorpio.Samples.Basic.Output;

internal sealed class OrFilterScenario : IQueryScenario
{
    private readonly IQueryProcessor processor;
    private readonly ConsoleResultWriter writer;

    public OrFilterScenario(IQueryProcessor processor, ConsoleResultWriter writer)
    {
        this.processor = processor;
        this.writer = writer;
    }

    public void Run()
    {
        var result = processor.Execute(SampleData.Customers(), new QueryRequest
        {
            Filters = "(Name|Email)@=*ada",
            Sorts = "Name"
        });

        writer.Write("Sieve-compatible OR filter: (Name|Email)@=*ada", result);
    }
}

