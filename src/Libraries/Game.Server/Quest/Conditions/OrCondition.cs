namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Evaluates multiple conditions with OR logic (at least one must be true).
/// Short-circuits on the first true condition.
/// </summary>
public class OrCondition : QuestConditionBase
{
    /// <summary>
    /// The list of conditions where at least one must be true.
    /// </summary>
    public required List<IQuestCondition> Conditions { get; init; }

    /// <inheritdoc />
    public override bool Evaluate(QuestConditionContext context)
    {
        return Conditions.Any(condition => condition.Evaluate(context));
    }
}
