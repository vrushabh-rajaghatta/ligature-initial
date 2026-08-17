using Acme.SharedKernel.Primitives;

namespace Acme.Platform.Application.Commands.ActivateTenant;

public sealed record ActivateTenantCommand(TenantId TenantId);
