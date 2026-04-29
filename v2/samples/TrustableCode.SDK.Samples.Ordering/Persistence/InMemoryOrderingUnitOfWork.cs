namespace TrustableCode.SDK.Samples.Ordering;

public sealed class InMemoryOrderingUnitOfWork : IOrderingUnitOfWork
{
    public void Commit()
    {
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
