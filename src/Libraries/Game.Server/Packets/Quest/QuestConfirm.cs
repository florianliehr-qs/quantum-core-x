using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Quest;

/// <summary>
/// Sends a confirmation dialog to the client (0x2E / HEADER_GC_QUEST_CONFIRM).
/// </summary>
[Packet(0x2E, EDirection.OUTGOING)]
[PacketGenerator]
public partial class QuestConfirm
{
    [Field(0, Length = 65)] public string Message { get; set; } = "";
    [Field(1)] public int Timeout { get; set; }
    [Field(2)] public uint RequestPID { get; set; }
}
