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
            $"{state.Name}: {state.Description}"));

        markdown.AppendLine("## Valid Transitions");
        AppendList(markdown, Model.Transitions.Select(transition =>
            $"{transition.Name}: {transition.FromState} -> {transition.ToState}. {transition.Description}"));

        markdown.AppendLine("## Invariants");
        AppendList(markdown, Model.Invariants.Select(invariant =>
            $"{invariant.Code} ({invariant.Severity}): {invariant.Description}"));

        markdown.AppendLine("## Boundary Rules");
        AppendList(markdown, Model.Boundaries.Select(boundary =>
            $"{boundary.Name}: {boundary.Description}"));

        markdown.AppendLine("## Side Effects");
        AppendList(markdown, Model.SideEffects.Select(sideEffect =>
            $"{sideEffect.Name}: {sideEffect.Description} [{sideEffect.Consistency}]"));

        markdown.AppendLine("## Evidence");
        AppendList(markdown, Model.Evidence.Select(evidence =>
            $"{evidence.Name} ({evidence.Kind}): {evidence.Description}"));

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
}

