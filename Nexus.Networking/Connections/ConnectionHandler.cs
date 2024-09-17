using Microsoft.Extensions.Logging;
using Nexus.Framework.Abstraction;
using Nexus.Networking.Abstraction;
using Nexus.Networking.Abstraction.Packets;
using System.Net;
using System.Net.Sockets;

namespace Nexus.Networking.Connections;

internal class ConnectionHandler(
    ILogger<ConnectionHandler> logger,
    ClientConnectionFactory connectionFactory,
    NetworkingConfiguration configuration) : IInitializeAndRunAsync, IShutdownAsync
{
    private TcpListener? _tcpListener;

    internal List<ClientConnection> ClientConnections { get; } = [];

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        _tcpListener = new TcpListener(IPAddress.Any, configuration.Port);
        _tcpListener.Start();

        logger.LogInformation("Listening on port {port}", configuration.Port);

        return Task.CompletedTask;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (_tcpListener is null)
            throw new InvalidOperationException("Connection handler is not initialized.");

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);

            var clientConnection = connectionFactory.CreateClientConnection(client);
            ClientConnections.Add(clientConnection);

            logger.LogInformation("Accepted new client {id} from IP address {ip}", clientConnection.ClientId, ((IPEndPoint?) client.Client.RemoteEndPoint)?.Address.ToString() ?? "Unknown");

            _ = clientConnection.ListenAsync(cancellationToken).ContinueWith((_) => Task.Run(() => ClientConnections.Remove(clientConnection)));
        }
    }

    public async Task SendPacketAsync<TPacket>(TPacket packet, Guid? clientId = null) where TPacket : PacketBase
    {
        if (clientId is not null)
        {
            var client = ClientConnections.FirstOrDefault(x => x.ClientId == clientId);

            client?.SendPacketAsync(packet);
        }
        else
        {
            foreach (var client in ClientConnections)
                await client.SendPacketAsync(packet);
        }
    }

    public void SetProtocolStateForClient(Guid clientId, ProtocolState state)
    {
        var client = ClientConnections.FirstOrDefault(x => x.ClientId == clientId);
        client?.SetProtocolState(state);
    }

    public Task ShutdownAsync(CancellationToken cancellationToken)
    {
        if (_tcpListener is null)
            return Task.CompletedTask;

        _tcpListener.Stop();

        return Task.CompletedTask;
    }
}
