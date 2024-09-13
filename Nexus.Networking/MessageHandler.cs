using MediatR;
using Nexus.Networking.Abstraction;

namespace Nexus.Networking;

internal class MessageHandler<TPacket> : INotificationHandler<SendPacketMessage<TPacket>> where TPacket : IPacket
{
    public Task Handle(SendPacketMessage<TPacket> message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
