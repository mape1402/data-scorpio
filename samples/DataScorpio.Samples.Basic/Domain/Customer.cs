namespace DataScorpio.Samples.Basic.Domain;

using DataScorpio.Samples.Basic.Contracts;

internal sealed record Customer(
    string Name,
    string Email,
    string Status,
    string Region,
    string TenantId,
    DateTime CreatedAt,
    DateTime? DeletedAt) : IRegional, ITenantScoped, ICreated;

