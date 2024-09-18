using Microsoft.Extensions.Logging;
using Nexus.Networking.Abstraction.Packets;
using Nexus.Networking.Connections;
using System.Text.Json;

namespace Nexus.Networking.Packets.Status;

internal class StatusPacketHandler(
    ConnectionHandler connectionHandler,
    NetworkingConfiguration configuration)
    : IPacketHandler<PingRequest>,
    IPacketHandler<StatusRequest>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public Task HandlePacket(StatusRequest statusRequest, Guid clientId, CancellationToken cancellationToken)
    {
        var content = new StatusResponseContent(
            new StatusVersion(configuration.ServerVersion, configuration.ProtocolVersion),
            new StatusPlayers(configuration.MaxConnections, connectionHandler.ConnectionCount, connectionHandler.GetClients(Abstraction.ProtocolState.Play).Select(c => new StatusPlayersSample(c.Username, c.ClientId)).ToArray()),
            new StatusDescription(configuration.Motd),
            "",
            false);

        var json = JsonSerializer.Serialize(content, _jsonSerializerOptions);

        return connectionHandler.SendPacketAsync(new StatusResponse(json), clientId);
    }

    public Task HandlePacket(PingRequest packet, Guid clientId, CancellationToken cancellationToken)
        => connectionHandler.SendPacketAsync(new PingResponse(packet.Time), clientId);
}
