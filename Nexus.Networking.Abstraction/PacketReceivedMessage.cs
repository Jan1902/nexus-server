using MediatR;

namespace Nexus.Networking.Abstraction;

public record PacketReceivedMessage<TPacket>(TPacket Packet) : INotification
    where TPacket : PacketBase;