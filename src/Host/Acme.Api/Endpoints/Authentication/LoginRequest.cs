namespace Acme.Api.Endpoints.Authentication;

public sealed record LoginRequest(string? Email, string? Password);
