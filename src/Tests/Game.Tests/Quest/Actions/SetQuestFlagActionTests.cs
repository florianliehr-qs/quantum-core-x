using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Persistence.Entities;
using QuantumCore.Game.Quest.Actions;
using Xunit;

namespace Game.Tests.Quest.Actions;

public class SetQuestFlagActionTests
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
    public async Task testExecuteAsync_givenNewFlag_shouldSetFlagValue()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var action = new SetQuestFlagAction { Flag = "test_flag", Value = 42 };

        await action.ExecuteAsync(context);

        state.GetIntFlag("test_flag").Should().Be(42);
    }

    [Fact]
    public async Task testExecuteAsync_givenExistingFlag_shouldUpdateFlagValue()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        state.SetIntFlag("test_flag", 10);
        var context = CreateContext(state: state);
        var action = new SetQuestFlagAction { Flag = "test_flag", Value = 99 };

        await action.ExecuteAsync(context);

        state.GetIntFlag("test_flag").Should().Be(99);
    }

    [Fact]
    public async Task testExecuteAsync_givenZeroValue_shouldSetFlagToZero()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var action = new SetQuestFlagAction { Flag = "test_flag", Value = 0 };

        await action.ExecuteAsync(context);

        state.GetIntFlag("test_flag").Should().Be(0);
    }

    [Fact]
    public async Task testExecuteAsync_givenNegativeValue_shouldSetFlagToNegative()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var action = new SetQuestFlagAction { Flag = "test_flag", Value = -5 };

        await action.ExecuteAsync(context);

        state.GetIntFlag("test_flag").Should().Be(-5);
    }
}
