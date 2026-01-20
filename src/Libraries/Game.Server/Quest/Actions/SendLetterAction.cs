using Microsoft.Extensions.Logging;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Sends a quest notification letter to the player.
/// Used to update the player on quest progress.
/// </summary>
public class SendLetterAction : QuestActionBase
{
    /// <summary>
    /// The title of the quest letter.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The body text of the quest letter.
    /// </summary>
    public required string Text { get; init; }

    /// <inheritdoc />
    public override Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        // Store letter info in quest flags for client UI to display
        context.State.SetStringFlag("_letter_title", Title);
        context.State.SetStringFlag("_letter_text", Text);
        context.State.SetBoolFlag("_has_letter", true);

        context.Logger.LogDebug("Sent quest letter '{Title}' to player {PlayerId}", Title, context.Player.Player.Id);

        // TODO: Send packet to client when packet protocol is defined
        return Task.CompletedTask;
    }
}
