using MediatR;

namespace Nexus.Networking.Abstraction.Packets;

public interface IPacketHandler<TPacket> : INotificationHandler<PacketReceivedMessage<TPacket>> where TPacket : PacketBase
{
    Task HandlePacket(TPacket packet, Guid clientId, CancellationToken cancellationToken);

    Task INotificationHandler<PacketReceivedMessage<TPacket>>.Handle(PacketReceivedMessage<TPacket> message, CancellationToken cancellationToken)
        => HandlePacket(message.Packet, message.ClientId, cancellationToken);
}
