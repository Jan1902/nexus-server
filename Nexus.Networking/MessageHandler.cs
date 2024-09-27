using MediatR;
using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;
using Nexus.Networking.Connections;

namespace Nexus.Networking;

internal class MessageHandler<TPacket>(ConnectionHandler connectionHandler)
    : INotificationHandler<SendPacketToClientMessage<TPacket>>,
    INotificationHandler<SendPacketToAllClientsMessage<TPacket>>
    where TPacket : PacketBase
{
    public Task Handle(SendPacketToClientMessage<TPacket> message, CancellationToken cancellationToken)
        => connectionHandler.SendPacketAsync(message.Packet, message.ClientId);

    public Task Handle(SendPacketToAllClientsMessage<TPacket> notification, CancellationToken cancellationToken)
        => connectionHandler.SendPacketAsync(notification.Packet, protocolState: notification.ProtocolState);
}
