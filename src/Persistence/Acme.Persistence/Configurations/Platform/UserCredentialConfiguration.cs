using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Acme.Platform.Domain.Aggregates.User;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;
using Acme.Platform.Contracts;
using UserCredentialAggregate =
    Acme.Platform.Domain.Aggregates.UserCredential.UserCredential;

namespace Acme.Persistence.Configurations.Platform;

public sealed class UserCredentialConfiguration
    : IEntityTypeConfiguration<UserCredentialAggregate>
{
    public void Configure(EntityTypeBuilder<UserCredentialAggregate> builder)
    {
        builder.ToTable("UserCredentials");

        // The key is the UserId, which is what enforces at most one credential
        // per user in the schema rather than in a rule someone has to remember.
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("UserId")
            .HasConversion(
                id => id.Value,
                value => new UserId(value));

        // Opaque to the domain and to the database alike. The length allows
        // room for the framework's versioned format to grow; nothing parses it.
        builder.Property(x => x.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.CreatedOn)
            .IsRequired();

        builder.Property(x => x.UpdatedOn)
            .IsRequired();

        // A credential has no identity outside its user, so the database
        // enforces that lifetime rather than trusting every caller to remember
        // it (ADR-026). Declared without navigation properties on either side:
        // the two remain separate aggregates, loaded and saved independently,
        // and neither can reach the other in code.
        builder.HasOne<UserAggregate>()
            .WithOne()
            .HasForeignKey<UserCredentialAggregate>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
