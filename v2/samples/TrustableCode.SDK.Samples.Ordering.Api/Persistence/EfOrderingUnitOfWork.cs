using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class EfOrderingUnitOfWork(OrderingDbContext db) : IOrderingUnitOfWork
{
    public void Commit()
        => db.SaveChanges();

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
