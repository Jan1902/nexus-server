using Nexus.Networking.Abstraction;

namespace Nexus.Networking;

[AutoSerializedPacket(0x00)]
public record HandshakeCBPacket(string Username, string Address, int Port, byte[] Data, [Conditional] string Message, TestObject Test) : PacketBase;

public record TestObject(string Connection, int Length);