using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Actions;
using Xunit;

namespace Game.Tests.Quest.Actions;

public class CompleteQuestActionTests
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
    public async Task testExecuteAsync_givenActiveQuest_shouldMarkAsCompleted()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid(), IsCompleted = false };
        var context = CreateContext(state: state);
        var action = new CompleteQuestAction();

        await action.ExecuteAsync(context);

        state.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task testExecuteAsync_givenActiveQuest_shouldSetCompletedAt()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid(), CompletedAt = null };
        var context = CreateContext(state: state);
        var action = new CompleteQuestAction();
        var beforeExecution = DateTime.UtcNow;

        await action.ExecuteAsync(context);

        state.CompletedAt.Should().NotBeNull();
        state.CompletedAt!.Value.Should().BeOnOrAfter(beforeExecution);
    }

    [Fact]
    public async Task testExecuteAsync_givenAlreadyCompleted_shouldKeepCompleted()
    {
        var completedAt = DateTime.UtcNow.AddHours(-1);
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid(), IsCompleted = true, CompletedAt = completedAt };
        var context = CreateContext(state: state);
        var action = new CompleteQuestAction();

        await action.ExecuteAsync(context);

        state.IsCompleted.Should().BeTrue();
    }
}
