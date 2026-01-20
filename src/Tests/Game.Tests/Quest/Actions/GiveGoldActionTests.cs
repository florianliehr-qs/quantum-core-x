using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Actions;
using Xunit;

namespace Game.Tests.Quest.Actions;

public class GiveGoldActionTests
{
    private QuestActionContext CreateContext(IPlayerEntity? player = null, QuestState? state = null)
    {
        return new QuestActionContext
        {
            Player = player ?? Substitute.For<IPlayerEntity>(),
            State = state ?? new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() },
            Services = Substitute.For<IServiceProvider>(),
            Logger = NullLogger.Instance
        };
    }

    private PlayerData CreatePlayerData()
    {
        return new PlayerData
        {
            Id = 1,
            Name = "TestPlayer",
            Level = 1
        };
    }

    [Fact]
    public async Task testExecuteAsync_givenAmount_shouldCallAddPointWithGold()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var context = CreateContext(player: player);
        var action = new GiveGoldAction { Amount = 5000 };

        await action.ExecuteAsync(context);

        player.Received(1).AddPoint(EPoint.GOLD, 5000);
    }

    [Fact]
    public async Task testExecuteAsync_givenAmount_shouldCallSendPoints()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var context = CreateContext(player: player);
        var action = new GiveGoldAction { Amount = 1000 };

        await action.ExecuteAsync(context);

        player.Received(1).SendPoints();
    }

    [Fact]
    public async Task testExecuteAsync_givenSmallAmount_shouldHandleCorrectly()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var context = CreateContext(player: player);
        var action = new GiveGoldAction { Amount = 1 };

        await action.ExecuteAsync(context);

        player.Received(1).AddPoint(EPoint.GOLD, 1);
    }
}
