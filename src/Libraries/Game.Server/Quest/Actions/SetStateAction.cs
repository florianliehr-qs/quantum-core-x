using Microsoft.Extensions.Logging;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Changes the quest to a new state.
/// Used for state machine transitions in multi-stage quests.
/// </summary>
public class SetStateAction : QuestActionBase
{
    /// <summary>
    /// The new state to transition to.
    /// </summary>
    public required string NewState { get; init; }

    /// <inheritdoc />
    public override Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        var oldState = context.State.CurrentState;
        context.State.CurrentState = NewState;
        context.Logger.LogDebug("Changed quest {QuestId} state from {OldState} to {NewState} for player {PlayerId}",
            context.State.QuestId, oldState, NewState, context.Player.Player.Id);
        return Task.CompletedTask;
    }
}
