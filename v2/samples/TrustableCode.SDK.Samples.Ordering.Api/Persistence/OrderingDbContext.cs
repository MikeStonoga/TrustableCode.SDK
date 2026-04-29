using Microsoft.EntityFrameworkCore;
using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<OrderSnapshotEntity> Orders => Set<OrderSnapshotEntity>();

    public DbSet<OrderLineEntity> OrderLines => Set<OrderLineEntity>();

    public DbSet<OrderingOutboxMessageEntity> OutboxMessages => Set<OrderingOutboxMessageEntity>();

    public DbSet<BusinessEvidenceEntity> BusinessEvidence => Set<BusinessEvidenceEntity>();

    public DbSet<SideEffectLifecycleEntity> SideEffectLifecycles => Set<SideEffectLifecycleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderSnapshotEntity>(entity =>
        {
            entity.HasKey(order => order.OrderId);
            entity.HasMany(order => order.Lines)
                .WithOne()
                .HasForeignKey(line => line.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLineEntity>(entity =>
        {
            entity.HasKey(line => line.Id);
            entity.Property(line => line.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<OrderingOutboxMessageEntity>()
            .HasKey(message => message.Id);

        modelBuilder.Entity<BusinessEvidenceEntity>()
            .HasKey(evidence => evidence.Id);

        modelBuilder.Entity<SideEffectLifecycleEntity>()
            .HasKey(lifecycle => lifecycle.IdempotencyKey);
    }
}
