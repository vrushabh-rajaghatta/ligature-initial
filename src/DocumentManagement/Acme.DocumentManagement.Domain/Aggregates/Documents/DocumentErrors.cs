namespace Acme.DocumentManagement.Domain.Aggregates.Documents;

public static class DocumentErrors
{
    public const string DocumentNameRequired =
        "Document Name is required.";

    public const string DocumentNameTooLong =
        "Document Name must be 200 characters or fewer.";

    public const string DocumentAlreadyHasInitialVersion =
        "The document already has an initial version.";

    public const string DocumentHasNoInitialVersion =
        "The document has no initial version yet.";

    public const string DocumentAlreadyActive =
        "The document is already active.";

    public const string DocumentArchived =
        "An archived document cannot be changed.";

    public const string CannotActivateWithoutVersion =
        "A document cannot be activated before it has content.";

    public const string CannotArchiveDraft =
        "A draft document cannot be archived; it has never been active.";

    public const string OriginalFileNameRequired =
        "Original file name is required.";

    public const string StoredFileNameRequired =
        "Stored file name is required.";

    public const string ContentTypeRequired =
        "Content type is required.";

    public const string InvalidStoragePath =
        "Storage path is required.";

    public const string InvalidFileSize =
        "File size must be greater than zero.";
}
