using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Quest;

/// <summary>
/// Receives text input from the client (0x1E / HEADER_CG_QUEST_INPUT_STRING).
/// </summary>
[Packet(0x1E, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class QuestInputString
{
    [Field(0, Length = 65)] public string Input { get; set; } = "";
}
