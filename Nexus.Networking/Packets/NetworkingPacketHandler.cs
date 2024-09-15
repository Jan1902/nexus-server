using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Packets;

internal class NetworkingPacketHandler : IPacketHandler<Handshake>
{
    public Task HandlePacket(Handshake packet, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
