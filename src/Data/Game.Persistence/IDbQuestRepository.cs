using QuantumCore.API;
using QuantumCore.API.Core.Models;

namespace QuantumCore.Game.Persistence;

/// <summary>
/// Extended quest repository interface with write operations.
/// </summary>
public interface IDbQuestRepository : IQuestRepository
{
    /// <summary>
    /// Saves or updates a quest state for a player.
    /// </summary>
    Task SaveQuestStateAsync(uint playerId, QuestState state);

    /// <summary>
    /// Deletes a quest state for a player.
    /// </summary>
    Task DeleteQuestStateAsync(uint playerId, string questName);

    /// <summary>
    /// Marks a timer as processed.
    /// </summary>
    Task MarkTimerProcessedAsync(uint playerId, string questName, string timerName);

    /// <summary>
    /// Saves all dirty quest states for a player.
    /// </summary>
    Task SaveAllDirtyQuestsAsync(uint playerId, IEnumerable<QuestState> states);
}
