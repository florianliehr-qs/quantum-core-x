using Microsoft.Extensions.Logging;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Provides context for executing quest actions.
/// Contains references to the player, quest state, services, and logger.
/// </summary>
public class QuestActionContext
{
    /// <summary>
    /// The player entity executing this quest action.
    /// </summary>
    public required IPlayerEntity Player { get; init; }

    /// <summary>
    /// The current state of the quest being executed.
    /// </summary>
    public required QuestState State { get; init; }

    /// <summary>
    /// Service provider for accessing game services like IItemManager.
    /// </summary>
    public required IServiceProvider Services { get; init; }

    /// <summary>
    /// Logger for debugging and error reporting.
    /// </summary>
    public required ILogger Logger { get; init; }
}
