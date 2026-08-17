using Acme.Platform.Domain.Aggregates.Tenant;

namespace Acme.Platform.Application.Queries.GetTenants;

public sealed record TenantListItem(
    Guid Id,
    string Name,
    TenantStatus Status);
