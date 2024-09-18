using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Packets.Play;

[AutoSerializedPacket(0x18, packetDirection: PacketDirection.ServerBound)]
public record ServerboundKeepAlive(long KeepAliveId) : PacketBase;