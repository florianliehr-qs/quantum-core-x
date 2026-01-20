using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types.Entities;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Awards gold (currency) to the player.
/// </summary>
public class GiveGoldAction : QuestActionBase
{
    /// <summary>
    /// The amount of gold to award.
    /// </summary>
    public required uint Amount { get; init; }

    /// <inheritdoc />
    public override Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        context.Player.AddPoint(EPoint.GOLD, (int)Amount);
        context.Player.SendPoints();
        context.Logger.LogDebug("Gave {Amount} gold to player {PlayerId}", Amount, context.Player.Player.Id);
        return Task.CompletedTask;
    }
}
