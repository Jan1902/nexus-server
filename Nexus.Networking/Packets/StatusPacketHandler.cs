using Microsoft.Extensions.Logging;
using Nexus.Networking.Abstraction.Packets;
using Nexus.Networking.Connections;
using System.Text.Json;

namespace Nexus.Networking.Packets;

internal class StatusPacketHandler(
    ConnectionHandler connectionHandler,
    ILogger<StatusPacketHandler> logger)
    : IPacketHandler<PingRequest>,
    IPacketHandler<StatusRequest>
{
    public Task HandlePacket(StatusRequest statusRequest, Guid clientId, CancellationToken cancellationToken)
    {
        logger.LogTrace(statusRequest.ToString());

        var content = new StatusResponseContent(
            new StatusVersion("1.21.1", 767),
            new StatusPlayers(20, connectionHandler.ClientConnections.Count, connectionHandler.ClientConnections.Select(c => new StatusPlayersSample(c.Username ?? "Unknown", c.ClientId)).ToArray()),
            new StatusDescription("Nexus - A Minecraft server thought different"),
            "",
            false);

        var json = JsonSerializer.Serialize(content, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        logger.LogTrace(json);

        return connectionHandler.SendPacketAsync(new StatusResponse(json), clientId);
    }

    public Task HandlePacket(PingRequest packet, Guid clientId, CancellationToken cancellationToken)
        => connectionHandler.SendPacketAsync(new PingResponse(packet.Time), clientId);
}
