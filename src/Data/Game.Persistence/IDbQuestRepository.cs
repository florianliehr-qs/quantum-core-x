using QuantumCore.API.Core.Models;

namespace QuantumCore.Game.Persistence;

/// <summary>
/// Repository for managing quest state persistence
/// </summary>
public interface IDbQuestRepository
{
    /// <summary>
    /// Gets the quest state for a specific player and quest
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="questId">The quest identifier</param>
    /// <returns>The quest state if found, null otherwise</returns>
    Task<QuestState?> GetQuestStateAsync(uint playerId, string questId);

    /// <summary>
    /// Saves or updates the quest state for a player
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="state">The quest state to save</param>
    Task SaveQuestStateAsync(uint playerId, QuestState state);

    /// <summary>
    /// Gets all quests for a player
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <returns>List of all quest states for the player</returns>
    Task<List<QuestState>> GetPlayerQuestsAsync(uint playerId);

    /// <summary>
    /// Gets all active (not completed) quests for a player
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <returns>List of active quest states</returns>
    Task<List<QuestState>> GetActiveQuestsAsync(uint playerId);

    /// <summary>
    /// Gets all completed quests for a player
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <returns>List of completed quest states</returns>
    Task<List<QuestState>> GetCompletedQuestsAsync(uint playerId);

    /// <summary>
    /// Deletes a quest state for a player (used for quest resets)
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="questId">The quest identifier</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteQuestStateAsync(uint playerId, string questId);
}
