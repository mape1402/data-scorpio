namespace DataScorpio.Tests.Parsing.Json;

using DataScorpio.Parsing.Json;
using DataScorpio.Querying;

public sealed class JsonQueryDescriptorParserTests
{
    [Fact]
    public void Parse_maps_native_json_descriptor()
    {
        var parser = new JsonQueryDescriptorParser();

        var descriptor = parser.Parse("""
        {
          "filters": [
            { "field": "Status", "operator": "equals", "value": "Active" },
            { "field": "DeletedAt", "operator": "equals", "value": null }
          ],
          "sorts": [
            { "field": "CreatedAt", "direction": "desc" }
          ],
          "search": { "term": "ada", "fields": [ "Name", "Email" ] },
          "includes": [ "orders" ],
          "page": { "pageNumber": 2, "pageSize": 25 },
          "presets": [
            { "name": "tenant", "arguments": { "tenantId": 42 } }
          ]
        }
        """);

        var group = Assert.Single(descriptor.FilterGroups);
        Assert.Equal(QueryLogicalOperator.And, group.LogicalOperator);
        Assert.Collection(
            group.Filters,
            filter =>
            {
                Assert.Equal("Status", filter.Field);
                Assert.Equal("equals", filter.Operator);
                Assert.Equal("Active", filter.Value.Value);
            },
            filter =>
            {
                Assert.Equal("DeletedAt", filter.Field);
                Assert.True(filter.Value.IsNull);
            });

        var sort = Assert.Single(descriptor.Sorts);
        Assert.Equal("CreatedAt", sort.Field);
        Assert.Equal(SortDirection.Descending, sort.Direction);
        Assert.Equal("ada", descriptor.Search.Term);
        Assert.Equal(["Name", "Email"], descriptor.Search.Fields);
        Assert.Equal("orders", Assert.Single(descriptor.Includes).Name);
        Assert.Equal(2, descriptor.Page.PageNumber);
        Assert.Equal(25, descriptor.Page.PageSize);

        var preset = Assert.Single(descriptor.Presets);
        Assert.Equal("tenant", preset.Name);
        Assert.Equal(42L, preset.Arguments["tenantId"].Value);
    }

    [Fact]
    public void Parse_maps_explicit_filter_groups()
    {
        var parser = new JsonQueryDescriptorParser();

        var descriptor = parser.Parse("""
        {
          "filterGroups": [
            {
              "logicalOperator": "or",
              "filters": [
                { "field": "Name", "operator": "containsInsensitive", "value": "ada" },
                { "field": "Email", "operator": "containsInsensitive", "value": "ada" }
              ]
            }
          ]
        }
        """);

        var group = Assert.Single(descriptor.FilterGroups);

        Assert.Equal(QueryLogicalOperator.Or, group.LogicalOperator);
        Assert.Equal(2, group.Filters.Count);
    }
}
