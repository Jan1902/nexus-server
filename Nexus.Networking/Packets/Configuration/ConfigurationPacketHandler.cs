using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;
using Nexus.Networking.Connections;

namespace Nexus.Networking.Packets.Configuration;

internal class ConfigurationPacketHandler(ConnectionHandler connectionHandler)
    : IPacketHandler<ServerboundKnownPacks>,
    IPacketHandler<ClientInformation>,
    IPacketHandler<AcknowledgeFinishConfiguration>
{
    public Task HandlePacket(ServerboundKnownPacks knownPacks, Guid clientId, CancellationToken cancellationToken)
        //=> connectionHandler.SendPacketAsync(new RegistryData("test", []));
        => Task.CompletedTask;

    public Task HandlePacket(ClientInformation packet, Guid clientId, CancellationToken cancellationToken)
        => connectionHandler.SendPacketAsync(new FinishConfiguration(), clientId);

    public Task HandlePacket(AcknowledgeFinishConfiguration packet, Guid clientId, CancellationToken cancellationToken)
    {
        connectionHandler.SetProtocolStateForClient(clientId, ProtocolState.Play);
        return Task.CompletedTask;
    }
}
