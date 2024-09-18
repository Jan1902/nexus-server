using Nexus.Networking.Abstraction.Packets;

namespace Nexus.Networking.Packets.Play;

internal class PlayPacketHandler(KeepAliveHandler keepAliveHandler) : IPacketHandler<ServerboundKeepAlive>
{
    public Task HandlePacket(ServerboundKeepAlive packet, Guid clientId, CancellationToken cancellationToken)
    {
        keepAliveHandler.HandleKeepAlive(clientId, packet.KeepAliveId);

        return Task.CompletedTask;
    }
}
