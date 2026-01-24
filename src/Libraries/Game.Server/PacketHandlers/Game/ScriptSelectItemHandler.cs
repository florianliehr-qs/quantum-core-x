using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Quest;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ScriptSelectItemHandler : IGamePacketHandler<ScriptSelectItem>
{
    private readonly ILogger<ScriptSelectItemHandler> _logger;

    public ScriptSelectItemHandler(ILogger<ScriptSelectItemHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<ScriptSelectItem> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        _logger.LogDebug("Script item selected: {Selection}", ctx.Packet.Selection);
        player.CurrentQuest?.OnSelectItem(ctx.Packet.Selection);

        return Task.CompletedTask;
    }
}
