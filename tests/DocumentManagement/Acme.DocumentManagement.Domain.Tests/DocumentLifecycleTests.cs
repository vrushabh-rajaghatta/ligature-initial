using FluentAssertions;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.SharedKernel.Exceptions;

namespace Acme.DocumentManagement.Domain.Tests;

public class DocumentLifecycleTests
{
    // A document can only be activated once it has a version, so every
    // activation test starts from an uploaded document.
    private static Document UploadedDocument()
    {
        var document = TestFactory.NewDocument();
        TestFactory.AddInitial(document);
        return document;
    }

    [Fact]
    public void Activate_FromDraftWithVersion_BecomesActive()
    {
        var document = UploadedDocument();

        document.Activate();

        document.Status.Should().Be(DocumentStatus.Active);
    }

    [Fact]
    public void Archive_FromActive_BecomesArchived()
    {
        var document = UploadedDocument();
        document.Activate();

        document.Archive();

        document.Status.Should().Be(DocumentStatus.Archived);
    }

    [Fact]
    public void HappyPath_Draft_Active_Archived()
    {
        var document = UploadedDocument();

        document.Activate();
        document.Status.Should().Be(DocumentStatus.Active);

        document.Archive();
        document.Status.Should().Be(DocumentStatus.Archived);
    }

    [Fact]
    public void Activate_WithoutCurrentVersion_Throws()
    {
        // Draft, but no version uploaded yet.
        var document = TestFactory.NewDocument();

        var act = () => document.Activate();

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(DocumentErrors.CannotActivateWithoutVersion);
    }

    [Fact]
    public void Archive_FromDraft_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => document.Archive();

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(DocumentErrors.CannotArchiveDraft);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_Throws()
    {
        var document = UploadedDocument();
        document.Activate();

        var act = () => document.Activate();

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(DocumentErrors.DocumentAlreadyActive);
    }

    [Fact]
    public void Activate_WhenArchived_Throws()
    {
        var document = UploadedDocument();
        document.Activate();
        document.Archive();

        var act = () => document.Activate();

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(DocumentErrors.DocumentArchived);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_Throws()
    {
        var document = UploadedDocument();
        document.Activate();
        document.Archive();

        var act = () => document.Archive();

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(DocumentErrors.DocumentArchived);
    }
}
