namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Evaluates multiple conditions with AND logic (all must be true).
/// Short-circuits on the first false condition.
/// </summary>
public class AndCondition : QuestConditionBase
{
    /// <summary>
    /// The list of conditions that must all be true.
    /// </summary>
    public required List<IQuestCondition> Conditions { get; init; }

    /// <inheritdoc />
    public override bool Evaluate(QuestConditionContext context)
    {
        return Conditions.All(condition => condition.Evaluate(context));
    }
}
