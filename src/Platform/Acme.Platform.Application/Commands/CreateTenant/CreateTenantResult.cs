using Acme.Platform.Domain.Aggregates.User;
using Acme.SharedKernel.Primitives;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Commands.CreateTenant;

public sealed record CreateTenantResult(
    TenantId TenantId,
    UserId AdminUserId);
