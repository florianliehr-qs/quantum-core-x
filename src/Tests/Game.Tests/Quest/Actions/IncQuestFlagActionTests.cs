using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Actions;
using Xunit;

namespace Game.Tests.Quest.Actions;

public class IncQuestFlagActionTests
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
    public async Task testExecuteAsync_givenExistingFlag_shouldIncrementByAmount()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        state.SetIntFlag("count", 5);
        var context = CreateContext(state: state);
        var action = new IncQuestFlagAction { Flag = "count", Amount = 3 };

        await action.ExecuteAsync(context);

        state.GetIntFlag("count").Should().Be(8);
    }

    [Fact]
    public async Task testExecuteAsync_givenNonExistentFlag_shouldSetToAmount()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var action = new IncQuestFlagAction { Flag = "new_counter", Amount = 10 };

        await action.ExecuteAsync(context);

        state.GetIntFlag("new_counter").Should().Be(10);
    }

    [Fact]
    public async Task testExecuteAsync_givenDefaultAmount_shouldIncrementByOne()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        state.SetIntFlag("kills", 3);
        var context = CreateContext(state: state);
        var action = new IncQuestFlagAction { Flag = "kills" };

        await action.ExecuteAsync(context);

        state.GetIntFlag("kills").Should().Be(4);
    }

    [Fact]
    public async Task testExecuteAsync_givenNegativeAmount_shouldDecrementFlag()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        state.SetIntFlag("health", 100);
        var context = CreateContext(state: state);
        var action = new IncQuestFlagAction { Flag = "health", Amount = -20 };

        await action.ExecuteAsync(context);

        state.GetIntFlag("health").Should().Be(80);
    }
}
