using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Provides context for evaluating quest conditions.
/// Contains references to the player, quest state, and services.
/// </summary>
public class QuestConditionContext
{
    /// <summary>
    /// The player entity for whom this condition is being evaluated.
    /// </summary>
    public required IPlayerEntity Player { get; init; }

    /// <summary>
    /// The current state of the quest being evaluated.
    /// </summary>
    public required QuestState State { get; init; }

    /// <summary>
    /// Service provider for accessing game services like IItemManager.
    /// </summary>
    public required IServiceProvider Services { get; init; }
}
