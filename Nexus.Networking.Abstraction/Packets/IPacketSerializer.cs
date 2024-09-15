using Nexus.Networking.Abstraction.Data;

namespace Nexus.Networking.Abstraction.Packets;

public interface IPacketSerializer<TPacket>
    where TPacket : PacketBase
{
    void SerializePacket(TPacket packet, IMinecraftBinaryWriter writer);

    TPacket DeserializePacket(IMinecraftBinaryReader reader);
}
