namespace Acme.DocumentManagement.Domain.Aggregates.Documents;

/// <summary>
/// Lifecycle over deletion: a document moves between states and is never
/// removed. Archived is terminal.
/// </summary>
public enum DocumentStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3,
}
