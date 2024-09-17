using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Packets;

[AutoSerializedPacket(0x00, ProtocolState.Status, PacketDirection.ServerBound)]
public record StatusRequest : PacketBase;

[AutoSerializedPacket(0x01, ProtocolState.Status, PacketDirection.ServerBound)]
public record PingRequest(long Time) : PacketBase;