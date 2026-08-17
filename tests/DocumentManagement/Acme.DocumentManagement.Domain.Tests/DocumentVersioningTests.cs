using FluentAssertions;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.SharedKernel.Exceptions;

namespace Acme.DocumentManagement.Domain.Tests;

public class DocumentVersioningTests
{
    [Fact]
    public void AddInitialVersion_CreatesVersionOne()
    {
        var document = TestFactory.NewDocument();

        TestFactory.AddInitial(document);

        var version = document.Versions.Should().ContainSingle().Subject;
        version.VersionNumber.Should().Be(1);
    }

    [Fact]
    public void AddInitialVersion_SetsCurrentVersionPointer()
    {
        var document = TestFactory.NewDocument();

        TestFactory.AddInitial(document);

        document.CurrentVersionId.Should().Be(document.Versions.Single().Id);
    }

    [Fact]
    public void AddInitialVersion_Twice_Throws()
    {
        var document = TestFactory.NewDocument();
        TestFactory.AddInitial(document);

        var act = () => TestFactory.AddInitial(document);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(DocumentErrors.DocumentAlreadyHasInitialVersion);
    }

    [Fact]
    public void AddNewVersion_WithoutInitialVersion_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => TestFactory.AddNext(document);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(DocumentErrors.DocumentHasNoInitialVersion);
    }

    [Fact]
    public void AddNewVersion_NumbersSequentially()
    {
        var document = TestFactory.NewDocument();
        TestFactory.AddInitial(document);

        TestFactory.AddNext(document);
        TestFactory.AddNext(document);

        document.Versions.Select(v => v.VersionNumber)
            .Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public void AddNewVersion_MovesCurrentVersionPointer()
    {
        var document = TestFactory.NewDocument();
        TestFactory.AddInitial(document);
        var initialVersionId = document.CurrentVersionId;

        TestFactory.AddNext(document);

        document.CurrentVersionId.Should().NotBe(initialVersionId);
        document.CurrentVersionId.Should().Be(
            document.Versions.Single(v => v.VersionNumber == 2).Id);
    }

    [Fact]
    public void Versions_AreImmutableFromOutside()
    {
        var document = TestFactory.NewDocument();
        TestFactory.AddInitial(document);

        document.Versions.Should().BeAssignableTo<
            IReadOnlyCollection<DocumentVersion>>();
    }
}
