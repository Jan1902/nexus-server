using MediatR;
using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Abstraction;

public record PacketReceivedMessage<TPacket>(TPacket Packet, Guid ClientId) : INotification
    where TPacket : PacketBase;