namespace DataScorpio.Samples.Basic.Scenarios;

using DataScorpio.Execution;
using DataScorpio.Parsing.Json;
using DataScorpio.Samples.Basic.Data;
using DataScorpio.Samples.Basic.Output;

internal sealed class NativeJsonScenario : IQueryScenario
{
    private readonly IJsonQueryDescriptorParser jsonParser;
    private readonly IQueryProcessor processor;
    private readonly ConsoleResultWriter writer;

    public NativeJsonScenario(
        IJsonQueryDescriptorParser jsonParser,
        IQueryProcessor processor,
        ConsoleResultWriter writer)
    {
        this.jsonParser = jsonParser;
        this.processor = processor;
        this.writer = writer;
    }

    public void Run()
    {
        var descriptor = jsonParser.Parse("""
        {
          "filters": [
            { "field": "Status", "operator": "equals", "value": "Active" }
          ],
          "sorts": [
            { "field": "CreatedAt", "direction": "desc" }
          ],
          "search": {
            "term": "south",
            "fields": [ "Region" ]
          },
          "page": {
            "pageNumber": 1,
            "pageSize": 5
          }
        }
        """);

        var result = processor.Execute(SampleData.Customers(), descriptor);

        writer.Write("Native JSON descriptor: active customers in searchable south region", result);
    }
}

