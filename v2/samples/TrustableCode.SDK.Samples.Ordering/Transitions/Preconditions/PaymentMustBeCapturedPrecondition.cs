using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class PaymentMustBeCapturedPrecondition : TransitionPrecondition<OrderStatus, CapturePaymentRequirement>
{
    public PaymentMustBeCapturedPrecondition()
        : base(
            code: "PaymentMustBeCaptured",
            description: "The payment provider must confirm capture before fulfillment can start.",
            isSatisfied: (_, requirement) => requirement.PaymentCaptured,
            rejectionReason: "Payment must be captured before the order can await fulfillment.")
    {
    }
}
