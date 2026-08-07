namespace DataScorpio.Tests.Querying;

using DataScorpio.Querying;

public sealed class QueryResultTests
{
    [Fact]
    public void Result_exposes_datascorpio_and_turtlepath_paging_names()
    {
        var result = new QueryResult<string>
        {
            Results = ["Ada", "Grace"],
            CurrentPage = 2,
            PageSize = 2,
            RowCount = 5,
            PageCount = 3
        };

        Assert.Equal(["Ada", "Grace"], result.Items);
        Assert.Equal(["Ada", "Grace"], result.Results);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(5, result.TotalRows);
        Assert.Equal(5, result.RowCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(3, result.PageCount);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }
}
