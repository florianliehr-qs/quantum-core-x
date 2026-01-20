using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Quest.Factories;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Quest;

public class QuestManager : IQuestManager, ILoadable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QuestManager> _logger;
    private readonly DeclarativeQuestProvider _declarativeQuestProvider;
    private readonly QuestActionFactory _actionFactory;
    private readonly QuestConditionFactory _conditionFactory;
    private readonly Dictionary<string, Type> _quests = new();

    public QuestManager(
        IServiceProvider serviceProvider,
        ILogger<QuestManager> logger,
        DeclarativeQuestProvider declarativeQuestProvider,
        QuestActionFactory actionFactory,
        QuestConditionFactory conditionFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _declarativeQuestProvider = declarativeQuestProvider;
        _actionFactory = actionFactory;
        _conditionFactory = conditionFactory;
    }

    public async Task LoadAsync(CancellationToken token = default)
    {
        // Scan for all available C# quests
        var assembly = Assembly.GetAssembly(typeof(QuestManager));
        if (assembly is not null)
        {
            foreach (var questType in assembly.GetTypes().Where(type => type.GetCustomAttribute<QuestAttribute>() is not null))
            {
                RegisterQuest(questType);
            }
        }

        // Load declarative quests from JSON files
        await _declarativeQuestProvider.LoadAsync(token);
        _logger.LogInformation("Loaded {Count} declarative quests", _declarativeQuestProvider.Quests.Count);
    }

    public void InitializePlayer(IPlayerEntity player)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }

        // Create a scope to resolve scoped services like IDbQuestRepository
        using var scope = _serviceProvider.CreateScope();
        var questRepository = scope.ServiceProvider.GetRequiredService<IDbQuestRepository>();

        // Initialize C# quests
        foreach (var (id, questType) in _quests)
        {
            var state = questRepository.GetQuestStateAsync(player.Player.Id, id).Result
                ?? new QuestState { QuestId = id };

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

            quest.Init();
            p.Quests[id] = quest;
        }

        // Initialize declarative quests
        foreach (var (id, definition) in _declarativeQuestProvider.Quests)
        {
            var state = questRepository.GetQuestStateAsync(player.Player.Id, id).Result
                ?? new QuestState { QuestId = id };

            try
            {
                var quest = new DeclarativeQuest(
                    state,
                    player,
                    definition,
                    _actionFactory,
                    _conditionFactory,
                    _serviceProvider,
                    _serviceProvider.GetRequiredService<ILogger<DeclarativeQuest>>());

                quest.Init();
                p.Quests[id] = quest;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to initialize declarative quest {Id} for {Player}", id, player);
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
