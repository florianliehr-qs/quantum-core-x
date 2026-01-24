using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Quest;

/// <summary>
/// Receives item selection from the client (0x72 / HEADER_CG_SCRIPT_SELECT_ITEM).
/// </summary>
[Packet(0x72, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class ScriptSelectItem
{
    [Field(0)] public uint Selection { get; set; }
}
