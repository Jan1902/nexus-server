using Microsoft.Extensions.Logging;
using Nexus.Framework.Abstraction;
using Nexus.Networking.Abstraction;
using Nexus.Networking.Connections;
using Nexus.Networking.Packets.Play;

namespace Nexus.Networking;

internal class KeepAliveHandler(
    NetworkingConfiguration configuration,
    ConnectionHandler connectionHandler,
    ILogger<KeepAliveHandler> logger) : IRunAsync
{
    private readonly Random _random = new();

    private readonly List<(Guid ClientId, long KeepAliveId)> _pendingKeepAlives = [];

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(configuration.KeepAliveIntervalSeconds * 1000, cancellationToken);

            foreach (var (ClientId, _) in _pendingKeepAlives)
            {
                connectionHandler.DisconnectClient(ClientId);
                logger.LogInformation("Disconnected client {clientId} due to timed out keep alive", ClientId);
            }

            var keepAliveId = _random.NextInt64();
            await connectionHandler.SendPacketAsync(new ClientboundKeepAlive(keepAliveId), null, ProtocolState.Play);

            _pendingKeepAlives.Clear();
            _pendingKeepAlives.AddRange(connectionHandler.GetClients(ProtocolState.Play).Select(c => (c.ClientId, keepAliveId)));

            logger.LogTrace("Sent out {count} keep alives with ID {id}", _pendingKeepAlives.Count, keepAliveId);
        }
    }

    public void HandleKeepAlive(Guid clientId, long keepAliveId)
    {
        if (!_pendingKeepAlives.Contains((clientId, keepAliveId)))
        {
            logger.LogWarning("Client {id} sent an invalid keep alive", clientId);
            connectionHandler.DisconnectClient(clientId);

            return;
        }

        _pendingKeepAlives.Remove((clientId, keepAliveId));
    }
}
