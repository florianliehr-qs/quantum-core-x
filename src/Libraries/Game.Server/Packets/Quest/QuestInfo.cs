using System.Text;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Quest;

/// <summary>
/// Sends quest status information to the client (0x51 / HEADER_GC_QUEST_INFO).
/// This packet has conditional fields based on the Flag byte, so it requires manual serialization.
/// </summary>
[Packet(0x51, EDirection.OUTGOING)]
public class QuestInfo : IPacketSerializable
{
    public ushort Index { get; set; }
    public byte Flag { get; set; }

    // Conditional fields based on Flag bits:
    public bool IsBegin { get; set; }              // Flag & 0x01
    public string Title { get; set; } = "";         // Flag & 0x02 (30 bytes)
    public string ClockName { get; set; } = "";     // Flag & 0x04 (16 bytes)
    public int ClockValue { get; set; }             // Flag & 0x08
    public string CounterName { get; set; } = "";   // Flag & 0x10 (16 bytes)
    public int CounterValue { get; set; }           // Flag & 0x20
    public string IconFile { get; set; } = "";      // Flag & 0x40 (24 bytes)

    // Flag constants
    private const byte QUEST_SEND_IS_BEGIN = 0x01;
    private const byte QUEST_SEND_TITLE = 0x02;
    private const byte QUEST_SEND_CLOCK_NAME = 0x04;
    private const byte QUEST_SEND_CLOCK_VALUE = 0x08;
    private const byte QUEST_SEND_COUNTER_NAME = 0x10;
    private const byte QUEST_SEND_COUNTER_VALUE = 0x20;
    private const byte QUEST_SEND_ICON_FILE = 0x40;

    // IPacketSerializable static members
    public static byte Header => 0x51;
    public static byte? SubHeader => null;
    public static bool HasStaticSize => false;
    public static bool HasSequence => false;

    public ushort GetSize()
    {
        // Base: Header (1) + Size (2) + Index (2) + Flag (1) = 6 bytes
        ushort size = 6;

        // Conditional fields (string lengths include null terminator: +1)
        if ((Flag & QUEST_SEND_IS_BEGIN) != 0) size += 1;
        if ((Flag & QUEST_SEND_TITLE) != 0) size += 31;        // 30 + 1
        if ((Flag & QUEST_SEND_CLOCK_NAME) != 0) size += 17;   // 16 + 1
        if ((Flag & QUEST_SEND_CLOCK_VALUE) != 0) size += 4;
        if ((Flag & QUEST_SEND_COUNTER_NAME) != 0) size += 17; // 16 + 1
        if ((Flag & QUEST_SEND_COUNTER_VALUE) != 0) size += 4;
        if ((Flag & QUEST_SEND_ICON_FILE) != 0) size += 25;    // 24 + 1

        return size;
    }

    public void Serialize(byte[] bytes, in int offset = 0)
    {
        var pos = offset;

        // Write Header
        bytes[pos++] = Header;

        // Write Size (ushort) - total packet size
        var totalSize = GetSize();
        BitConverter.TryWriteBytes(bytes.AsSpan(pos), totalSize);
        pos += 2;

        // Write Index (ushort)
        BitConverter.TryWriteBytes(bytes.AsSpan(pos), Index);
        pos += 2;

        // Write Flag
        bytes[pos++] = Flag;

        if ((Flag & QUEST_SEND_IS_BEGIN) != 0)
        {
            bytes[pos++] = (byte)(IsBegin ? 1 : 0);
        }

        if ((Flag & QUEST_SEND_TITLE) != 0)
        {
            WriteFixedString(bytes, ref pos, Title, 31);
        }

        if ((Flag & QUEST_SEND_CLOCK_NAME) != 0)
        {
            WriteFixedString(bytes, ref pos, ClockName, 17);
        }

        if ((Flag & QUEST_SEND_CLOCK_VALUE) != 0)
        {
            BitConverter.TryWriteBytes(bytes.AsSpan(pos), ClockValue);
            pos += 4;
        }

        if ((Flag & QUEST_SEND_COUNTER_NAME) != 0)
        {
            WriteFixedString(bytes, ref pos, CounterName, 17);
        }

        if ((Flag & QUEST_SEND_COUNTER_VALUE) != 0)
        {
            BitConverter.TryWriteBytes(bytes.AsSpan(pos), CounterValue);
            pos += 4;
        }

        if ((Flag & QUEST_SEND_ICON_FILE) != 0)
        {
            WriteFixedString(bytes, ref pos, IconFile, 25);
        }
    }

    private static void WriteFixedString(byte[] bytes, ref int pos, string value, int length)
    {
        Array.Clear(bytes, pos, length);
        var strBytes = Encoding.ASCII.GetBytes(value);
        var copyLength = Math.Min(strBytes.Length, length - 1); // Leave room for null terminator
        Array.Copy(strBytes, 0, bytes, pos, copyLength);
        pos += length;
    }

    // Not used for outgoing packets, but required by interface
    public static T Deserialize<T>(ReadOnlySpan<byte> bytes, in int offset = 0) where T : IPacketSerializable
    {
        throw new NotSupportedException("QuestInfo is an outgoing-only packet");
    }

    public static ValueTask<object> DeserializeFromStreamAsync(Stream stream)
    {
        throw new NotSupportedException("QuestInfo is an outgoing-only packet");
    }
}
