using TrustableCode.SDK.TrustableModeling.Admission;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.Invariants;
using TrustableCode.SDK.TrustableModeling.SideEffects;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Testing;

/// <summary>
/// Framework-neutral checks for trustable model behavior.
/// </summary>
public static class TrustableChecks
{
    public static TrustableCheck TransitionApplied<TState>(
        TransitionExecutionResult<TState> result,
        TState expectedState,
        IEnumerable<string>? producedEvents = null,
        IEnumerable<string>? producedEvidence = null)
        where TState : notnull
    {
        ArgumentNullException.ThrowIfNull(result);

        var failures = new List<string>();
        if (result.Status != TransitionExecutionStatus.Applied)
        {
            failures.Add($"Expected transition '{result.TransitionName}' to be Applied, but it was {result.Status}.");
        }

        if (!EqualityComparer<TState>.Default.Equals(result.CurrentState, expectedState))
        {
            failures.Add($"Expected current state '{expectedState}', but it was '{result.CurrentState}'.");
        }

        AddMissing("produced event", result.ProducedEvents, producedEvents, failures);
        AddMissing("produced evidence", result.ProducedEvidence, producedEvidence, failures);

        return new TrustableCheck(failures);
    }

    public static TrustableCheck TransitionRejected<TState>(
        TransitionExecutionResult<TState> result,
        IEnumerable<string>? rejectionReasons = null,
        IEnumerable<string>? rejectionEvidence = null)
        where TState : notnull
    {
        ArgumentNullException.ThrowIfNull(result);

        var failures = new List<string>();
        if (result.Status != TransitionExecutionStatus.Rejected)
        {
            failures.Add($"Expected transition '{result.TransitionName}' to be Rejected, but it was {result.Status}.");
        }

        AddMissing("rejection reason", result.RejectionReasons, rejectionReasons, failures);
        AddMissing(
            "rejection evidence",
            result.RejectionEvidence.Select(evidence => evidence.Name),
            rejectionEvidence,
            failures);

        return new TrustableCheck(failures);
    }

    public static TrustableCheck AdmissionAccepted<TAccepted>(AdmissionResult<TAccepted> result)
        where TAccepted : notnull
    {
        ArgumentNullException.ThrowIfNull(result);

        var failures = new List<string>();
        if (!result.WasAccepted)
        {
            failures.Add($"Expected admission to be accepted, but it was rejected: {string.Join("; ", result.RejectionReasons)}");
        }

        if (result.Value is null)
        {
            failures.Add("Expected accepted admission to carry a value.");
        }

        return new TrustableCheck(failures);
    }

    public static TrustableCheck AdmissionRejected<TAccepted>(
        AdmissionResult<TAccepted> result,
        IEnumerable<string>? rejectionReasons = null,
        IEnumerable<string>? rejectionEvidence = null)
        where TAccepted : notnull
    {
        ArgumentNullException.ThrowIfNull(result);

        var failures = new List<string>();
        if (result.WasAccepted)
        {
            failures.Add("Expected admission to be rejected, but it was accepted.");
        }

        AddMissing("rejection reason", result.RejectionReasons, rejectionReasons, failures);
        AddMissing(
            "rejection evidence",
            result.RejectionEvidence.Select(evidence => evidence.Name),
            rejectionEvidence,
            failures);

        return new TrustableCheck(failures);
    }

    public static TrustableCheck InvariantPreserved(InvariantEvaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        return evaluation.IsPreserved
            ? TrustableCheck.Pass()
            : TrustableCheck.Fail($"Expected invariant '{evaluation.Code}' to be preserved, but it was violated: {evaluation.Message}");
    }

    public static TrustableCheck InvariantViolated(InvariantEvaluation evaluation, string? expectedCode = null)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var failures = new List<string>();
        if (evaluation.IsPreserved)
        {
            failures.Add($"Expected invariant '{evaluation.Code}' to be violated, but it was preserved.");
        }

        if (!string.IsNullOrWhiteSpace(expectedCode)
            && !string.Equals(evaluation.Code, expectedCode, StringComparison.Ordinal))
        {
            failures.Add($"Expected invariant code '{expectedCode}', but it was '{evaluation.Code}'.");
        }

        return new TrustableCheck(failures);
    }

    public static TrustableCheck SideEffectStatus(
        SideEffectExecutionResult result,
        SideEffectExecutionStatus expectedStatus,
        string? expectedEvidenceName = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        var failures = new List<string>();
        if (result.Status != expectedStatus)
        {
            failures.Add($"Expected side effect '{result.SideEffectName}' status {expectedStatus}, but it was {result.Status}.");
        }

        if (!string.IsNullOrWhiteSpace(expectedEvidenceName)
            && !string.Equals(result.Evidence.Name, expectedEvidenceName, StringComparison.Ordinal))
        {
            failures.Add($"Expected side effect evidence '{expectedEvidenceName}', but it was '{result.Evidence.Name}'.");
        }

        return new TrustableCheck(failures);
    }

    public static TrustableCheck EvidenceNamed(BusinessEvidence evidence, string expectedName)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        expectedName = Require.Text(expectedName, nameof(expectedName));

        return string.Equals(evidence.Name, expectedName, StringComparison.Ordinal)
            ? TrustableCheck.Pass()
            : TrustableCheck.Fail($"Expected evidence '{expectedName}', but it was '{evidence.Name}'.");
    }

    private static void AddMissing(
        string label,
        IEnumerable<string> actual,
        IEnumerable<string>? expected,
        ICollection<string> failures)
    {
        if (expected is null)
        {
            return;
        }

        var actualSet = actual.ToHashSet(StringComparer.Ordinal);
        foreach (var item in expected)
        {
            if (!actualSet.Contains(item))
            {
                failures.Add($"Expected {label} '{item}' was not found.");
            }
        }
    }
}
