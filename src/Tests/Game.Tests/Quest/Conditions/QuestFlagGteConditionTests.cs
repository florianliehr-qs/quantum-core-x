using AwesomeAssertions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Conditions;
using Xunit;

namespace Game.Tests.Quest.Conditions;

public class QuestFlagGteConditionTests
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
    public void testEvaluate_givenFlagGreaterThanValue_shouldReturnTrue()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        state.SetIntFlag("kills", 15);
        var context = CreateContext(state: state);
        var condition = new QuestFlagGteCondition { Flag = "kills", Value = 10 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenFlagEqualToValue_shouldReturnTrue()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        state.SetIntFlag("collected", 10);
        var context = CreateContext(state: state);
        var condition = new QuestFlagGteCondition { Flag = "collected", Value = 10 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenFlagLessThanValue_shouldReturnFalse()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        state.SetIntFlag("progress", 5);
        var context = CreateContext(state: state);
        var condition = new QuestFlagGteCondition { Flag = "progress", Value = 10 };

        var result = condition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenNonExistentFlag_shouldReturnFalseForPositiveValue()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var condition = new QuestFlagGteCondition { Flag = "missing", Value = 1 };

        var result = condition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenNonExistentFlag_shouldReturnTrueForZeroValue()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var condition = new QuestFlagGteCondition { Flag = "missing", Value = 0 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }
}
