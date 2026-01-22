using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Persistence.Extensions;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Game.Tests;

public class DbQuestRepositoryTests : IAsyncLifetime
{
    private readonly IDbQuestRepository _questRepository;
    private readonly GameDbContext _dbContext;
    private readonly AsyncServiceScope _scope;
    private const uint TEST_PLAYER_ID = 999999;
    private const string TEST_QUEST_ID = "test_quest_integration";

    public DbQuestRepositoryTests(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection()
            .AddLogging(cfg => cfg
                .ClearProviders()
                .AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(outputHelper)
                    .CreateLogger()))
            .AddGameDatabase()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .Configure<DatabaseOptions>(HostingOptions.MODE_GAME, opts =>
            {
                opts.Provider = DatabaseProvider.SQLITE;
                opts.ConnectionString = "Data Source=:memory:";
            })
            .BuildServiceProvider();

        _scope = services.CreateAsyncScope();
        _questRepository = _scope.ServiceProvider.GetRequiredService<IDbQuestRepository>();
        _dbContext = _scope.ServiceProvider.GetRequiredService<GameDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.OpenConnectionAsync();
        await _dbContext.Database.MigrateAsync();

        // Create a test player for foreign key constraints
        var testPlayer = new QuantumCore.Game.Persistence.Entities.Player
        {
            Id = TEST_PLAYER_ID,
            AccountId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Empire = QuantumCore.API.Game.Types.EEmpire.SHINSOO,
            PlayerClass = 0,
            SkillGroup = 0,
            PlayTime = 0,
            Level = 1,
            Experience = 0,
            Gold = 0,
            St = 0,
            Ht = 0,
            Dx = 0,
            Iq = 0,
            PositionX = 0,
            PositionY = 0,
            Health = 100,
            Mana = 100,
            BodyPart = 0,
            HairPart = 0,
            Name = "TestPlayer",
            Stamina = 0,
            AvailableSkillPoints = 0,
            AvailableStatusPoints = 0,
            GivenStatusPoints = 0
        };
        _dbContext.Players.Add(testPlayer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up all test data
        var quests = _dbContext.PlayerQuests.Where(q => q.PlayerId == TEST_PLAYER_ID);
        _dbContext.PlayerQuests.RemoveRange(quests);
        await _dbContext.SaveChangesAsync();
        await _scope.DisposeAsync();
    }

