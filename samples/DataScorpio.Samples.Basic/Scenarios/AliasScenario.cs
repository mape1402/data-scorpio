namespace DataScorpio.Samples.Basic.Scenarios;

using DataScorpio.Execution;
using DataScorpio.Querying;
using DataScorpio.Samples.Basic.Data;
using DataScorpio.Samples.Basic.Output;

internal sealed class AliasScenario : IQueryScenario
{
    private readonly IQueryProcessor processor;
    private readonly ConsoleResultWriter writer;

    public AliasScenario(IQueryProcessor processor, ConsoleResultWriter writer)
    {
        this.processor = processor;
        this.writer = writer;
    }

    public void Run()
    {
        var result = processor.Execute(SampleData.Customers(), new QueryRequest
        {
            Filters = "customerName@=*grace"
        });

        writer.Write("Alias filter: customerName@=*grace", result);
    }
}

