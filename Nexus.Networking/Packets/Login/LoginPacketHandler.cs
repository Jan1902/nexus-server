using Microsoft.Extensions.Logging;
using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;
using Nexus.Networking.Connections;
using Nexus.Networking.Packets.Configuration;

namespace Nexus.Networking.Packets.Login;

internal class LoginPacketHandler(
    ConnectionHandler connectionHandler,
    ILogger<LoginPacketHandler> logger)
    : IPacketHandler<Handshake>,
    IPacketHandler<LoginStart>,
    IPacketHandler<LoginAcknowledged>
{
    public Task HandlePacket(Handshake handshake, Guid clientId, CancellationToken cancellationToken)
    {
        connectionHandler.SetProtocolStateForClient(clientId, handshake.NextState);

        return Task.CompletedTask;
    }

    public Task HandlePacket(LoginStart loginStart, Guid clientId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Player {id} logged in as {name}", clientId, loginStart.Name);

        connectionHandler.AssignUsername(clientId, loginStart.Name);

        return connectionHandler.SendPacketAsync(new LoginSuccess(loginStart.PlayerUUID, loginStart.Name, [], true), clientId);
    }

    public Task HandlePacket(LoginAcknowledged packet, Guid clientId, CancellationToken cancellationToken)
    {
        connectionHandler.SetProtocolStateForClient(clientId, ProtocolState.Configuration);

        return connectionHandler.SendPacketAsync(new ClientboundKnownPacks([]), clientId);
    }
}
