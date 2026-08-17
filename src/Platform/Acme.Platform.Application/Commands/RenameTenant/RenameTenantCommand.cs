using Acme.SharedKernel.Primitives;

namespace Acme.Platform.Application.Commands.RenameTenant;

public sealed record RenameTenantCommand(
    TenantId TenantId,
    string? Name);
