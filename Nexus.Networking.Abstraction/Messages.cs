using MediatR;
using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Abstraction;

public record SendPacketMessage<TPacket>(TPacket Packet) : INotification
    where TPacket : PacketBase;