namespace Acme.Api.Endpoints.Authentication;

public sealed record ChangePasswordRequest(
    string? CurrentPassword,
    string? NewPassword);
