namespace Acme.Api.Endpoints.Authentication;

public sealed record AcceptInvitationRequest(string? Token, string? Password);
