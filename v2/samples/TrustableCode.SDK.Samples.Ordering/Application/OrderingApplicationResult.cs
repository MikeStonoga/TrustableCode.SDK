using TrustableCode.SDK.TrustableModeling.SideEffects;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering;

public sealed record OrderingApplicationResult(
    string OperationName,
    Order? Order,
    OrderStatus? CurrentStatus,
    bool WasAccepted,
    TransitionExecutionStatus? TransitionStatus,
    IReadOnlyList<string> RejectionReasons,
    IReadOnlyList<string> ProducedEvents,
    IReadOnlyList<string> ProducedEvidence,
    SideEffectLifecycleRecord? SideEffectLifecycle = null)
{
    public bool Succeeded => WasAccepted && TransitionStatus is null or TransitionExecutionStatus.Applied or TransitionExecutionStatus.AlreadyApplied;

    public static OrderingApplicationResult Created(string operationName, Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderingApplicationResult(
            operationName,
            order,
            order.Status,
            WasAccepted: true,
            TransitionStatus: null,
            RejectionReasons: [],
            ProducedEvents: order.Events,
            ProducedEvidence: order.Evidence);
    }

    public static OrderingApplicationResult Rejected(string operationName, IEnumerable<string> rejectionReasons)
        => new(
            operationName,
            Order: null,
            CurrentStatus: null,
            WasAccepted: false,
            TransitionStatus: null,
            RejectionReasons: rejectionReasons.ToArray(),
            ProducedEvents: [],
            ProducedEvidence: []);

    public static OrderingApplicationResult FromTransition(
        string operationName,
        Order order,
        TransitionExecutionResult<OrderStatus> transition,
        SideEffectLifecycleRecord? sideEffectLifecycle = null)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(transition);

        return new OrderingApplicationResult(
            operationName,
            order,
            order.Status,
            WasAccepted: true,
            transition.Status,
            transition.RejectionReasons,
            transition.ProducedEvents,
            transition.ProducedEvidence,
            sideEffectLifecycle);
    }
}
