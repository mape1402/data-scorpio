namespace DataScorpio.Samples.Basic.Data;

using DataScorpio.Samples.Basic.Domain;

internal static class SampleData
{
    public static IQueryable<Customer> Customers()
        => new[]
        {
            new Customer(
                Name: "Ada Lovelace",
                Email: "ada@analytics.example",
                Status: "Active",
                Region: "North",
                TenantId: "elysium",
                CreatedAt: new DateTime(2026, 1, 3),
                DeletedAt: null),
            new Customer(
                Name: "Grace Hopper",
                Email: "grace@navy.example",
                Status: "Active",
                Region: "South",
                TenantId: "elysium",
                CreatedAt: new DateTime(2026, 1, 2),
                DeletedAt: null),
            new Customer(
                Name: "Alan Turing",
                Email: "alan@computing.example",
                Status: "Inactive",
                Region: "North",
                TenantId: "elysium",
                CreatedAt: new DateTime(2026, 1, 1),
                DeletedAt: new DateTime(2026, 2, 1)),
            new Customer(
                Name: "Katherine Johnson",
                Email: "katherine@flight.example",
                Status: "Active",
                Region: "South",
                TenantId: "atlas",
                CreatedAt: new DateTime(2026, 1, 4),
                DeletedAt: null),
            new Customer(
                Name: "Margaret Hamilton",
                Email: "margaret@apollo.example",
                Status: "Active",
                Region: "East",
                TenantId: "atlas",
                CreatedAt: new DateTime(2026, 1, 5),
                DeletedAt: null)
        }.AsQueryable();
}

