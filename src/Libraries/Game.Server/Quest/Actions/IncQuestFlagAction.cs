using Microsoft.Extensions.Logging;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Increments an integer quest flag by a specified amount.
/// Commonly used for tracking kill counts, item collection, etc.
/// </summary>
public class IncQuestFlagAction : QuestActionBase
{
    /// <summary>
    /// The name of the flag to increment.
    /// </summary>
    public required string Flag { get; init; }

    /// <summary>
    /// The amount to increment by (default: 1).
    /// </summary>
    public int Amount { get; init; } = 1;

    /// <inheritdoc />
    public override Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        context.State.IncIntFlag(Flag, Amount);
        var newValue = context.State.GetIntFlag(Flag);
        context.Logger.LogDebug("Incremented quest flag {Flag} by {Amount} to {NewValue} for player {PlayerId}",
            Flag, Amount, newValue, context.Player.Player.Id);
        return Task.CompletedTask;
    }
}
