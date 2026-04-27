using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class CapturePaymentTransition
{
    private readonly GovernedTransition<OrderStatus, CapturePaymentRequirement> _transition;

    public CapturePaymentTransition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _transition = new GovernedTransition<OrderStatus, CapturePaymentRequirement>(
            name: "CapturePayment",
            from: OrderStatus.PlacedAwaitingPayment,
            to: OrderStatus.PaidAwaitingFulfillment,
            currentState: () => order.Status,
            applyState: order.ApplyStatus,
            preconditions:
            [
                new PaymentMustBeCapturedPrecondition(),
                new PaymentReferenceRequiredPrecondition()
            ],
            producedEvents:
            [
                "OrderPaymentCaptured"
            ],
            producedEvidence:
            [
                "OrderPaymentCapturedEvidence"
            ],
            repetitionPolicy: TransitionRepetitionPolicy.TreatAsAlreadyApplied);
    }

    public TransitionExecutionResult<OrderStatus> Execute(CapturePaymentRequirement requirement)
        => _transition.Execute(requirement);
}
