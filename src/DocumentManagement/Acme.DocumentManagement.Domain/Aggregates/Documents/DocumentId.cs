using Acme.SharedKernel.Primitives;

namespace Acme.DocumentManagement.Domain.Aggregates.Documents;

public sealed class DocumentId : StronglyTypedId
{
    public DocumentId(Guid value) : base(value)
    {
    }

    public static DocumentId New() => new(Guid.NewGuid());

    public static DocumentId From(Guid value) => new(value);

    public static implicit operator Guid(DocumentId id) => id.Value;
}
