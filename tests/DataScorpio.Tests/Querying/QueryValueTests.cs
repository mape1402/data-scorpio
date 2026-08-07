namespace DataScorpio.Tests.Querying;

using DataScorpio.Querying;

public sealed class QueryValueTests
{
    [Fact]
    public void Null_represents_explicit_null()
    {
        var value = QueryValue.Null;

        Assert.True(value.IsNull);
        Assert.False(value.IsMissing);
        Assert.Null(value.Value);
    }

    [Fact]
    public void Missing_represents_absent_value()
    {
        var value = QueryValue.Missing;

        Assert.False(value.IsNull);
        Assert.True(value.IsMissing);
        Assert.Null(value.Value);
    }

    [Fact]
    public void From_wraps_non_null_values()
    {
        var value = QueryValue.From("active");

        Assert.False(value.IsNull);
        Assert.False(value.IsMissing);
        Assert.Equal("active", value.Value);
    }
}
