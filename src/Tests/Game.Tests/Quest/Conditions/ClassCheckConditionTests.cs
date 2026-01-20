using AwesomeAssertions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Conditions;
using Xunit;

namespace Game.Tests.Quest.Conditions;

public class ClassCheckConditionTests
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

    private PlayerData CreatePlayerData(EPlayerClass playerClass)
    {
        var genderedClass = playerClass switch
        {
            EPlayerClass.WARRIOR => EPlayerClassGendered.WARRIOR_MALE,
            EPlayerClass.NINJA => EPlayerClassGendered.NINJA_MALE,
            EPlayerClass.SURA => EPlayerClassGendered.SURA_MALE,
            EPlayerClass.SHAMAN => EPlayerClassGendered.SHAMAN_MALE,
            _ => EPlayerClassGendered.WARRIOR_MALE
        };

        return new PlayerData
        {
            Id = 1,
            Name = "TestPlayer",
            PlayerClass = genderedClass,
            Level = 1
        };
    }

    [Fact]
    public void testEvaluate_givenMatchingClass_shouldReturnTrue()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData(EPlayerClass.WARRIOR));
        var context = CreateContext(player: player);
        var condition = new ClassCheckCondition { Class = EPlayerClass.WARRIOR };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenNonMatchingClass_shouldReturnFalse()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData(EPlayerClass.NINJA));
        var context = CreateContext(player: player);
        var condition = new ClassCheckCondition { Class = EPlayerClass.WARRIOR };

        var result = condition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenSuraClass_shouldMatchSura()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData(EPlayerClass.SURA));
        var context = CreateContext(player: player);
        var condition = new ClassCheckCondition { Class = EPlayerClass.SURA };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenShamanClass_shouldMatchShaman()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData(EPlayerClass.SHAMAN));
        var context = CreateContext(player: player);
        var condition = new ClassCheckCondition { Class = EPlayerClass.SHAMAN };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }
}
