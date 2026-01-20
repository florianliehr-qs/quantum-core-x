namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if a quest flag is greater than or equal to a value.
/// Commonly used for checking quest progress (e.g., "collected >= 10 items").
/// </summary>
public class QuestFlagGteCondition : QuestConditionBase
{
    /// <summary>
    /// The name of the flag to check.
    /// </summary>
    public required string Flag { get; init; }

    /// <summary>
    /// The value to compare against.
    /// </summary>
    public required int Value { get; init; }

    /// <inheritdoc />
    public override bool Evaluate(QuestConditionContext context)
    {
        return context.State.GetIntFlag(Flag) >= Value;
    }
}
