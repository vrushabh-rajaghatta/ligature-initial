using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;
using Acme.Platform.Contracts;

namespace Acme.Persistence.Configurations.Platform;

public sealed class UserConfiguration : IEntityTypeConfiguration<UserAggregate>
{
    public void Configure(EntityTypeBuilder<UserAggregate> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new UserId(value));

        // The Email value object is stored as its normalized string. Email.Create
        // re-runs (idempotent) normalization/validation on read; stored values are
        // always already valid, so it never throws during materialization.
        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOn)
            .IsRequired();

        // Defense in depth for the uniqueness policy. ADR-021 made an address
        // identify exactly one user because authentication had to resolve a
        // user before a tenant existed; ADR-066 removed the tenant, and the
        // rule is now simply that an address identifies one user in this
        // deployment — the only scope there is.
        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}
