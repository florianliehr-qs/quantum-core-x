using System.Text.Json.Serialization;

namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if player level is within a specified range (inclusive).
/// </summary>
public class LevelRangeCondition : QuestConditionBase
{
    /// <summary>
    /// Minimum level (inclusive).
    /// </summary>
    [JsonPropertyName("min")]
    public required byte Min { get; init; }

    /// <summary>
    /// Maximum level (inclusive).
    /// </summary>
    [JsonPropertyName("max")]
    public required byte Max { get; init; }

    public override bool Evaluate(QuestConditionContext context)
    {
        var playerLevel = context.Player.Player.Level;
        return playerLevel >= Min && playerLevel <= Max;
    }
}
