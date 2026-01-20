namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Represents an action that can be executed as part of a quest flow.
/// Actions modify game state such as giving items, awarding experience, or changing quest state.
/// </summary>
public interface IQuestAction
{
    /// <summary>
    /// Executes the action within the given quest context.
    /// </summary>
    /// <param name="context">The context containing player, quest state, and services</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default);
}
