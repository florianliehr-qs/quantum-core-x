using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Actions;
using Xunit;

namespace Game.Tests.Quest.Actions;

public class GiveExpActionTests
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
    public async Task testExecuteAsync_givenAmount_shouldCallAddPointWithExperience()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var context = CreateContext(player: player);
        var action = new GiveExpAction { Amount = 1000 };

        await action.ExecuteAsync(context);

        player.Received(1).AddPoint(EPoint.EXPERIENCE, 1000);
    }

    [Fact]
    public async Task testExecuteAsync_givenAmount_shouldCallSendPoints()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var context = CreateContext(player: player);
        var action = new GiveExpAction { Amount = 500 };

        await action.ExecuteAsync(context);

        player.Received(1).SendPoints();
    }

    [Fact]
    public async Task testExecuteAsync_givenLargeAmount_shouldHandleCorrectly()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var context = CreateContext(player: player);
        var action = new GiveExpAction { Amount = 999999 };

        await action.ExecuteAsync(context);

        player.Received(1).AddPoint(EPoint.EXPERIENCE, 999999);
    }
}
