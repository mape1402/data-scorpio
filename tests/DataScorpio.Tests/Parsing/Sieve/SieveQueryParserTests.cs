namespace DataScorpio.Tests.Parsing.Sieve;

using DataScorpio.Parsing.Sieve;
using DataScorpio.Querying;

public sealed class SieveQueryParserTests
{
    private readonly SieveQueryParser parser = new();

    [Fact]
    public void Parse_reads_sieve_sort_string()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Sorts = "LikeCount,CommentCount,-created"
        });

        Assert.Collection(
            descriptor.Sorts,
            sort =>
            {
                Assert.Equal("LikeCount", sort.Field);
                Assert.Equal(SortDirection.Ascending, sort.Direction);
            },
            sort =>
            {
                Assert.Equal("CommentCount", sort.Field);
                Assert.Equal(SortDirection.Ascending, sort.Direction);
            },
            sort =>
            {
                Assert.Equal("created", sort.Field);
                Assert.Equal(SortDirection.Descending, sort.Direction);
            });
    }

    [Fact]
    public void Parse_reads_basic_filter_clauses_as_and_groups()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = "LikeCount>10,Title@=awesome title"
        });

        Assert.Collection(
            descriptor.FilterGroups,
            group =>
            {
                var filter = Assert.Single(group.Filters);

                Assert.Equal(QueryLogicalOperator.And, group.LogicalOperator);
                Assert.Equal("LikeCount", filter.Field);
                Assert.Equal(SieveOperatorNames.GreaterThan, filter.Operator);
                Assert.Equal("10", filter.Value.Value);
            },
            group =>
            {
                var filter = Assert.Single(group.Filters);

                Assert.Equal(QueryLogicalOperator.And, group.LogicalOperator);
                Assert.Equal("Title", filter.Field);
                Assert.Equal(SieveOperatorNames.Contains, filter.Operator);
                Assert.Equal("awesome title", filter.Value.Value);
            });
    }

    [Theory]
    [InlineData("Title==Ada", SieveOperatorNames.Equal)]
    [InlineData("Title!=Ada", SieveOperatorNames.NotEquals)]
    [InlineData("Title>=Ada", SieveOperatorNames.GreaterThanOrEqual)]
    [InlineData("Title<=Ada", SieveOperatorNames.LessThanOrEqual)]
    [InlineData("Title_=Ada", SieveOperatorNames.StartsWith)]
    [InlineData("Title_-=Ada", SieveOperatorNames.EndsWith)]
    [InlineData("Title!@=Ada", SieveOperatorNames.NotContains)]
    [InlineData("Title!_=Ada", SieveOperatorNames.NotStartsWith)]
    [InlineData("Title!_-=Ada", SieveOperatorNames.NotEndsWith)]
    [InlineData("Title@=*Ada", SieveOperatorNames.ContainsInsensitive)]
    [InlineData("Title_=*Ada", SieveOperatorNames.StartsWithInsensitive)]
    [InlineData("Title_-=*Ada", SieveOperatorNames.EndsWithInsensitive)]
    [InlineData("Title==*Ada", SieveOperatorNames.EqualInsensitive)]
    [InlineData("Title!=*Ada", SieveOperatorNames.NotEqualsInsensitive)]
    [InlineData("Title!@=*Ada", SieveOperatorNames.NotContainsInsensitive)]
    [InlineData("Title!_=*Ada", SieveOperatorNames.NotStartsWithInsensitive)]
    [InlineData("Title!_-=*Ada", SieveOperatorNames.NotEndsWithInsensitive)]
    public void Parse_maps_sieve_operators(string filters, string expectedOperator)
    {
        var descriptor = parser.Parse(new QueryRequest { Filters = filters });
        var filter = Assert.Single(Assert.Single(descriptor.FilterGroups).Filters);

        Assert.Equal(expectedOperator, filter.Operator);
    }

    [Fact]
    public void Parse_supports_or_across_fields()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = "(LikeCount|CommentCount)>10"
        });

        var group = Assert.Single(descriptor.FilterGroups);

        Assert.Equal(QueryLogicalOperator.Or, group.LogicalOperator);
        Assert.Collection(
            group.Filters,
            filter => Assert.Equal("LikeCount", filter.Field),
            filter => Assert.Equal("CommentCount", filter.Field));
    }

    [Fact]
    public void Parse_supports_or_across_values()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = "Title@=new|hot"
        });

        var group = Assert.Single(descriptor.FilterGroups);

        Assert.Equal(QueryLogicalOperator.Or, group.LogicalOperator);
        Assert.Collection(
            group.Filters,
            filter => Assert.Equal("new", filter.Value.Value),
            filter => Assert.Equal("hot", filter.Value.Value));
    }

    [Fact]
    public void Parse_supports_combined_field_and_value_or()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = "(Title|Summary)@=new|hot"
        });

        var group = Assert.Single(descriptor.FilterGroups);

        Assert.Equal(QueryLogicalOperator.Or, group.LogicalOperator);
        Assert.Equal(4, group.Filters.Count);
    }

    [Fact]
    public void Parse_supports_escaped_comma_and_pipe_values()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = @"Title@=some\,title,Summary@=some\|summary"
        });

        Assert.Collection(
            descriptor.FilterGroups,
            group => Assert.Equal("some,title", Assert.Single(group.Filters).Value.Value),
            group => Assert.Equal("some|summary", Assert.Single(group.Filters).Value.Value));
    }

    [Fact]
    public void Parse_distinguishes_null_from_escaped_null_literal()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = @"DeletedAt==null,Name==\null"
        });

        Assert.Collection(
            descriptor.FilterGroups,
            group =>
            {
                var value = Assert.Single(group.Filters).Value;
                Assert.True(value.IsNull);
            },
            group =>
            {
                var value = Assert.Single(group.Filters).Value;
                Assert.False(value.IsNull);
                Assert.Equal("null", value.Value);
            });
    }

    [Fact]
    public void Parse_copies_page_and_search_values()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Search = "ada",
            PageNumber = 2,
            PageSize = 25
        });

        Assert.Equal("ada", descriptor.Search.Term);
        Assert.Equal(2, descriptor.Page.PageNumber);
        Assert.Equal(25, descriptor.Page.PageSize);
    }
}
