using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Waits for a specified duration before completing.
/// Useful for timed quest events or delays between actions.
/// </summary>
public class DelayAction : QuestActionBase
{
    /// <summary>
    /// Duration to wait in seconds.
    /// </summary>
    [JsonPropertyName("seconds")]
    public required int Seconds { get; init; }

    /// <summary>
    /// Optional message to log when delay starts.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    public override async Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        if (Seconds <= 0)
        {
            context.Logger.LogWarning("DelayAction: Invalid seconds value {Seconds} for quest {QuestId}",
                Seconds, context.State.QuestId);
            return;
        }

        if (!string.IsNullOrWhiteSpace(Message))
        {
            context.Logger.LogDebug("DelayAction: {Message} (waiting {Seconds} seconds for quest {QuestId})",
                Message, Seconds, context.State.QuestId);
        }
        else
        {
            context.Logger.LogDebug("DelayAction: Waiting {Seconds} seconds for quest {QuestId}",
                Seconds, context.State.QuestId);
        }

        await Task.Delay(TimeSpan.FromSeconds(Seconds), cancellationToken);

        context.Logger.LogDebug("DelayAction: Delay complete for quest {QuestId}", context.State.QuestId);
    }
}
