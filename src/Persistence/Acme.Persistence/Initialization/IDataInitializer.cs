namespace Acme.Persistence.Initialization;

public interface IDataInitializer
{
    Task InitializeAsync(
        CancellationToken cancellationToken = default);
}