    [Fact]
    public async Task testSaveQuestState_givenNewQuest_shouldCreateDatabaseRecord()
    {
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID,
            CurrentState = "start",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        questState.SetIntFlag("kills", 5);
        questState.SetStringFlag("target", "wolf");
        questState.SetBoolFlag("tutorial_shown", true);

        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID);
        loaded.Should().NotBeNull();
    }

    [Fact]
    public async Task testGetQuestState_givenExistingQuest_shouldReturnCorrectState()
    {
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_get",
            CurrentState = "gathering",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        questState.SetIntFlag("ore_count", 10);
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_get");

        loaded!.CurrentState.Should().Be("gathering");
    }

    [Fact]
    public async Task testGetQuestState_givenNonExistentQuest_shouldReturnNull()
    {
        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, "nonexistent_quest");

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task testSaveQuestState_givenExistingQuest_shouldUpdateRecord()
    {
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_update",
            CurrentState = "start",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        questState.SetIntFlag("progress", 0);
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        questState.CurrentState = "completed";
        questState.IsCompleted = true;
        questState.CompletedAt = DateTime.UtcNow;
        questState.SetIntFlag("progress", 100);
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_update");
        loaded!.CurrentState.Should().Be("completed");
    }

    [Fact]
    public async Task testSaveQuestState_givenIntFlags_shouldPersistCorrectly()
    {
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_intflags",
            CurrentState = "start",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        questState.SetIntFlag("kills", 15);
        questState.SetIntFlag("deaths", 3);
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_intflags");

        loaded!.GetIntFlag("kills").Should().Be(15);
    }

    [Fact]
    public async Task testSaveQuestState_givenStringFlags_shouldPersistCorrectly()
    {
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_stringflags",
            CurrentState = "start",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        questState.SetStringFlag("location", "village");
        questState.SetStringFlag("npc_name", "Blacksmith");
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_stringflags");

        loaded!.GetStringFlag("location").Should().Be("village");
    }

    [Fact]
    public async Task testSaveQuestState_givenBoolFlags_shouldPersistCorrectly()
    {
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_boolflags",
            CurrentState = "start",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        questState.SetBoolFlag("found_secret", true);
        questState.SetBoolFlag("failed_challenge", false);
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_boolflags");

        loaded!.GetBoolFlag("found_secret").Should().BeTrue();
    }

    [Fact]
    public async Task testSaveQuestState_givenMultipleFlagTypes_shouldPersistAll()
    {
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_multiflags",
            CurrentState = "start",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        questState.SetIntFlag("count", 42);
        questState.SetStringFlag("name", "test");
        questState.SetBoolFlag("active", true);
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_multiflags");

        loaded!.IntFlags.Count.Should().Be(1);
    }

    [Fact]
    public async Task testGetPlayerQuests_givenMultipleQuests_shouldReturnAllQuests()
    {
        // Create multiple quests
        var quest1 = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_multi1",
            CurrentState = "start",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        var quest2 = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_multi2",
            CurrentState = "completed",
            StartedAt = DateTime.UtcNow,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, quest1);
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, quest2);

        var allQuests = await _questRepository.GetPlayerQuestsAsync(TEST_PLAYER_ID);

        allQuests.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task testGetActiveQuests_givenMixedQuests_shouldReturnOnlyActiveQuests()
    {
        var activeQuest = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_active",
            CurrentState = "gathering",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        var completedQuest = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_completed",
            CurrentState = "done",
            StartedAt = DateTime.UtcNow,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, activeQuest);
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, completedQuest);

        var activeQuests = await _questRepository.GetActiveQuestsAsync(TEST_PLAYER_ID);

        activeQuests.Should().Contain(q => q.QuestId == TEST_QUEST_ID + "_active");
    }

    [Fact]
    public async Task testGetCompletedQuests_givenMixedQuests_shouldReturnOnlyCompletedQuests()
    {
        var activeQuest = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_active2",
            CurrentState = "gathering",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        var completedQuest = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_completed2",
            CurrentState = "done",
            StartedAt = DateTime.UtcNow,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, activeQuest);
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, completedQuest);

        var completedQuests = await _questRepository.GetCompletedQuestsAsync(TEST_PLAYER_ID);

        completedQuests.Should().Contain(q => q.QuestId == TEST_QUEST_ID + "_completed2");
    }

    [Fact]
    public async Task testDeleteQuestState_givenExistingQuest_shouldReturnTrueAndRemoveQuest()
    {
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_delete",
            CurrentState = "start",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        var result = await _questRepository.DeleteQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_delete");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task testDeleteQuestState_givenNonExistentQuest_shouldReturnFalse()
    {
        var result = await _questRepository.DeleteQuestStateAsync(TEST_PLAYER_ID, "nonexistent_delete");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task testDeleteQuestState_givenDeletedQuest_shouldNotBeRetrievable()
    {
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_delete2",
            CurrentState = "start",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);
        await _questRepository.DeleteQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_delete2");

        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_delete2");

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task testSaveQuestState_givenEmptyFlags_shouldNotCrash()
    {
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_empty",
            CurrentState = "start",
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_empty");
        loaded.Should().NotBeNull();
    }

    [Fact]
    public async Task testGetQuestState_givenLoadedState_shouldPreserveAllProperties()
    {
        var startTime = DateTime.UtcNow.AddHours(-1);
        var completionTime = DateTime.UtcNow;
        var questState = new QuestState
        {
            PlayerId = Guid.NewGuid(),
            QuestId = TEST_QUEST_ID + "_preserve",
            CurrentState = "completed",
            StartedAt = startTime,
            CompletedAt = completionTime,
            IsCompleted = true
        };
        await _questRepository.SaveQuestStateAsync(TEST_PLAYER_ID, questState);

        var loaded = await _questRepository.GetQuestStateAsync(TEST_PLAYER_ID, TEST_QUEST_ID + "_preserve");

        loaded!.IsCompleted.Should().BeTrue();
    }
}
