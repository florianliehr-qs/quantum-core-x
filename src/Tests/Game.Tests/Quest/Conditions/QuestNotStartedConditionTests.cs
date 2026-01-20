using AwesomeAssertions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Conditions;
using Xunit;

namespace Game.Tests.Quest.Conditions;

public class QuestNotStartedConditionTests
{
    private QuestConditionContext CreateContext(IPlayerEntity? player = null, QuestState? state = null)
    {
        return new QuestConditionContext
        {
            Player = player ?? Substitute.For<IPlayerEntity>(),
            State = state ?? new QuestState { QuestId = "", PlayerId = Guid.NewGuid() },
            Services = Substitute.For<IServiceProvider>()
        };
    }

    [Fact]
    public void testEvaluate_givenEmptyQuestId_shouldReturnTrue()
    {
        var state = new QuestState { QuestId = "", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var condition = new QuestNotStartedCondition();

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenStartState_shouldReturnTrue()
    {
        var state = new QuestState { QuestId = "test_quest", CurrentState = "start", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var condition = new QuestNotStartedCondition();

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenActiveState_shouldReturnFalse()
    {
        var state = new QuestState { QuestId = "test_quest", CurrentState = "gathering", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var condition = new QuestNotStartedCondition();

        var result = condition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenCompletedState_shouldReturnFalse()
    {
        var state = new QuestState { QuestId = "test_quest", CurrentState = "completed", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var condition = new QuestNotStartedCondition();

        var result = condition.Evaluate(context);

        result.Should().BeFalse();
    }
}
