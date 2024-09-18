using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Packets.Play;

[AutoSerializedPacket(0x26)]
public record ClientboundKeepAlive(long KeepAliveId) : PacketBase;