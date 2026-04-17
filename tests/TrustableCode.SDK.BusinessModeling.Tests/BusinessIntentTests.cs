using TrustableCode.SDK.BusinessModeling.Boundaries;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessIntentTests
{
    [Fact]
    public void Business_intent_should_preserve_name_payload_and_correlation()
    {
        var payload = new PrepareOrderForShippingRequest("order-1");
        var intent = new BusinessIntent<PrepareOrderForShippingRequest>(
            Name: "PrepareOrderForShipping",
            Payload: payload,
            RequestedAt: new DateTimeOffset(2026, 4, 17, 20, 30, 0, TimeSpan.Zero),
            CorrelationId: "corr-789");

        Assert.Equal("PrepareOrderForShipping", intent.Name);
        Assert.Equal(payload, intent.Payload);
        Assert.Equal("corr-789", intent.CorrelationId);
    }

    private sealed record PrepareOrderForShippingRequest(string OrderNumber);
}
