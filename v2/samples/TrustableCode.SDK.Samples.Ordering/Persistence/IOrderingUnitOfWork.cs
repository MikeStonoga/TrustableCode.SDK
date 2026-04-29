namespace TrustableCode.SDK.Samples.Ordering;

public interface IOrderingUnitOfWork
{
    void Commit();

    Task CommitAsync(CancellationToken cancellationToken = default);
}
