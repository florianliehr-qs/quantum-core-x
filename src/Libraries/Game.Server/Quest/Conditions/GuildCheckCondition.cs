using System.Text.Json.Serialization;

namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if player is in a guild.
/// </summary>
public class GuildCheckCondition : QuestConditionBase
{
    /// <summary>
    /// If true, checks that player IS in a guild.
    /// If false, checks that player IS NOT in a guild.
    /// Default is true.
    /// </summary>
    [JsonPropertyName("in_guild")]
    public bool InGuild { get; init; } = true;

    public override bool Evaluate(QuestConditionContext context)
    {
        var hasGuild = context.Player.Player.GuildId != null;
        return InGuild ? hasGuild : !hasGuild;
    }
}
