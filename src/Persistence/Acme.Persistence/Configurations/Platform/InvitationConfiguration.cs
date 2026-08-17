using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Acme.Platform.Domain.Aggregates.Invitation;
using Acme.Platform.Domain.Aggregates.User;

using InvitationAggregate =
    Acme.Platform.Domain.Aggregates.Invitation.Invitation;
using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;
using Acme.Platform.Contracts;

namespace Acme.Persistence.Configurations.Platform;

public sealed class InvitationConfiguration
    : IEntityTypeConfiguration<InvitationAggregate>
{
    public void Configure(EntityTypeBuilder<InvitationAggregate> builder)
    {
        builder.ToTable("Invitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new InvitationId(value));

        builder.Property(x => x.UserId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        // SHA-256 as uppercase hex is always 64 characters.
        builder.Property(x => x.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.CreatedOn).IsRequired();
        builder.Property(x => x.ConsumedOn);
        builder.Property(x => x.RevokedOn);

        // Every issued token is distinct, so a collision would mean the RNG or
        // the hash failed and the database should refuse it.
        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.HasIndex(x => x.UserId);

        // An invitation to a user who no longer exists cannot be accepted by
        // anyone, so its lifetime belongs to the user (ADR-026). One-to-many,
        // like RefreshToken.
        builder.HasOne<UserAggregate>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
