using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types.Entities;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Awards experience points to the player.
/// </summary>
public class GiveExpAction : QuestActionBase
{
    /// <summary>
    /// The amount of experience to award.
    /// </summary>
    public required uint Amount { get; init; }

    /// <inheritdoc />
    public override Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        context.Player.AddPoint(EPoint.EXPERIENCE, (int)Amount);
        context.Player.SendPoints();
        context.Logger.LogDebug("Gave {Amount} experience to player {PlayerId}", Amount, context.Player.Player.Id);
        return Task.CompletedTask;
    }
}
