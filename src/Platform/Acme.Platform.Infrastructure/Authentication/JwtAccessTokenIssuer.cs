using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using Acme.Platform.Application.Services;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;

namespace Acme.Platform.Infrastructure.Authentication;

/// <summary>
/// Issues signed JWTs. Like <see cref="Services.PasswordHasher"/>, this writes
/// no cryptography of its own: the signing, encoding and format all belong to
/// the framework's token handler.
/// </summary>
public sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _credentials;

    public JwtAccessTokenIssuer(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SigningKey!));

        _credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);
    }

    public AccessToken Issue(UserAggregate user)
    {
        // UtcNow once, so "issued at" and "expires at" cannot straddle a tick
        // and produce a token that looks issued after it expires.
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_options.AccessTokenMinutes);

        var claims = new Dictionary<string, object>
        {
            // The user id is the subject. The role rides along for the
            // authorization policies — see AcmeClaims.Role for why that
            // stopped being forbidden (ADR-033). Still no name or status
            // claims: those really are snapshots nothing downstream checks.
            //
            // A tenant claim travelled here until ADR-066. Which customer a
            // token belongs to is now the deployment that issued it, and a
            // token issued elsewhere fails on issuer and signature anyway.
            [JwtRegisteredClaimNames.Sub] = user.Id.Value.ToString(),
            [JwtRegisteredClaimNames.Email] = user.Email.Value,
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            [AcmeClaims.Role] = user.Role.ToString()
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expiresAt,
            SigningCredentials = _credentials,
            Claims = claims
        };

        return new AccessToken(
            new JsonWebTokenHandler().CreateToken(descriptor),
            expiresAt);
    }
}
