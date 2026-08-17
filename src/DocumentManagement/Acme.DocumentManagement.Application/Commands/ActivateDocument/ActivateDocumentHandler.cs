using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.SharedKernel.Exceptions;

namespace Acme.DocumentManagement.Application.Commands.ActivateDocument;

public sealed class ActivateDocumentHandler
{
    private readonly IDocumentRepository _repository;

    public ActivateDocumentHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ActivateDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(
            command.DocumentId,
            cancellationToken);

        if (document is null)
            throw new NotFoundException(
                DocumentLifecycleErrors.DocumentDoesNotExist);

        // Invalid transitions are enforced by the aggregate; they surface as
        // BusinessRuleViolationException and map to 409 in middleware.
        document.Activate();

        await _repository.UpdateAsync(document, cancellationToken);
    }
}
