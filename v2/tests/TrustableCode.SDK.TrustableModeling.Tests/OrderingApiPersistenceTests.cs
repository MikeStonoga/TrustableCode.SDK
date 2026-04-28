using Microsoft.Data.Sqlite;
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
        using var connection = CreateOpenConnection();
        using var db = CreateDbContext(connection);
        var store = new EfOrderSnapshotStore(db);
        var unitOfWork = new EfOrderingUnitOfWork(db);

        store.Save(new OrderPersistenceSnapshot(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 2)],
            Status: OrderStatus.PaidAwaitingFulfillment));
        unitOfWork.Commit();

        using var reloadedDb = CreateDbContext(connection);
        var snapshot = new EfOrderSnapshotStore(reloadedDb).Find("order-1");

        Assert.NotNull(snapshot);
        Assert.Equal(OrderStatus.PaidAwaitingFulfillment, snapshot.Status);
        Assert.Equal(new OrderLine("sku-1", 2), snapshot.Lines.Single());
    }

    [Fact]
    public void Ef_business_evidence_sink_should_persist_structured_evidence()
    {
        using var connection = CreateOpenConnection();
        using var db = CreateDbContext(connection);
        var sink = new EfBusinessEvidenceSink(db);
        var unitOfWork = new EfOrderingUnitOfWork(db);

        sink.Record(new BusinessEvidence(
            name: "OrderRejectedEvidence",
            kind: EvidenceKind.BoundaryRejection,
            message: "Order was rejected.",
            correlationId: "corr-api-1",
            metadata: new Dictionary<string, string>
            {
                ["admission.name"] = "CreateOrderAdmission"
            }));
        unitOfWork.Commit();

        var evidence = Assert.Single(db.BusinessEvidence);
        Assert.Equal("OrderRejectedEvidence", evidence.Name);
        Assert.Equal("BoundaryRejection", evidence.Kind);
        Assert.Contains("CreateOrderAdmission", evidence.MetadataJson);
    }

    [Fact]
    public void Orders_controller_should_create_order_and_persist_snapshot_outbox_and_evidence()
    {
        using var connection = CreateOpenConnection();
        using var db = CreateDbContext(connection);
        var orders = new EfOrderSnapshotStore(db);
        var outbox = new EfOrderingOutbox(db);
        var unitOfWork = new EfOrderingUnitOfWork(db);
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
            unitOfWork,
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
        Assert.Equal("created", body.Outcome);
        Assert.Equal("CreateOrder completed successfully.", body.Message);
        Assert.Null(body.FailureStage);
        Assert.Equal(OrderStatus.PlacedAwaitingPayment, orders.Find("order-1")?.Status);
        Assert.Equal("OrderCreated", Assert.Single(db.OutboxMessages).EventName);
        Assert.Equal("OrderCreatedEvidence", Assert.Single(db.BusinessEvidence).Name);
    }

    [Fact]
    public void Orders_controller_should_commit_rejection_evidence_without_snapshot_or_outbox()
    {
        using var connection = CreateOpenConnection();
        using var db = CreateDbContext(connection);
        var orders = new EfOrderSnapshotStore(db);
        var outbox = new EfOrderingOutbox(db);
        var unitOfWork = new EfOrderingUnitOfWork(db);
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
            unitOfWork,
            application,
            persistedApplication);

        var response = controller.Create(new ExternalCreateOrderRequest(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 1)],
            RequestedStatus: "Delivered",
            CorrelationId: "corr-api-reject-1"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(response.Result);
        var body = Assert.IsType<OperationResponse>(badRequest.Value);
        Assert.False(body.Succeeded);
        Assert.Equal("admissionRejected", body.Outcome);
        Assert.Equal("admission", body.FailureStage);
        Assert.Contains("application boundary", body.Message);
        Assert.Empty(db.Orders);
        Assert.Empty(db.OutboxMessages);
        Assert.Equal("OrderCreationRejectedEvidence", Assert.Single(db.BusinessEvidence).Name);
    }

    [Fact]
    public void Orders_controller_should_explain_transition_conflicts()
    {
        using var connection = CreateOpenConnection();
        using var db = CreateDbContext(connection);
        var orders = new EfOrderSnapshotStore(db);
        var outbox = new EfOrderingOutbox(db);
        var unitOfWork = new EfOrderingUnitOfWork(db);
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
            unitOfWork,
            application,
            persistedApplication);

        controller.Create(new ExternalCreateOrderRequest(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 1)],
            RequestedStatus: null,
            CorrelationId: "corr-api-create-1"));
        controller.CapturePayment(
            "order-1",
            new ExternalCapturePaymentRequest(
                PaymentCaptured: true,
                PaymentReference: "pay-api-1",
                RequestedStatus: null,
                CorrelationId: "corr-api-payment-1"));

        var response = controller.PrepareForShipping(
            "order-1",
            new ExternalPrepareOrderForShippingRequest(
                PaymentCaptured: true,
                StockReserved: false,
                RequestedStatus: "",
                CorrelationId: "corr-api-reject-transition-1"));

        var conflict = Assert.IsType<ConflictObjectResult>(response.Result);
        var body = Assert.IsType<OperationResponse>(conflict.Value);
        Assert.False(body.Succeeded);
        Assert.Equal("transitionRejected", body.Outcome);
        Assert.Equal("transition", body.FailureStage);
        Assert.Contains("governed transition", body.Message);
        Assert.Equal("PaidAwaitingFulfillment", body.CurrentStatus);
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        return connection;
    }

    private static OrderingDbContext CreateDbContext(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new OrderingDbContext(options);
        db.Database.EnsureCreated();

        return db;
    }
}
