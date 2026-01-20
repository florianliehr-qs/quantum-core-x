using Microsoft.Extensions.Logging;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Removes an item from the player's inventory.
/// </summary>
public class RemoveItemAction : QuestActionBase
{
    /// <summary>
    /// The item ID to remove from the player.
    /// </summary>
    public required uint ItemId { get; init; }

    /// <summary>
    /// The number of items to remove (default: 1).
    /// </summary>
    public byte Count { get; init; } = 1;

    /// <inheritdoc />
    public override Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        var inventory = context.Player.Inventory;
        byte remaining = Count;

        // Find and remove items from inventory
        foreach (var item in inventory.Items.Where(i => i.ItemId == ItemId).ToList())
        {
            if (remaining == 0) break;

            var countToRemove = Math.Min(remaining, item.Count);
            if (countToRemove >= item.Count)
            {
                // Remove entire stack
                inventory.RemoveItem(item);
            }
            else
            {
                // Decrement count
                item.Count -= countToRemove;
            }
            remaining -= countToRemove;
        }

        if (remaining > 0)
        {
            context.Logger.LogWarning("Could not remove all items - {RemainingCount} of {ItemId} not found in inventory",
                remaining, ItemId);
        }

        context.Player.SendInventory();
        context.Logger.LogDebug("Removed {Count}x item {ItemId} from player {PlayerId}", Count - remaining, ItemId, context.Player.Player.Id);
        return Task.CompletedTask;
    }
}
