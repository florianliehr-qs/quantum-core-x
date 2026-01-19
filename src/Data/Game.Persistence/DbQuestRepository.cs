using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Persistence;

/// <summary>
/// Repository implementation for quest state persistence
/// </summary>
public class DbQuestRepository : IDbQuestRepository
{
    private readonly GameDbContext _db;
    private readonly ILogger<DbQuestRepository> _logger;

    public DbQuestRepository(GameDbContext db, ILogger<DbQuestRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<QuestState?> GetQuestStateAsync(uint playerId, string questId)
    {
        var playerQuest = await _db.PlayerQuests
            .AsNoTracking()
            .FirstOrDefaultAsync(pq => pq.PlayerId == playerId && pq.QuestId == questId);

        if (playerQuest == null)
        {
            return null;
        }

        return MapToQuestState(playerQuest);
    }

    public async Task SaveQuestStateAsync(uint playerId, QuestState state)
    {
        var playerQuest = await _db.PlayerQuests
            .FirstOrDefaultAsync(pq => pq.PlayerId == playerId && pq.QuestId == state.QuestId);

        var questDataJson = SerializeQuestData(state);

        if (playerQuest == null)
        {
            // Insert new quest
            playerQuest = new PlayerQuest
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                QuestId = state.QuestId,
                CurrentState = state.CurrentState,
                StartedAt = state.StartedAt == default ? DateTime.UtcNow : state.StartedAt,
                CompletedAt = state.CompletedAt,
                IsCompleted = state.IsCompleted,
                QuestDataJson = questDataJson
            };

            _db.PlayerQuests.Add(playerQuest);
            _logger.LogDebug("Creating new quest state for player {PlayerId}, quest {QuestId}",
                playerId, state.QuestId);
        }
        else
        {
            // Update existing quest
            playerQuest.CurrentState = state.CurrentState;
            playerQuest.CompletedAt = state.CompletedAt;
            playerQuest.IsCompleted = state.IsCompleted;
            playerQuest.QuestDataJson = questDataJson;

            _db.PlayerQuests.Update(playerQuest);
            _logger.LogDebug("Updating quest state for player {PlayerId}, quest {QuestId}",
                playerId, state.QuestId);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<QuestState>> GetPlayerQuestsAsync(uint playerId)
    {
        var playerQuests = await _db.PlayerQuests
            .AsNoTracking()
            .Where(pq => pq.PlayerId == playerId)
            .ToListAsync();

        return playerQuests.Select(MapToQuestState).ToList();
    }

    public async Task<List<QuestState>> GetActiveQuestsAsync(uint playerId)
    {
        var playerQuests = await _db.PlayerQuests
            .AsNoTracking()
            .Where(pq => pq.PlayerId == playerId && !pq.IsCompleted)
            .ToListAsync();

        return playerQuests.Select(MapToQuestState).ToList();
    }

    public async Task<List<QuestState>> GetCompletedQuestsAsync(uint playerId)
    {
        var playerQuests = await _db.PlayerQuests
            .AsNoTracking()
            .Where(pq => pq.PlayerId == playerId && pq.IsCompleted)
            .ToListAsync();

        return playerQuests.Select(MapToQuestState).ToList();
    }

    public async Task<bool> DeleteQuestStateAsync(uint playerId, string questId)
    {
        var playerQuest = await _db.PlayerQuests
            .FirstOrDefaultAsync(pq => pq.PlayerId == playerId && pq.QuestId == questId);

        if (playerQuest == null)
        {
            return false;
        }

        _db.PlayerQuests.Remove(playerQuest);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted quest state for player {PlayerId}, quest {QuestId}",
            playerId, questId);

        return true;
    }

    /// <summary>
    /// Maps a PlayerQuest entity to a QuestState model
    /// </summary>
    private QuestState MapToQuestState(PlayerQuest playerQuest)
    {
        var questData = DeserializeQuestData(playerQuest.QuestDataJson);

        return new QuestState
        {
            PlayerId = Guid.Empty, // Will be set by caller if needed
            QuestId = playerQuest.QuestId,
            CurrentState = playerQuest.CurrentState,
            StartedAt = playerQuest.StartedAt,
            CompletedAt = playerQuest.CompletedAt,
            IsCompleted = playerQuest.IsCompleted,
            IntFlags = questData.IntFlags,
            StringFlags = questData.StringFlags,
            BoolFlags = questData.BoolFlags
        };
    }

    /// <summary>
    /// Serializes quest state data to JSON
    /// </summary>
    private string SerializeQuestData(QuestState state)
    {
        var questData = new QuestStateData
        {
            IntFlags = state.IntFlags,
            StringFlags = state.StringFlags,
            BoolFlags = state.BoolFlags
        };

        return JsonSerializer.Serialize(questData, new JsonSerializerOptions
        {
            WriteIndented = false // Compact JSON for database storage
        });
    }

    /// <summary>
    /// Deserializes quest state data from JSON
    /// </summary>
    private QuestStateData DeserializeQuestData(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<QuestStateData>(json) ?? new QuestStateData();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize quest data JSON: {Json}", json);
            return new QuestStateData();
        }
    }

    /// <summary>
    /// Internal DTO for JSON serialization of quest flags
    /// </summary>
    private class QuestStateData
    {
        public Dictionary<string, int> IntFlags { get; set; } = new();
        public Dictionary<string, string> StringFlags { get; set; } = new();
        public Dictionary<string, bool> BoolFlags { get; set; } = new();
    }
}
