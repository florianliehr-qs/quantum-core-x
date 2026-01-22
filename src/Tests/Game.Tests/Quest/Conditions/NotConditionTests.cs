using AwesomeAssertions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Conditions;
using Xunit;

namespace Game.Tests.Quest.Conditions;

public class NotConditionTests
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
    public void testEvaluate_givenTrueCondition_shouldReturnFalse()
    {
        var context = CreateContext();
        var innerCondition = Substitute.For<IQuestCondition>();
        innerCondition.Evaluate(context).Returns(true);

        var notCondition = new NotCondition { Condition = innerCondition };

        var result = notCondition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenFalseCondition_shouldReturnTrue()
    {
        var context = CreateContext();
        var innerCondition = Substitute.For<IQuestCondition>();
        innerCondition.Evaluate(context).Returns(false);

        var notCondition = new NotCondition { Condition = innerCondition };

        var result = notCondition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenDoubleNegation_shouldReturnOriginalValue()
    {
        var context = CreateContext();
        var innerCondition = Substitute.For<IQuestCondition>();
        innerCondition.Evaluate(context).Returns(true);

        var notCondition1 = new NotCondition { Condition = innerCondition };
        var notCondition2 = new NotCondition { Condition = notCondition1 };

        var result = notCondition2.Evaluate(context);

        result.Should().BeTrue();
    }
}
