using Microsoft.Extensions.Logging;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Sets an integer quest flag to a specific value.
/// This is the most fundamental action used for tracking quest progress.
/// </summary>
public class SetQuestFlagAction : QuestActionBase
{
    /// <summary>
    /// The name of the flag to set.
    /// </summary>
    public required string Flag { get; init; }

    /// <summary>
    /// The value to set the flag to.
    /// </summary>
    public required int Value { get; init; }

    /// <inheritdoc />
    public override Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        context.State.SetIntFlag(Flag, Value);
        context.Logger.LogDebug("Set quest flag {Flag} to {Value} for player {PlayerId}", Flag, Value, context.Player.Player.Id);
        return Task.CompletedTask;
    }
}
