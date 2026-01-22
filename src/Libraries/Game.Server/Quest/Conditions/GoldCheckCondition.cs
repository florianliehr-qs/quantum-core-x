using System.Text.Json.Serialization;
using QuantumCore.API.Game.Types.Entities;

namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if player has at least the specified amount of gold.
/// </summary>
public class GoldCheckCondition : QuestConditionBase
{
    /// <summary>
    /// Minimum amount of gold required.
    /// </summary>
    [JsonPropertyName("amount")]
    public required uint Amount { get; init; }

    public override bool Evaluate(QuestConditionContext context)
    {
        var playerGold = context.Player.GetPoint(EPoint.GOLD);
        return playerGold >= Amount;
    }
}
