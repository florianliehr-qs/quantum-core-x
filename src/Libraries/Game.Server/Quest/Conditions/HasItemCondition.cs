namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if the player has a specific item in their inventory.
/// Used for item collection quests.
/// </summary>
public class HasItemCondition : QuestConditionBase
{
    /// <summary>
    /// The item ID to check for.
    /// </summary>
    public required uint ItemId { get; init; }

    /// <summary>
    /// The minimum count required (default: 1).
    /// </summary>
    public byte Count { get; init; } = 1;

    /// <inheritdoc />
    public override bool Evaluate(QuestConditionContext context)
    {
        var totalCount = context.Player.Inventory.Items
            .Where(item => item.ItemId == ItemId)
            .Sum(item => item.Count);

        return totalCount >= Count;
    }
}
