using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Actions;
using Xunit;

namespace Game.Tests.Quest.Actions;

public class SetStateActionTests
{
    private QuestActionContext CreateContext(IPlayerEntity? player = null, QuestState? state = null)
    {
        if (player == null)
        {
            player = Substitute.For<IPlayerEntity>();
            player.Player.Returns(new PlayerData { Id = 1, Name = "TestPlayer", Level = 1 });
        }

        return new QuestActionContext
        {
            Player = player,
            State = state ?? new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() },
            Services = Substitute.For<IServiceProvider>(),
            Logger = NullLogger.Instance
        };
    }

    [Fact]
    public async Task testExecuteAsync_givenNewState_shouldChangeState()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid(), CurrentState = "start" };
        var context = CreateContext(state: state);
        var action = new SetStateAction { NewState = "gathering" };

        await action.ExecuteAsync(context);

        state.CurrentState.Should().Be("gathering");
    }

    [Fact]
    public async Task testExecuteAsync_givenSameState_shouldKeepState()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid(), CurrentState = "active" };
        var context = CreateContext(state: state);
        var action = new SetStateAction { NewState = "active" };

        await action.ExecuteAsync(context);

        state.CurrentState.Should().Be("active");
    }

    [Fact]
    public async Task testExecuteAsync_givenCompletedState_shouldChangeToCompleted()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid(), CurrentState = "gathering" };
        var context = CreateContext(state: state);
        var action = new SetStateAction { NewState = "completed" };

        await action.ExecuteAsync(context);

        state.CurrentState.Should().Be("completed");
    }
}
