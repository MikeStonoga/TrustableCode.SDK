using System.Text;
using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.AgentContext;

/// <summary>
/// A compact semantic context packet for AI agents and reviewers before changing important code.
/// </summary>
public sealed record AgentContextPacket(TrustableModelDescriptor Model)
{
    public static AgentContextPacket From(TrustableModelDescriptor model)
        => new(model ?? throw new ArgumentNullException(nameof(model)));

    public string ToMarkdown()
    {
        var markdown = new StringBuilder();

        markdown.AppendLine($"# Trustable Context: {Model.Name}");
        markdown.AppendLine();
        markdown.AppendLine("## Business Purpose");
        markdown.AppendLine(Model.BusinessPurpose);
        markdown.AppendLine();

        markdown.AppendLine("## State Model");
        markdown.AppendLine($"Authoritative state: `{Model.StateModel.AuthoritativeState}`");
        AppendList(markdown, Model.StateModel.States.Select(state =>
            $"{state.Name}{TerminalMarker(state)}: {state.Description}"));

        markdown.AppendLine("## Suggested Reading Order");
        AppendList(markdown,
        [
            $"Start with `{Model.StateModel.AuthoritativeState}` and the declared states.",
            "Read valid transitions before changing state mutation code.",
            "Read boundary rules before accepting external input.",
            "Read invariants before adding or relaxing behavior.",
            "Read side effects and evidence before publishing, logging, tracing, or compensating work."
        ]);

        markdown.AppendLine("## Expected State Flow");
        AppendList(markdown, Model.Transitions.Select(transition =>
            $"{transition.FromState} -> {transition.ToState} via `{transition.Name}`"));

        markdown.AppendLine("## Valid Transitions");
        AppendTransitionDetails(markdown);

        markdown.AppendLine("## Invariants");
        AppendList(markdown, Model.Invariants.Select(invariant =>
            $"{invariant.Code} ({invariant.Severity}): {invariant.Description}"));

        markdown.AppendLine("## Boundary Rules");
        AppendBoundaryDetails(markdown);

        markdown.AppendLine("## Application Entry Points");
        AppendApplicationEntryPointDetails(markdown);

        markdown.AppendLine("## Side Effects");
        AppendList(markdown, Model.SideEffects.Select(sideEffect =>
            $"{sideEffect.Name}: {sideEffect.Description} [{sideEffect.Consistency}; idempotency={sideEffect.RequiresIdempotencyKey}; compensation={sideEffect.RequiresCompensation}]"));

        markdown.AppendLine("## Evidence");
        AppendList(markdown, Model.Evidence.Select(evidence =>
            $"{evidence.Name} ({evidence.Kind}): {evidence.Description}"));

        markdown.AppendLine("## Rejection And Observation Map");
        AppendList(markdown, Model.Boundaries.SelectMany(boundary =>
            boundary.RejectionEvidence.Select(evidence => $"{boundary.Name} rejects with `{evidence}`")));

        markdown.AppendLine("## Change Checklist");
        AppendList(markdown,
        [
            "Do not introduce a state change that is missing from Valid Transitions.",
            "Do not admit external input without a Boundary Rule and rejection evidence.",
            "Preserve or explicitly replace every listed invariant.",
            "Keep side effects idempotent when the descriptor requires an idempotency key.",
            "Emit or preserve declared evidence for accepted, rejected, and side-effect behavior."
        ]);

        markdown.AppendLine("## Non-Goals");
        AppendList(markdown, Model.NonGoals);

        return markdown.ToString();
    }

    private static void AppendList(StringBuilder markdown, IEnumerable<string> items)
    {
        var materialized = items.ToArray();
        if (materialized.Length == 0)
        {
            markdown.AppendLine("- Not declared yet.");
            markdown.AppendLine();
            return;
        }

        foreach (var item in materialized)
        {
            markdown.AppendLine($"- {item}");
        }

        markdown.AppendLine();
    }

    private void AppendTransitionDetails(StringBuilder markdown)
    {
        foreach (var transition in Model.Transitions)
        {
            markdown.AppendLine($"- {transition.Name}: {transition.FromState} -> {transition.ToState}. {transition.Description}");
            AppendNestedList(markdown, "Preconditions", transition.Preconditions);
            AppendNestedList(markdown, "Produced events", transition.ProducedEvents);
            AppendNestedList(markdown, "Produced evidence", transition.ProducedEvidence);
        }

        if (Model.Transitions.Count == 0)
        {
            markdown.AppendLine("- Not declared yet.");
        }

        markdown.AppendLine();
    }

    private void AppendBoundaryDetails(StringBuilder markdown)
    {
        foreach (var boundary in Model.Boundaries)
        {
            markdown.AppendLine($"- {boundary.Name}: {boundary.Description}");
            AppendNestedList(markdown, "Admission rules", boundary.AdmissionRules);
            AppendNestedList(markdown, "Rejection evidence", boundary.RejectionEvidence);
        }

        if (Model.Boundaries.Count == 0)
        {
            markdown.AppendLine("- Not declared yet.");
        }

        markdown.AppendLine();
    }

    private void AppendApplicationEntryPointDetails(StringBuilder markdown)
    {
        foreach (var entryPoint in Model.ApplicationEntryPoints)
        {
            markdown.AppendLine($"- {entryPoint.Name}: {entryPoint.Description}");
            markdown.AppendLine($"  - Use when: {entryPoint.WhenToUse}");
            AppendNestedList(markdown, "Reads", entryPoint.Reads);
            AppendNestedList(markdown, "Writes", entryPoint.Writes);
            AppendNestedList(markdown, "Emits", entryPoint.Emits);
        }

        if (Model.ApplicationEntryPoints.Count == 0)
        {
            markdown.AppendLine("- Not declared yet.");
        }

        markdown.AppendLine();
    }

    private static void AppendNestedList(StringBuilder markdown, string label, IReadOnlyList<string> items)
    {
        if (items.Count == 0)
        {
            return;
        }

        markdown.AppendLine($"  - {label}: {string.Join(", ", items.Select(item => $"`{item}`"))}");
    }

    private static string TerminalMarker(StateDefinition state)
        => state.IsTerminal ? " (terminal)" : state.IsInitial ? " (initial)" : string.Empty;
}
