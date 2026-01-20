namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if the player's level is greater than or equal to a minimum value.
/// Used for level gating quests.
/// </summary>
public class LevelMinCondition : QuestConditionBase
{
    /// <summary>
    /// The minimum level required.
    /// </summary>
    public required byte MinLevel { get; init; }

    /// <inheritdoc />
    public override bool Evaluate(QuestConditionContext context)
    {
        return context.Player.Player.Level >= MinLevel;
    }
}
