using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Quest;

/// <summary>
/// Receives quest button click from the client (0x42 / HEADER_CG_SCRIPT_BUTTON).
/// </summary>
[Packet(0x42, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class ScriptButton
{
    [Field(0)] public uint Index { get; set; }
}
