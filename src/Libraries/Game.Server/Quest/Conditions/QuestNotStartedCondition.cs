namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if a quest has not been started yet or is still in the "start" state.
/// Used to ensure a quest is only offered once.
/// </summary>
public class QuestNotStartedCondition : QuestConditionBase
{
    /// <inheritdoc />
    public override bool Evaluate(QuestConditionContext context)
    {
        // Quest is not started if QuestId is empty or CurrentState is "start"
        return string.IsNullOrEmpty(context.State.QuestId) || context.State.CurrentState == "start";
    }
}
