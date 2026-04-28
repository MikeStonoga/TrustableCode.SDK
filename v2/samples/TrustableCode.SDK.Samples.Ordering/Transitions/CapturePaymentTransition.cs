using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class CapturePaymentTransition
{
    private readonly GovernedTransition<OrderStatus, CapturePaymentRequirement> _transition;

    public CapturePaymentTransition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _transition = GovernedTransition<OrderStatus, CapturePaymentRequirement>
            .Create("CapturePayment")
            .From(OrderStatus.PlacedAwaitingPayment)
            .To(OrderStatus.PaidAwaitingFulfillment)
            .State(order.StatusState)
            .Require(new PaymentMustBeCapturedPrecondition())
            .Require(new PaymentReferenceRequiredPrecondition())
            .ProducesEvent("OrderPaymentCaptured")
            .ProducesEvidence("OrderPaymentCapturedEvidence")
            .TreatRepetitionAsAlreadyApplied()
            .Build();
    }

    public TransitionExecutionResult<OrderStatus> Execute(CapturePaymentRequirement requirement)
        => _transition.Execute(requirement);
}
