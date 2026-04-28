using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Api.Models;

public sealed record OperationResponse(
    string OperationName,
    string Outcome,
    string Message,
    bool Succeeded,
    bool WasAccepted,
    string? FailureStage,
    string? CurrentStatus,
    TransitionExecutionStatus? TransitionStatus,
    OrderResponse? Order,
    SideEffectLifecycleResponse? SideEffectLifecycle,
    IReadOnlyList<string> RejectionReasons,
    IReadOnlyList<string> ProducedEvents,
    IReadOnlyList<string> ProducedEvidence)
{
    public static OperationResponse From(OrderingApplicationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new OperationResponse(
            result.OperationName,
            OutcomeFrom(result),
            MessageFrom(result),
            result.Succeeded,
            result.WasAccepted,
            FailureStageFrom(result),
            result.CurrentStatus?.ToString(),
            result.TransitionStatus,
            result.Order is null ? null : OrderResponse.From(result.Order),
            result.SideEffectLifecycle is null ? null : SideEffectLifecycleResponse.From(result.SideEffectLifecycle),
            result.RejectionReasons,
            result.ProducedEvents,
            result.ProducedEvidence);
    }

    private static string OutcomeFrom(OrderingApplicationResult result)
    {
        if (!result.WasAccepted)
        {
            return "admissionRejected";
        }

        if (!result.Succeeded)
        {
            return "transitionRejected";
        }

        return result.TransitionStatus switch
        {
            null => "created",
            TransitionExecutionStatus.AlreadyApplied => "alreadyApplied",
            _ => "applied"
        };
    }

    private static string MessageFrom(OrderingApplicationResult result)
    {
        if (result.Succeeded)
        {
            return result.TransitionStatus == TransitionExecutionStatus.AlreadyApplied
                ? $"{result.OperationName} was already applied."
                : $"{result.OperationName} completed successfully.";
        }

        var reason = result.RejectionReasons.FirstOrDefault() ?? "No rejection reason was provided.";
        return !result.WasAccepted
            ? $"{result.OperationName} was rejected at the application boundary: {reason}"
            : $"{result.OperationName} was accepted but rejected by the governed transition: {reason}";
    }

    private static string? FailureStageFrom(OrderingApplicationResult result)
    {
        if (result.Succeeded)
        {
            return null;
        }

        return result.WasAccepted ? "transition" : "admission";
    }
}
