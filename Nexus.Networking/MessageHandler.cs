using MediatR;
using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking;

internal class MessageHandler<TPacket> : INotificationHandler<SendPacketMessage<TPacket>> where TPacket : PacketBase
{
    public Task Handle(SendPacketMessage<TPacket> message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
