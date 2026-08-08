namespace DataScorpio.Samples.Basic.Querying;

using DataScorpio.Profiles;
using DataScorpio.Querying;
using DataScorpio.Samples.Basic.Domain;

internal sealed class CustomerQueryProfile : QueryProfile<Customer>
{
    public override void Configure(IQueryProfileBuilder<Customer> builder)
    {
        builder
            .AllowFilter(customer => customer.Name)
            .AllowFilter("customerName", customer => customer.Name)
            .AllowFilter(customer => customer.Email)
            .AllowFilter(customer => customer.Status)
            .AllowFilter(customer => customer.CreatedAt)
            .AllowFilter(customer => customer.DeletedAt)
            .AllowSearch(customer => customer.Name)
            .AllowSearch(customer => customer.Email)
            .AllowSearch(customer => customer.Region)
            .AllowSort(customer => customer.Name)
            .AllowSort(customer => customer.CreatedAt)
            .DefaultSort(customer => customer.CreatedAt, SortDirection.Descending)
            .MaxPageSize(50);
    }
}
