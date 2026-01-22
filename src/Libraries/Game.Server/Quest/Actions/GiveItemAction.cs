using Microsoft.Extensions.Logging;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Awards an item to the player's inventory.
/// </summary>
public class GiveItemAction : QuestActionBase
{
    /// <summary>
    /// The item ID to give to the player.
    /// </summary>
    public required uint ItemId { get; init; }

    /// <summary>
    /// The number of items to give (default: 1).
    /// </summary>
    public byte Count { get; init; } = 1;

    /// <inheritdoc />
    public override async Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        var itemManager = GetItemManager(context);
        var itemProto = itemManager.GetItem(ItemId);

        if (itemProto == null)
        {
            context.Logger.LogWarning("Cannot give item {ItemId} - item not found in item manager", ItemId);
            return;
        }

        var item = itemManager.CreateItem(itemProto, Count);
        if (item == null)
        {
            context.Logger.LogWarning("Failed to create item instance for item {ItemId}", ItemId);
            return;
        }

        await context.Player.Inventory.PlaceItem(item);
        context.Player.SendInventory();
        context.Logger.LogDebug("Gave {Count}x item {ItemId} to player {PlayerId}", Count, ItemId, context.Player.Player.Id);
    }
}
