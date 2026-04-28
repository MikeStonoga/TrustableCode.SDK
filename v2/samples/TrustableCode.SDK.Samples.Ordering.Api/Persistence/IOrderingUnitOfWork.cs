namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public interface IOrderingUnitOfWork
{
    void Commit();

    Task CommitAsync(CancellationToken cancellationToken = default);
}
