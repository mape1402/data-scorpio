namespace DataScorpio.Samples.Basic.Querying;

using DataScorpio.Profiles;
using DataScorpio.Samples.Basic.Contracts;

internal sealed class SampleQueryConventions : QueryConventionSet
{
    public override void Configure(IQueryConventionBuilder builder)
    {
        builder
            .CustomFilter<IRegional>("InRegion", value =>
                customer => customer.Region == Convert.ToString(value.Value))
            .CustomFilter<ITenantScoped>("ForTenant", value =>
                customer => customer.TenantId == Convert.ToString(value.Value))
            .CustomSort<ICreated>("RecentlyCreated", customer => customer.CreatedAt);
    }
}

