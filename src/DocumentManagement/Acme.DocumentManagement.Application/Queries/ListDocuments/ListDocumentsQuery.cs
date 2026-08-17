namespace Acme.DocumentManagement.Application.Queries.ListDocuments;

/// <summary>
/// The tenant's documents. No parameters today — the caller's tenant is
/// ambient (ADR-013) and applied by the global query filter; the record
/// exists so the first parameter is a change to a type, not to a method
/// signature (SC-003).
/// </summary>
public sealed record ListDocumentsQuery;
