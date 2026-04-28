using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.Samples.Ordering.Api.Persistence;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class OrderingApiHttpTests
{
    [Fact]
    public async Task Swagger_document_should_be_available()
    {
        await using var factory = new OrderingApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.Equal(
            "TrustableCode Ordering API Sample",
            json.RootElement.GetProperty("info").GetProperty("title").GetString());
    }

    [Fact]
    public async Task Order_flow_should_create_and_fetch_order_over_http()
    {
        await using var factory = new OrderingApiFactory();
        using var client = factory.CreateClient();

        var create = await client.PostAsJsonAsync("/api/orders", new ExternalCreateOrderRequest(
            OrderId: "order-http-test-1",
            CustomerId: "customer-http-test-1",
            Lines: [new OrderLine("sku-1", 2)],
            RequestedStatus: null,
            CorrelationId: "corr-http-test-create-1"));

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        using var createJson = await JsonDocument.ParseAsync(await create.Content.ReadAsStreamAsync());
        Assert.Equal("created", createJson.RootElement.GetProperty("outcome").GetString());
        Assert.Equal(
            "PlacedAwaitingPayment",
            createJson.RootElement.GetProperty("order").GetProperty("status").GetString());

        var get = await client.GetAsync("/api/orders/order-http-test-1");

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        using var getJson = await JsonDocument.ParseAsync(await get.Content.ReadAsStreamAsync());
        Assert.Equal("order-http-test-1", getJson.RootElement.GetProperty("orderId").GetString());
        Assert.Equal("PlacedAwaitingPayment", getJson.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Rejected_admission_should_return_bad_request_with_clear_outcome()
    {
        await using var factory = new OrderingApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new ExternalCreateOrderRequest(
            OrderId: "order-http-rejected-1",
            CustomerId: "customer-http-test-1",
            Lines: [new OrderLine("sku-1", 1)],
            RequestedStatus: "Delivered",
            CorrelationId: "corr-http-test-rejected-1"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.Equal("admissionRejected", json.RootElement.GetProperty("outcome").GetString());
        Assert.Equal("admission", json.RootElement.GetProperty("failureStage").GetString());
        Assert.Contains(
            "application boundary",
            json.RootElement.GetProperty("message").GetString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task Rejected_transition_should_return_conflict_with_current_status()
    {
        await using var factory = new OrderingApiFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/orders", new ExternalCreateOrderRequest(
            OrderId: "order-http-conflict-1",
            CustomerId: "customer-http-test-1",
            Lines: [new OrderLine("sku-1", 1)],
            RequestedStatus: null,
            CorrelationId: "corr-http-test-create-1"));
        await client.PostAsJsonAsync(
            "/api/orders/order-http-conflict-1/capture-payment",
            new ExternalCapturePaymentRequest(
                PaymentCaptured: true,
                PaymentReference: "pay-http-test-1",
                RequestedStatus: null,
                CorrelationId: "corr-http-test-payment-1"));

        var response = await client.PostAsJsonAsync(
            "/api/orders/order-http-conflict-1/prepare-for-shipping",
            new ExternalPrepareOrderForShippingRequest(
                PaymentCaptured: true,
                StockReserved: false,
                RequestedStatus: "",
                CorrelationId: "corr-http-test-conflict-1"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.Equal("transitionRejected", json.RootElement.GetProperty("outcome").GetString());
        Assert.Equal("transition", json.RootElement.GetProperty("failureStage").GetString());
        Assert.Equal("PaidAwaitingFulfillment", json.RootElement.GetProperty("currentStatus").GetString());
    }

    private sealed class OrderingApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection.Open();

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<OrderingDbContext>>();
                services.AddDbContext<OrderingDbContext>(options =>
                    options.UseSqlite(_connection));

                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
                db.Database.EnsureCreated();
            });
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
