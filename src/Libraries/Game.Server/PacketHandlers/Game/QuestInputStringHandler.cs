using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Quest;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuestInputStringHandler : IGamePacketHandler<QuestInputString>
{
    private readonly ILogger<QuestInputStringHandler> _logger;

    public QuestInputStringHandler(ILogger<QuestInputStringHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<QuestInputString> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        _logger.LogDebug("Quest input string received: {Input}", ctx.Packet.Input);
        player.CurrentQuest?.AnswerInput(ctx.Packet.Input);

        return Task.CompletedTask;
    }
}
