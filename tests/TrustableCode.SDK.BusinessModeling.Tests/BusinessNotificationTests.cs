using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessNotificationTests
{
    [Fact]
    public void CollectIf_does_not_evaluate_rule_factory_when_condition_is_false()
    {
        var notification = new BusinessNotification();

        notification.CollectIf(false, () => throw new InvalidOperationException("should not run"));

        Assert.False(notification.HasViolations);
    }

    [Fact]
    public void ThrowIfAny_does_nothing_when_no_violation_was_collected()
    {
        var notification = new BusinessNotification()
            .Collect(new PassingRule());

        var exception = Record.Exception(notification.ThrowIfAny);

        Assert.Null(exception);
    }

    [Fact]
    public void ThrowIfAny_throws_aggregated_exception_with_collected_messages()
    {
        var notification = new BusinessNotification()
            .Collect(new FailingRule("First broken rule."))
            .Collect(new FailingRule("Second broken rule."));

        var exception = Assert.Throws<AggregatedBusinessRuleViolationException>(notification.ThrowIfAny);

        Assert.Equal(2, exception.Violations.Count);
        Assert.Equal("First broken rule.", exception.Violations[0]);
    }

    private sealed class PassingRule : IBusinessInvariantRule
    {
        public string Description => "Rule passes.";

        public void EnsureIsPreserved()
        {
        }
    }

    private sealed class FailingRule(string message) : IBusinessInvariantRule
    {
        public string Description => message;

        public void EnsureIsPreserved()
        {
            throw new BusinessRuleViolationException(message);
        }
    }
}
