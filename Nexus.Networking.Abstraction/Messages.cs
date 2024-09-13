using MediatR;

namespace Nexus.Networking.Abstraction;

public record SendPacketMessage<TPacket>(TPacket Packet) : INotification
    where TPacket : IPacket;