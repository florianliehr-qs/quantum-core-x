using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Quest;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuestConfirmAnswerHandler : IGamePacketHandler<QuestConfirmAnswer>
{
    private readonly ILogger<QuestConfirmAnswerHandler> _logger;

    public QuestConfirmAnswerHandler(ILogger<QuestConfirmAnswerHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<QuestConfirmAnswer> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        _logger.LogDebug("Quest confirm answer: {Answer} for PID {PID}", ctx.Packet.Answer, ctx.Packet.RequestPID);

        // Answer 1 = Yes/Confirm, 0 = No/Cancel
        player.CurrentQuest?.AnswerConfirm(ctx.Packet.Answer == 1);

        return Task.CompletedTask;
    }
}
