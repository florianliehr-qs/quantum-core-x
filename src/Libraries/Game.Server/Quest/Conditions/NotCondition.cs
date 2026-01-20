namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Inverts the result of another condition.
/// Used for negation logic (e.g., "player does NOT have item").
/// </summary>
public class NotCondition : QuestConditionBase
{
    /// <summary>
    /// The condition to invert.
    /// </summary>
    public required IQuestCondition Condition { get; init; }

    /// <inheritdoc />
    public override bool Evaluate(QuestConditionContext context)
    {
        return !Condition.Evaluate(context);
    }
}
