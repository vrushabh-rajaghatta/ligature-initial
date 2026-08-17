using FluentAssertions;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.SharedKernel.Exceptions;
using Acme.SharedKernel.Primitives;

namespace Acme.DocumentManagement.Domain.Tests;

public class DocumentValidationTests
{
    [Fact]
    public void Create_WithNullTenant_Throws()
    {
        var act = () => Document.Create(null!, "Label");

        act.Should().Throw<DomainException>()
            .WithMessage($"{DocumentErrors.TenantRequired}*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankName_Throws(string? name)
    {
        var act = () => Document.Create(TenantId.New(), name!);

        act.Should().Throw<DomainException>()
            .WithMessage($"{DocumentErrors.DocumentNameRequired}*");
    }

    [Fact]
    public void Create_WithNameOverMaxLength_Throws()
    {
        var name = new string('a', Document.NameMaxLength + 1);

        var act = () => Document.Create(TenantId.New(), name);

        act.Should().Throw<DomainException>()
            .WithMessage($"{DocumentErrors.DocumentNameTooLong}*");
    }

    [Fact]
    public void AddInitialVersion_WithBlankOriginalFileName_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => document.AddInitialVersion(
            " ", "stored.pdf", "application/pdf", 10, "path", "sum");

        act.Should().Throw<DomainException>()
            .WithMessage($"{DocumentErrors.OriginalFileNameRequired}*");
    }

    [Fact]
    public void AddInitialVersion_WithBlankStoredFileName_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => document.AddInitialVersion(
            "original.pdf", " ", "application/pdf", 10, "path", "sum");

        act.Should().Throw<DomainException>()
            .WithMessage($"{DocumentErrors.StoredFileNameRequired}*");
    }

    [Fact]
    public void AddInitialVersion_WithBlankContentType_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => document.AddInitialVersion(
            "original.pdf", "stored.pdf", " ", 10, "path", "sum");

        act.Should().Throw<DomainException>()
            .WithMessage($"{DocumentErrors.ContentTypeRequired}*");
    }

    [Fact]
    public void AddInitialVersion_WithBlankStoragePath_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => document.AddInitialVersion(
            "original.pdf", "stored.pdf", "application/pdf", 10, " ", "sum");

        act.Should().Throw<DomainException>()
            .WithMessage($"{DocumentErrors.InvalidStoragePath}*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddInitialVersion_WithNonPositiveFileSize_Throws(long fileSize)
    {
        var document = TestFactory.NewDocument();

        var act = () => document.AddInitialVersion(
            "original.pdf", "stored.pdf", "application/pdf",
            fileSize, "path", "sum");

        act.Should().Throw<DomainException>()
            .WithMessage($"{DocumentErrors.InvalidFileSize}*");
    }
}
