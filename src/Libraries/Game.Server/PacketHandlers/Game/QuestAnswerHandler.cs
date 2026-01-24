using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Quest;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuestAnswerHandler : IGamePacketHandler<QuestAnswer>
{
    private readonly ILogger<QuestAnswerHandler> _logger;

    public QuestAnswerHandler(ILogger<QuestAnswerHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<QuestAnswer> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            _logger.LogWarning("QuestAnswerHandler: player is null");
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        _logger.LogWarning("Quest answer: {Answer}, CurrentQuest: {HasQuest}", ctx.Packet.Answer, player.CurrentQuest != null);

        if (player.CurrentQuest is null)
        {
            _logger.LogWarning("QuestAnswerHandler: CurrentQuest is null for player {Player}", player.Name);
        }

        player.CurrentQuest?.Answer(ctx.Packet.Answer);

        return Task.CompletedTask;
    }
}
