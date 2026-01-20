using AwesomeAssertions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Conditions;
using Xunit;

namespace Game.Tests.Quest.Conditions;

public class AndConditionTests
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
    public void testEvaluate_givenAllConditionsTrue_shouldReturnTrue()
    {
        var context = CreateContext();
        var condition1 = Substitute.For<IQuestCondition>();
        var condition2 = Substitute.For<IQuestCondition>();
        condition1.Evaluate(context).Returns(true);
        condition2.Evaluate(context).Returns(true);

        var andCondition = new AndCondition { Conditions = [condition1, condition2] };

        var result = andCondition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenOneConditionFalse_shouldReturnFalse()
    {
        var context = CreateContext();
        var condition1 = Substitute.For<IQuestCondition>();
        var condition2 = Substitute.For<IQuestCondition>();
        condition1.Evaluate(context).Returns(true);
        condition2.Evaluate(context).Returns(false);

        var andCondition = new AndCondition { Conditions = [condition1, condition2] };

        var result = andCondition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenAllConditionsFalse_shouldReturnFalse()
    {
        var context = CreateContext();
        var condition1 = Substitute.For<IQuestCondition>();
        var condition2 = Substitute.For<IQuestCondition>();
        condition1.Evaluate(context).Returns(false);
        condition2.Evaluate(context).Returns(false);

        var andCondition = new AndCondition { Conditions = [condition1, condition2] };

        var result = andCondition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenEmptyConditionList_shouldReturnTrue()
    {
        var context = CreateContext();
        var andCondition = new AndCondition { Conditions = [] };

        var result = andCondition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenThreeConditionsAllTrue_shouldReturnTrue()
    {
        var context = CreateContext();
        var condition1 = Substitute.For<IQuestCondition>();
        var condition2 = Substitute.For<IQuestCondition>();
        var condition3 = Substitute.For<IQuestCondition>();
        condition1.Evaluate(context).Returns(true);
        condition2.Evaluate(context).Returns(true);
        condition3.Evaluate(context).Returns(true);

        var andCondition = new AndCondition { Conditions = [condition1, condition2, condition3] };

        var result = andCondition.Evaluate(context);

        result.Should().BeTrue();
    }
}
