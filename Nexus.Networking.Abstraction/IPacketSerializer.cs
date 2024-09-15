namespace Nexus.Networking.Abstraction;

public interface IPacketSerializer<TPacket>
    where TPacket : PacketBase
{
    void SerializePacket(TPacket packet, IMinecraftBinaryWriter writer);

    TPacket DeserializePacket(IMinecraftBinaryReader reader);
}
