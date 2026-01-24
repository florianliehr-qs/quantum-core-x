using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Quest;

/// <summary>
/// Receives confirmation dialog answer from the client (0x1F / HEADER_CG_QUEST_CONFIRM).
/// </summary>
[Packet(0x1F, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class QuestConfirmAnswer
{
    [Field(0)] public byte Answer { get; set; }
    [Field(1)] public uint RequestPID { get; set; }
}
