using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Api.Models;

public sealed record OperationResponse(
    string OperationName,
    bool Succeeded,
    bool WasAccepted,
    TransitionExecutionStatus? TransitionStatus,
    OrderResponse? Order,
    IReadOnlyList<string> RejectionReasons,
    IReadOnlyList<string> ProducedEvents,
    IReadOnlyList<string> ProducedEvidence)
{
    public static OperationResponse From(OrderingApplicationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new OperationResponse(
            result.OperationName,
            result.Succeeded,
            result.WasAccepted,
            result.TransitionStatus,
            result.Order is null ? null : OrderResponse.From(result.Order),
            result.RejectionReasons,
            result.ProducedEvents,
            result.ProducedEvidence);
    }
}
