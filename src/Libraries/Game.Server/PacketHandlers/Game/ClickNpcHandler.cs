using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ClickNpcHandler : IGamePacketHandler<ClickNpc>
{
    private readonly IQuestEventManager _questEventManager;
    private readonly ILogger<ClickNpcHandler> _logger;

    public ClickNpcHandler(IQuestEventManager questEventManager, ILogger<ClickNpcHandler> logger)
    {
        _questEventManager = questEventManager;
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<ClickNpc> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        var entity = player.Map?.GetEntity(ctx.Packet.Vid);
        if (entity is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        // Fire-and-forget the quest event to avoid blocking packet processing.
        // Quest dialogs use async/await and wait for user input - if we awaited here,
        // we'd block the packet processing loop and create a deadlock.
        _ = Task.Run(async () =>
        {
            try
            {
                await _questEventManager.OnNpcClick(entity.EntityClass, player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NPC click event handler");
            }
        });

        return Task.CompletedTask;
    }
}
