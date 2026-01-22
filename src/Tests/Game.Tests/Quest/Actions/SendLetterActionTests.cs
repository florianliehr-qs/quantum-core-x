using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Actions;
using Xunit;

namespace Game.Tests.Quest.Actions;

public class SendLetterActionTests
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
    public async Task testExecuteAsync_givenTitleAndText_shouldSetLetterTitleFlag()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var action = new SendLetterAction { Title = "Quest Update", Text = "You have completed the task!" };

        await action.ExecuteAsync(context);

        state.GetStringFlag("_letter_title").Should().Be("Quest Update");
    }

    [Fact]
    public async Task testExecuteAsync_givenTitleAndText_shouldSetLetterTextFlag()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var action = new SendLetterAction { Title = "Important", Text = "Gather 10 iron ore" };

        await action.ExecuteAsync(context);

        state.GetStringFlag("_letter_text").Should().Be("Gather 10 iron ore");
    }

    [Fact]
    public async Task testExecuteAsync_givenTitleAndText_shouldSetHasLetterFlag()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var action = new SendLetterAction { Title = "Test", Text = "Test message" };

        await action.ExecuteAsync(context);

        state.GetBoolFlag("_has_letter").Should().BeTrue();
    }

    [Fact]
    public async Task testExecuteAsync_givenEmptyText_shouldStillSetFlags()
    {
        var state = new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() };
        var context = CreateContext(state: state);
        var action = new SendLetterAction { Title = "Notice", Text = "" };

        await action.ExecuteAsync(context);

        state.GetStringFlag("_letter_title").Should().Be("Notice");
        state.GetStringFlag("_letter_text").Should().Be("");
        state.GetBoolFlag("_has_letter").Should().BeTrue();
    }
}
