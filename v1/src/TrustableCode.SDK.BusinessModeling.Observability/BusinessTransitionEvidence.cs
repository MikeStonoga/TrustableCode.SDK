namespace TrustableCode.SDK.BusinessModeling.Observability;

/// <summary>
/// Non-generic view of business transition evidence for adapters and sinks.
/// </summary>
public interface IBusinessTransitionEvidence : IBusinessEvidence
{
    /// <summary>
    /// Gets the name of the business model that transitioned.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Gets the business meaning of the transition.
    /// </summary>
    string TransitionName { get; }

    /// <summary>
    /// Gets the previous state rendered for sinks and instrumentation.
    /// </summary>
    string PreviousStateText { get; }

    /// <summary>
    /// Gets the current state rendered for sinks and instrumentation.
    /// </summary>
    string CurrentStateText { get; }

    /// <summary>
    /// Gets the correlation identifier associated with the transition, if any.
    /// </summary>
    string? CorrelationId { get; }
}

/// <summary>
/// Observable evidence that a business model changed from one meaningful state to another.
/// Unlike a business event, which communicates a business fact for other parts of the system to react to,
/// transition evidence exists to make the change observable, auditable, and diagnosable after it happened.
/// </summary>
public record BusinessTransitionEvidence<TState>(
    string ModelName,
    string TransitionName,
    TState PreviousState,
    TState CurrentState,
    string? CorrelationId,
    DateTimeOffset ObservedAt) : IBusinessTransitionEvidence
{
    /// <summary>
    /// Stable classifier for business transition evidence.
    /// </summary>
    public string EvidenceType => "business-transition";

    /// <summary>
    /// Gets the previous state rendered for instrumentation and logs.
    /// </summary>
    public string PreviousStateText => PreviousState?.ToString() ?? string.Empty;

    /// <summary>
    /// Gets the current state rendered for instrumentation and logs.
    /// </summary>
    public string CurrentStateText => CurrentState?.ToString() ?? string.Empty;
}
