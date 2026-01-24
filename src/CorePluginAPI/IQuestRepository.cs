using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

/// <summary>
/// Repository interface for quest state persistence (read operations).
/// </summary>
public interface IQuestRepository
{
    /// <summary>
    /// Gets all quest states for a player.
    /// </summary>
    Task<ICollection<QuestState>> GetPlayerQuestsAsync(uint playerId);

    /// <summary>
    /// Gets a specific quest state for a player.
    /// </summary>
    Task<QuestState?> GetPlayerQuestAsync(uint playerId, string questName);

    /// <summary>
    /// Gets all pending timers that need to be processed.
    /// </summary>
    Task<ICollection<(uint PlayerId, string QuestName, string TimerName, DateTime TriggerAt)>> GetPendingTimersAsync();
}
