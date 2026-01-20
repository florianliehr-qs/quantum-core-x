using AwesomeAssertions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Conditions;
using Xunit;

namespace Game.Tests.Quest.Conditions;

public class QuestFlagEqConditionTests
{
    private QuestConditionContext CreateContext(IPlayerEntity? player = null, QuestState? state = null)
    {
        return new QuestConditionContext
        {
            Player = player ?? Substitute.For<IPlayerEntity>(),
            State = state ?? new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() },
            Services = Substitute.For<IServiceProvider>()
        };
    }

    [Fact]
    public void testEvaluate_givenFlagEqualsValue_shouldReturnTrue()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        state.SetIntFlag("stage", 2);
        var context = CreateContext(state: state);
        var condition = new QuestFlagEqCondition { Flag = "stage", Value = 2 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenFlagDoesNotEqualValue_shouldReturnFalse()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        state.SetIntFlag("stage", 1);
        var context = CreateContext(state: state);
        var condition = new QuestFlagEqCondition { Flag = "stage", Value = 2 };

        var result = condition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenNonExistentFlag_shouldReturnTrueForZeroValue()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var condition = new QuestFlagEqCondition { Flag = "missing", Value = 0 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenNonExistentFlag_shouldReturnFalseForNonZeroValue()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var condition = new QuestFlagEqCondition { Flag = "missing", Value = 5 };

        var result = condition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenNegativeFlag_shouldMatchNegativeValue()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        state.SetIntFlag("debt", -10);
        var context = CreateContext(state: state);
        var condition = new QuestFlagEqCondition { Flag = "debt", Value = -10 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }
}
