namespace DataScorpio.AspNetCore.Tests.QueryRequestBinding;

using DataScorpio.AspNetCore.QueryRequestBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

public sealed class DataScorpioQueryCollectionExtensionsTests
{
    [Fact]
    public void ToDataScorpioQueryRequest_binds_standard_query_parameters()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["filters"] = "Name@=*ada",
            ["sorts"] = "-CreatedAt",
            ["search"] = "ada",
            ["pageNumber"] = "2",
            ["pageSize"] = "25"
        });

        var request = query.ToDataScorpioQueryRequest();

        Assert.Equal("Name@=*ada", request.Filters);
        Assert.Equal("-CreatedAt", request.Sorts);
        Assert.Equal("ada", request.Search);
        Assert.Equal(2, request.PageNumber);
        Assert.Equal(25, request.PageSize);
    }

    [Fact]
    public void ToDataScorpioQueryRequest_supports_common_aliases()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["filter"] = "Status==Active",
            ["sort"] = "Name",
            ["q"] = "grace",
            ["page"] = "3",
            ["size"] = "10"
        });

        var request = query.ToDataScorpioQueryRequest();

        Assert.Equal("Status==Active", request.Filters);
        Assert.Equal("Name", request.Sorts);
        Assert.Equal("grace", request.Search);
        Assert.Equal(3, request.PageNumber);
        Assert.Equal(10, request.PageSize);
    }

    [Fact]
    public void ToDataScorpioQueryRequest_ignores_invalid_integer_values()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["pageNumber"] = "wat",
            ["pageSize"] = "nah"
        });

        var request = query.ToDataScorpioQueryRequest();

        Assert.Null(request.PageNumber);
        Assert.Null(request.PageSize);
    }
}
