using Microsoft.Extensions.Options;

using Acme.Platform.Application.Services;

namespace Acme.Platform.Infrastructure.Authentication;

/// <summary>
/// Issues refresh tokens. Generation and hashing come from
/// <see cref="SecretTokenFactory"/>, shared with invitations; what belongs here
/// is the lifetime, which is the only part that differs.
/// </summary>
public sealed class RefreshTokenIssuer : IRefreshTokenIssuer
{
    private readonly SecretTokenFactory _tokens;
    private readonly RefreshTokenOptions _options;

    public RefreshTokenIssuer(
        SecretTokenFactory tokens,
        IOptions<RefreshTokenOptions> options)
    {
        _tokens = tokens;
        _options = options.Value;
    }

    public IssuedRefreshToken Issue(DateTime now)
    {
        var value = _tokens.CreateValue();

        return new IssuedRefreshToken(
            value,
            Hash(value),
            now.AddDays(_options.Days));
    }

    public string Hash(string tokenValue) => _tokens.Hash(tokenValue);
}
