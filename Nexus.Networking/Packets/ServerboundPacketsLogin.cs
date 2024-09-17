using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Packets;

[AutoSerializedPacket(0x00, ProtocolState.Handshake, PacketDirection.ServerBound)]
public record Handshake(int ProtocolVersion, string ServerAddress, short ServerPort, [Enum] ProtocolState NextState) : PacketBase;

[AutoSerializedPacket(0x00, ProtocolState.Login, PacketDirection.ServerBound)]
public record LoginStart(string Name, Guid PlayerUUID) : PacketBase;

[AutoSerializedPacket(0x03, ProtocolState.Login, PacketDirection.ServerBound)]
public record LoginAcknowledged : PacketBase;