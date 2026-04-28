using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.Samples.Ordering.Api.Controllers;
using TrustableCode.SDK.Samples.Ordering.Api.Models;
using TrustableCode.SDK.Samples.Ordering.Api.Persistence;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.Modeling;
using TrustableCode.SDK.TrustableModeling.SideEffects;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class OrderingApiPersistenceTests
{
    [Fact]
    public void Ef_order_snapshot_store_should_save_and_load_order_snapshot()
    {
        using var db = CreateDbContext();
        var store = new EfOrderSnapshotStore(db);

        store.Save(new OrderPersistenceSnapshot(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 2)],
            Status: OrderStatus.PaidAwaitingFulfillment));

        var snapshot = store.Find("order-1");

        Assert.NotNull(snapshot);
        Assert.Equal(OrderStatus.PaidAwaitingFulfillment, snapshot.Status);
        Assert.Equal(new OrderLine("sku-1", 2), snapshot.Lines.Single());
    }

    [Fact]
    public void Ef_business_evidence_sink_should_persist_structured_evidence()
    {
        using var db = CreateDbContext();
        var sink = new EfBusinessEvidenceSink(db);

        sink.Record(new BusinessEvidence(
            name: "OrderRejectedEvidence",
            kind: EvidenceKind.BoundaryRejection,
            message: "Order was rejected.",
            correlationId: "corr-api-1",
            metadata: new Dictionary<string, string>
            {
                ["admission.name"] = "CreateOrderAdmission"
            }));

        var evidence = Assert.Single(db.BusinessEvidence);
        Assert.Equal("OrderRejectedEvidence", evidence.Name);
        Assert.Equal("BoundaryRejection", evidence.Kind);
        Assert.Contains("CreateOrderAdmission", evidence.MetadataJson);
    }

    [Fact]
    public void Orders_controller_should_create_order_and_persist_snapshot_outbox_and_evidence()
    {
        using var db = CreateDbContext();
        var orders = new EfOrderSnapshotStore(db);
        var outbox = new EfOrderingOutbox(db);
        var evidenceSink = new EfBusinessEvidenceSink(db);
        var lifecycleStore = new InMemorySideEffectLifecycleStore();
        var application = new OrderingApplicationService(evidenceSink, lifecycleStore);
        var persistedApplication = new PersistedOrderingApplicationService(
            orders,
            outbox,
            evidenceSink,
            lifecycleStore);
        var controller = new OrdersController(
            orders,
            outbox,
            application,
            persistedApplication);

        var response = controller.Create(new ExternalCreateOrderRequest(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 1)],
            RequestedStatus: null,
            CorrelationId: "corr-api-create-1"));

        var created = Assert.IsType<CreatedAtActionResult>(response.Result);
        var body = Assert.IsType<OperationResponse>(created.Value);
        Assert.True(body.Succeeded);
        Assert.Equal(OrderStatus.PlacedAwaitingPayment, orders.Find("order-1")?.Status);
        Assert.Equal("OrderCreated", Assert.Single(db.OutboxMessages).EventName);
        Assert.Equal("OrderCreatedEvidence", Assert.Single(db.BusinessEvidence).Name);
    }

    private static OrderingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OrderingDbContext(options);
    }
}
