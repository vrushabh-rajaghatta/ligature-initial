using FluentAssertions;

using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.DocumentManagement.Domain.Tests;

public class DocumentCreationTests
{
    [Fact]
    public void Create_StartsInDraft()
    {
        TestFactory.NewDocument().Status
            .Should().Be(DocumentStatus.Draft);
    }

    [Fact]
    public void Create_HasNoCurrentVersion()
    {
        TestFactory.NewDocument().CurrentVersionId.Should().BeNull();
    }

    [Fact]
    public void Create_HasNoVersions()
    {
        TestFactory.NewDocument().Versions.Should().BeEmpty();
    }

    [Fact]
    public void Create_SetsProvidedValues()
    {
        var document = Document.Create("  Risk Management File  ");

        document.Name.Should().Be("Risk Management File");
        document.CreatedOnUtc.Should().NotBe(default);
    }
}
