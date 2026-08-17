using Acme.Platform.Application.Services;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Tests.Fakes;

/// <summary>
/// Policy stand-in: each rule either passes or fails with a supplied exception,
/// so handler orchestration can be tested without a database.
/// </summary>
public sealed class FakeUserPolicy : IUserPolicy
{
    private readonly Exception? _emailError;
    private readonly Exception? _updateEmailError;

    public FakeUserPolicy(
        Exception? emailError = null,
        Exception? updateEmailError = null)
    {
        _emailError = emailError;
        _updateEmailError = updateEmailError;
    }

    public Task EnsureEmailIsUniqueAsync(
        Email email,
        CancellationToken cancellationToken)
        => _emailError is null
            ? Task.CompletedTask
            : Task.FromException(_emailError);

    public Task EnsureEmailIsUniqueForUpdateAsync(
        UserId userId,
        Email email,
        CancellationToken cancellationToken)
        => _updateEmailError is null
            ? Task.CompletedTask
            : Task.FromException(_updateEmailError);
}
