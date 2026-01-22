namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if a quest flag equals a specific value.
/// Used for exact value matching (e.g., "stage == 2").
/// </summary>
public class QuestFlagEqCondition : QuestConditionBase
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
        return context.State.GetIntFlag(Flag) == Value;
    }
}
