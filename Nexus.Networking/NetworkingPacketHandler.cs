using Nexus.Networking.Abstraction;

namespace Nexus.Networking;

internal class NetworkingPacketHandler : IPacketHandler<HandshakeCBPacket>
{
    public Task HandlePacket(HandshakeCBPacket packet, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
