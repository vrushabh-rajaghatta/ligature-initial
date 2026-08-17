using Acme.SharedKernel.Primitives;

namespace Acme.Platform.Application.Commands.DeactivateTenant;

public sealed record DeactivateTenantCommand(TenantId TenantId);
