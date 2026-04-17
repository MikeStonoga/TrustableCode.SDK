using System.Diagnostics;
using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessActivitySourceSinkTests
{
    [Fact]
    public async Task WriteAsync_should_emit_activity_for_transition_evidence()
    {
        using var activityListener = new ActivityListener();
        Activity? capturedActivity = null;

        activityListener.ShouldListenTo = source => source.Name == "TrustableCode.SDK.Tests";
        activityListener.Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded;
        activityListener.ActivityStopped = activity => capturedActivity = activity;
        ActivitySource.AddActivityListener(activityListener);

        using var activitySource = new ActivitySource("TrustableCode.SDK.Tests");
        var sink = new BusinessActivitySourceSink(activitySource);
        var evidence = new BusinessTransitionEvidence<OrderStatus>(
            ModelName: "Order",
            TransitionName: "PrepareForShipping",
            PreviousState: OrderStatus.Paid,
            CurrentState: OrderStatus.ReadyForShipping,
            CorrelationId: "corr-321",
            ObservedAt: DateTimeOffset.UtcNow);

        await sink.WriteAsync(evidence);

        Assert.NotNull(capturedActivity);
        Assert.Equal("business.business-transition", capturedActivity!.OperationName);
        Assert.Equal("Order", capturedActivity.Tags.Single(x => x.Key == "business.model.name").Value);
        Assert.Equal("Paid", capturedActivity.Tags.Single(x => x.Key == "business.transition.from").Value);
        Assert.Equal("ReadyForShipping", capturedActivity.Tags.Single(x => x.Key == "business.transition.to").Value);
    }

    private enum OrderStatus
    {
        Paid = 1,
        ReadyForShipping = 2
    }
}
