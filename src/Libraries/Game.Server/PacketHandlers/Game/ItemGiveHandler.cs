using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemGiveHandler : IGamePacketHandler<ItemGive>
{
    private readonly ILogger<ItemGiveHandler> _logger;
    private readonly IQuestEventManager _questEventManager;

    public ItemGiveHandler(ILogger<ItemGiveHandler> logger, IQuestEventManager questEventManager)
    {
        _logger = logger;
        _questEventManager = questEventManager;
    }

    public Task ExecuteAsync(GamePacketContext<ItemGive> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        var entity = player.Map?.GetEntity(ctx.Packet.TargetVid);
        if (entity is null)
        {
            _logger.LogDebug("Ignore item give to non existing entity");
            return Task.CompletedTask;
        }

        var item = player.GetItem(ctx.Packet.Window, ctx.Packet.Position);
        if (item is null)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation("Item give to {Entity}", entity);

        // Fire-and-forget the quest event to avoid blocking packet processing.
        // Quest dialogs use async/await and wait for user input - if we awaited here,
        // we'd block the packet processing loop and create a deadlock.
        _ = Task.Run(async () =>
        {
            try
            {
                await _questEventManager.OnNpcGive(entity.EntityClass, player, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NPC give event handler");
            }
        });

        return Task.CompletedTask;
    }
}
