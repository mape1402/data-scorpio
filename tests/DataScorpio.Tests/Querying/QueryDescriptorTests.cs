namespace DataScorpio.Tests.Querying;

using DataScorpio.Querying;

public sealed class QueryDescriptorTests
{
    [Fact]
    public void Empty_descriptor_has_empty_children()
    {
        var descriptor = QueryDescriptor.Empty;

        Assert.Empty(descriptor.FilterGroups);
        Assert.Empty(descriptor.Sorts);
        Assert.Empty(descriptor.Presets);
        Assert.Same(SearchDescriptor.Empty, descriptor.Search);
        Assert.Same(PageDescriptor.Unpaged, descriptor.Page);
    }

    [Fact]
    public void Page_descriptor_is_paged_only_when_number_and_size_are_present()
    {
        Assert.False(PageDescriptor.Unpaged.IsPaged);

        var descriptor = new PageDescriptor
        {
            PageNumber = 2,
            PageSize = 25
        };

        Assert.True(descriptor.IsPaged);
    }

    [Fact]
    public void Filter_groups_can_represent_or_logic()
    {
        var group = new FilterGroupDescriptor
        {
            LogicalOperator = QueryLogicalOperator.Or,
            Filters =
            [
                new FilterDescriptor { Field = "Title", Operator = "contains", Value = QueryValue.From("new") },
                new FilterDescriptor { Field = "Summary", Operator = "contains", Value = QueryValue.From("new") }
            ]
        };

        Assert.Equal(QueryLogicalOperator.Or, group.LogicalOperator);
        Assert.Equal(2, group.Filters.Count);
    }
}
