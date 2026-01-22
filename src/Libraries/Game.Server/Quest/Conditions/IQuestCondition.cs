namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Represents a condition that can be evaluated to determine if a quest action should execute.
/// Conditions check game state such as player level, inventory items, or quest flags.
/// </summary>
public interface IQuestCondition
{
    /// <summary>
    /// Evaluates the condition within the given quest context.
    /// </summary>
    /// <param name="context">The context containing player, quest state, and services</param>
    /// <returns>True if the condition is met, false otherwise</returns>
    bool Evaluate(QuestConditionContext context);
}
