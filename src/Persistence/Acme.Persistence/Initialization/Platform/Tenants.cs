using Acme.SharedKernel.Primitives;

using TenantAggregate = Acme.Platform.Domain.Aggregates.Tenant.Tenant;

namespace Acme.Persistence.Initialization.Platform;

internal static class Tenants
{
    public static IReadOnlyList<TenantAggregate> Data =>
    [
        TenantAggregate.Create(
            new TenantId(TenantIds.DemoManufacturer),
            "Demo Manufacturer Ltd."),
        TenantAggregate.Create(
            new TenantId(TenantIds.DemoSponsor),
            "Demo Sponsor Ltd."),
        TenantAggregate.Create(
            new TenantId(TenantIds.DemoMarketingAuthorizationHolder),
            "Demo MAH Ltd.")
    ];
}
