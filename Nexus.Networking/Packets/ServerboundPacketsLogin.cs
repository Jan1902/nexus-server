using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Packets;

[AutoSerializedPacket(0x00, ProtocolState.Handshake, PacketDirection.ServerBound)]
public record Handshake(int ProtocolVersion, string ServerAddress, short ServerPort, [Enum] ProtocolState NextState) : PacketBase;