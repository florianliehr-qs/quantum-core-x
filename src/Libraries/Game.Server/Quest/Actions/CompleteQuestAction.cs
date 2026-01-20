using Microsoft.Extensions.Logging;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Marks the quest as completed.
/// Sets IsCompleted to true and records the completion timestamp.
/// </summary>
public class CompleteQuestAction : QuestActionBase
{
    /// <inheritdoc />
    public override Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        context.State.IsCompleted = true;
        context.State.CompletedAt = DateTime.UtcNow;
        context.Logger.LogInformation("Completed quest {QuestId} for player {PlayerId}",
            context.State.QuestId, context.Player.Player.Id);
        return Task.CompletedTask;
    }
}
