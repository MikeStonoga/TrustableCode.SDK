using TrustableCode.SDK.Samples.Ordering.SideEffects;
using TrustableCode.SDK.TrustableModeling.Admission;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.SideEffects;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering;

/// <summary>
/// Application-layer example that composes admission, governed transitions, evidence, and side-effect lifecycle.
/// </summary>
public sealed class OrderingApplicationService
{
    private readonly OrderingEvidencePublisher _orderEvidencePublisher;
    private readonly BusinessEvidenceRecorder _evidenceRecorder;
    private readonly NotifyFulfillmentLifecycle _fulfillmentLifecycle;

    public OrderingApplicationService(
        IBusinessEvidenceSink evidenceSink,
        ISideEffectLifecycleStore sideEffectLifecycleStore)
    {
        ArgumentNullException.ThrowIfNull(evidenceSink);
        ArgumentNullException.ThrowIfNull(sideEffectLifecycleStore);

        _orderEvidencePublisher = new OrderingEvidencePublisher(evidenceSink);
        _evidenceRecorder = new BusinessEvidenceRecorder(evidenceSink);
        _fulfillmentLifecycle = new NotifyFulfillmentLifecycle(sideEffectLifecycleStore);
    }

    public OrderingApplicationResult CreateOrder(ExternalCreateOrderRequest request)
    {
        var admitted = OrderFactory.Create(request);
        if (!admitted.WasAccepted)
        {
            return Rejected("CreateOrder", admitted);
        }

        var order = admitted.Value!;
        _orderEvidencePublisher.Publish(order);

        return OrderingApplicationResult.Created("CreateOrder", order);
    }

    public OrderingApplicationResult CapturePayment(Order order, ExternalCapturePaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(order);

        var result = TrustableAdmissionFlow.ExecuteTransition(
            CapturePaymentAdmission.Create(),
            request,
            order.CapturePayment);

        return Complete("CapturePayment", order, result);
    }

    public OrderingApplicationResult PrepareForShipping(Order order, ExternalPrepareOrderForShippingRequest request)
    {
        ArgumentNullException.ThrowIfNull(order);

        var result = TrustableAdmissionFlow.ExecuteTransition(
            PrepareOrderForShippingAdmission.Create(),
            request,
            order.PrepareForShipping);

        var lifecycle = result.Transition?.Status == TransitionExecutionStatus.Applied
            ? PublishFulfillmentLifecycle(order, result.Admission.Value!.CorrelationId)
            : null;

        return Complete("PrepareForShipping", order, result, lifecycle);
    }

    public OrderingApplicationResult Ship(Order order, ExternalShipOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(order);

        var result = TrustableAdmissionFlow.ExecuteTransition(
            ShipOrderAdmission.Create(),
            request,
            order.Ship);

        return Complete("ShipOrder", order, result);
    }

    public OrderingApplicationResult Deliver(Order order, ExternalDeliverOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(order);

        var result = TrustableAdmissionFlow.ExecuteTransition(
            DeliverOrderAdmission.Create(),
            request,
            order.Deliver);

        return Complete("DeliverOrder", order, result);
    }

    public OrderingApplicationResult Cancel(Order order, ExternalCancelOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(order);

        var result = TrustableAdmissionFlow.ExecuteTransition(
            CancelOrderAdmission.Create(),
            request,
            order.Cancel);

        return Complete("CancelOrder", order, result);
    }

    private OrderingApplicationResult Rejected<TAccepted>(
        string operationName,
        AdmissionResult<TAccepted> admitted)
        where TAccepted : notnull
    {
        _evidenceRecorder.RecordMany(admitted.RejectionEvidence);
        return OrderingApplicationResult.Rejected(operationName, admitted.RejectionReasons);
    }

    private OrderingApplicationResult Complete<TAccepted>(
        string operationName,
        Order order,
        AdmittedTransitionResult<TAccepted, OrderStatus> result,
        SideEffectLifecycleRecord? sideEffectLifecycle = null)
        where TAccepted : notnull
    {
        if (!result.WasAccepted)
        {
            _evidenceRecorder.RecordMany(result.RejectionEvidence);
            return OrderingApplicationResult.Rejected(operationName, result.RejectionReasons);
        }

        _orderEvidencePublisher.Publish(order);

        return OrderingApplicationResult.FromTransition(
            operationName,
            order,
            result.Transition!,
            sideEffectLifecycle);
    }

    private SideEffectLifecycleRecord PublishFulfillmentLifecycle(Order order, string correlationId)
    {
        var notification = new FulfillmentNotification(order.OrderId, correlationId);
        return _fulfillmentLifecycle.PlanPersistAndPublish(notification, _evidenceRecorder);
    }
}
