using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Quest;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ScriptButtonHandler : IGamePacketHandler<ScriptButton>
{
    private readonly ILogger<ScriptButtonHandler> _logger;

    public ScriptButtonHandler(ILogger<ScriptButtonHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<ScriptButton> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        _logger.LogDebug("Script button clicked: index {Index}", ctx.Packet.Index);

        // Find the quest that owns this button index and trigger it
        foreach (var quest in player.Quests.Values)
        {
            quest.OnButton(ctx.Packet.Index);
        }

        return Task.CompletedTask;
    }
}
