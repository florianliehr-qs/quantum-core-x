using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Quest;

public class QuestManager : IQuestManager, ILoadable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QuestManager> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Dictionary<string, Type> _quests = new();

    public QuestManager(IServiceProvider serviceProvider, ILogger<QuestManager> logger, IServiceScopeFactory scopeFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public Task LoadAsync(CancellationToken token = default)
    {
        // Scan for all available quests
        var assembly = Assembly.GetAssembly(typeof(QuestManager));
        if (assembly is null)
        {
            return Task.CompletedTask;
        }

        foreach (var questType in assembly.GetTypes().Where(type => type.GetCustomAttribute<QuestAttribute>() is not null))
        {
            RegisterQuest(questType);
        }

        return Task.CompletedTask;
    }

    public void InitializePlayer(IPlayerEntity player)
    {
        // Synchronous wrapper for backwards compatibility
        InitializePlayerAsync(player).GetAwaiter().GetResult();
    }

    public async Task InitializePlayerAsync(IPlayerEntity player)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }

        // Load saved quest states from database using a scoped repository
        Dictionary<string, QuestState> savedStatesDict;
        try
        {
            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var questRepository = scope.ServiceProvider.GetRequiredService<IDbQuestRepository>();
                var savedStates = await questRepository.GetPlayerQuestsAsync(player.Player.Id);
                savedStatesDict = savedStates.ToDictionary(s => s.QuestName, s => s);
                _logger.LogDebug("Loaded {Count} quest states from database for player {PlayerId}", savedStates.Count, player.Player.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load quest states from database for player {PlayerId}", player.Player.Id);
            savedStatesDict = new Dictionary<string, QuestState>();
        }

        ushort questIndex = 1; // Start at index 1 (0 might be reserved)
        foreach (var (id, questType) in _quests)
        {
            // Try to load existing state from database, or create new
            QuestState state;
            if (savedStatesDict.TryGetValue(id, out var savedState))
            {
                state = savedState;
            }
            else
            {
                state = new QuestState { QuestName = id };
            }

            Quest quest;
            try
            {
                quest = (Quest) ActivatorUtilities.CreateInstance(_serviceProvider, questType, state, player);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to initialize quest {Id} for {Player}", id, player);
                continue;
            }

            // Assign unique quest index for client tracking
            quest.SetQuestIndex(questIndex++);
            quest.Init();
            p.Quests[id] = quest;
            // Note: Quest info is sent when player enters game, not during loading
        }
    }

    public async Task SavePlayerQuestsAsync(IPlayerEntity player)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }

        var playerId = player.Player.Id;
        var statesToSave = p.Quests.Values
            .Select(q => q.State)
            .Where(s => s.IsDirty || s.IsStarted) // Save dirty or started quests
            .ToList();

        if (statesToSave.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Saving {Count} quest states for player {PlayerId}", statesToSave.Count, playerId);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var questRepository = scope.ServiceProvider.GetRequiredService<IDbQuestRepository>();

        foreach (var state in statesToSave)
        {
            try
            {
                await questRepository.SaveQuestStateAsync(playerId, state);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to save quest {QuestName} for player {PlayerId}", state.QuestName, playerId);
            }
        }
    }

    public void RegisterQuest(Type questType)
    {
        var id = questType.FullName ?? Guid.NewGuid().ToString();
        if (_quests.ContainsKey(id))
        {
            _logger.LogError("Can't register quest {Type} because it's already registered or a duplicate",
                questType.FullName);
            return;
        }

        _logger.LogInformation("Registered quest {Id}", id);
        _quests[id] = questType;
    }
}
