namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if the player's level is less than or equal to a maximum value.
/// Used for capping quest availability (e.g., beginner quests).
/// </summary>
public class LevelMaxCondition : QuestConditionBase
{
    /// <summary>
    /// The maximum level allowed.
    /// </summary>
    public required byte MaxLevel { get; init; }

    /// <inheritdoc />
    public override bool Evaluate(QuestConditionContext context)
    {
        return context.Player.Player.Level <= MaxLevel;
    }
}
