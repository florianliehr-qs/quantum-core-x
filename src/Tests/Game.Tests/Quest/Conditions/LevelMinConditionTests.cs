using AwesomeAssertions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Conditions;
using Xunit;

namespace Game.Tests.Quest.Conditions;

public class LevelMinConditionTests
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

    private PlayerData CreatePlayerData(byte level)
    {
        return new PlayerData
        {
            Id = 1,
            Name = "TestPlayer",
            Level = level
        };
    }

    [Fact]
    public void testEvaluate_givenPlayerLevelGreaterThanMinimum_shouldReturnTrue()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData(10));
        var context = CreateContext(player: player);
        var condition = new LevelMinCondition { MinLevel = 5 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenPlayerLevelEqualToMinimum_shouldReturnTrue()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData(10));
        var context = CreateContext(player: player);
        var condition = new LevelMinCondition { MinLevel = 10 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenPlayerLevelLessThanMinimum_shouldReturnFalse()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData(3));
        var context = CreateContext(player: player);
        var condition = new LevelMinCondition { MinLevel = 5 };

        var result = condition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenLevel1PlayerAndMinLevel1_shouldReturnTrue()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData(1));
        var context = CreateContext(player: player);
        var condition = new LevelMinCondition { MinLevel = 1 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }
}
