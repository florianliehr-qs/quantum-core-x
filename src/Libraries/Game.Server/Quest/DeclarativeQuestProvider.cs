using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Game.Quest.Models;

namespace QuantumCore.Game.Quest;

/// <summary>
/// Provides declarative quests loaded from JSON files.
/// Implements ILoadable to support startup loading of quest definitions.
/// </summary>
public class DeclarativeQuestProvider : ILoadable
{
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<DeclarativeQuestProvider> _logger;
    private ImmutableDictionary<string, QuestDefinition> _quests = ImmutableDictionary<string, QuestDefinition>.Empty;

    /// <summary>
    /// All loaded quest definitions, keyed by quest ID
    /// </summary>
    public IReadOnlyDictionary<string, QuestDefinition> Quests => _quests;

    public DeclarativeQuestProvider(IFileProvider fileProvider, ILogger<DeclarativeQuestProvider> logger)
    {
        _fileProvider = fileProvider;
        _logger = logger;
    }

    /// <summary>
    /// Loads all quest JSON files from the data/quests directory
    /// </summary>
    public async Task LoadAsync(CancellationToken token = default)
    {
        var questsDir = _fileProvider.GetDirectoryContents("quests");

        if (!questsDir.Exists)
        {
            _logger.LogWarning("Quests directory does not exist, no declarative quests loaded");
            return;
        }

        var loadedQuests = new Dictionary<string, QuestDefinition>();
        var questFiles = questsDir.Where(f => !f.IsDirectory && f.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in questFiles)
        {
            try
            {
                var quest = await LoadQuestFile(file, token);
                if (quest != null)
                {
                    if (loadedQuests.ContainsKey(quest.Id))
                    {
                        _logger.LogError("Duplicate quest ID {QuestId} found in {FileName}, skipping", quest.Id, file.Name);
                        continue;
                    }

                    if (ValidateQuest(quest, file.Name))
                    {
                        loadedQuests[quest.Id] = quest;
                        _logger.LogInformation("Loaded quest {QuestId} ({QuestName}) from {FileName}",
                            quest.Id, quest.Name, file.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load quest from {FileName}", file.Name);
            }
        }

        _quests = loadedQuests.ToImmutableDictionary();
        _logger.LogInformation("Loaded {Count} declarative quests", _quests.Count);
    }

    private async Task<QuestDefinition?> LoadQuestFile(IFileInfo file, CancellationToken token)
    {
        await using var stream = file.CreateReadStream();

        try
        {
            var quest = await JsonSerializer.DeserializeAsync<QuestDefinition>(stream, new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            }, cancellationToken: token);

            return quest;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error in {FileName}", file.Name);
            return null;
        }
    }

    private bool ValidateQuest(QuestDefinition quest, string fileName)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(quest.Id))
        {
            _logger.LogError("Quest in {FileName} is missing required 'id' field", fileName);
            return false;
        }

        if (string.IsNullOrWhiteSpace(quest.Name))
        {
            _logger.LogError("Quest {QuestId} in {FileName} is missing required 'name' field", quest.Id, fileName);
            return false;
        }

        if (quest.States == null || quest.States.Count == 0)
        {
            _logger.LogError("Quest {QuestId} has no states defined", quest.Id);
            return false;
        }

        // Validate that a "start" state exists
        if (!quest.States.ContainsKey("start"))
        {
            _logger.LogError("Quest {QuestId} is missing required 'start' state", quest.Id);
            return false;
        }

        // Validate state transitions
        foreach (var (stateName, state) in quest.States)
        {
            if (state.Triggers != null)
            {
                foreach (var trigger in state.Triggers)
                {
                    if (!string.IsNullOrEmpty(trigger.NextState) && !quest.States.ContainsKey(trigger.NextState))
                    {
                        _logger.LogWarning("Quest {QuestId} state '{StateName}' references non-existent state '{NextState}'",
                            quest.Id, stateName, trigger.NextState);
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Gets a quest definition by ID
    /// </summary>
    public QuestDefinition? GetQuest(string questId)
    {
        _quests.TryGetValue(questId, out var quest);
        return quest;
    }
}
