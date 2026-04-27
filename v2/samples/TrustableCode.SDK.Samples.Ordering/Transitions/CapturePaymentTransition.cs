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
                new TransitionPrecondition<OrderStatus, CapturePaymentRequirement>(
                    code: "PaymentMustBeCaptured",
                    description: "The payment provider must confirm capture before fulfillment can start.",
                    isSatisfied: (_, requirement) => requirement.PaymentCaptured,
                    rejectionReason: "Payment must be captured before the order can await fulfillment."),
                new TransitionPrecondition<OrderStatus, CapturePaymentRequirement>(
                    code: "PaymentReferenceRequired",
                    description: "Captured payment must carry a provider reference.",
                    isSatisfied: (_, requirement) => !string.IsNullOrWhiteSpace(requirement.PaymentReference),
                    rejectionReason: "A payment reference is required after capture.")
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
