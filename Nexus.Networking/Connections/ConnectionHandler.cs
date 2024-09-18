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

    private readonly List<ClientConnection> _clientConnections = [];

    public int ConnectionCount => _clientConnections.Count;

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

            if (_clientConnections.Count >= configuration.MaxConnections)
            {
                logger.LogWarning("Client attempted to connect but server is full");
                client.Close();

                continue;
            }

            var clientConnection = connectionFactory.CreateClientConnection(client);
            _clientConnections.Add(clientConnection);

            logger.LogInformation("Accepted new client {id} from IP address {ip}", clientConnection.ClientId, ((IPEndPoint?) client.Client.RemoteEndPoint)?.Address.ToString() ?? "Unknown");

            _ = clientConnection.ListenAsync(cancellationToken).ContinueWith((_) => Task.Run(() => _clientConnections.Remove(clientConnection)));
        }
    }

    public async Task SendPacketAsync<TPacket>(TPacket packet, Guid? clientId = null) where TPacket : PacketBase
    {
        if (clientId is not null)
        {
            var client = _clientConnections.FirstOrDefault(x => x.ClientId == clientId);

            client?.SendPacketAsync(packet);

            logger.LogTrace("Sent packet {packetType} to client {clientId}", packet.GetType().Name, clientId);
        }
        else
        {
            foreach (var client in _clientConnections)
                await client.SendPacketAsync(packet);

            logger.LogTrace("Sent packet {packetType} to all clients", packet.GetType().Name);
        }
    }

    public void SetProtocolStateForClient(Guid clientId, ProtocolState state)
    {
        var client = _clientConnections.FirstOrDefault(x => x.ClientId == clientId)
            ?? throw new ArgumentException("Client does not exist.");

        client.SetProtocolState(state);

        logger.LogTrace("Set protocol state to {state} for client {clientId}", state, clientId);
    }

    public void ReleaseReceiveLock(Guid clientId)
    {
        var client = _clientConnections.FirstOrDefault(x => x.ClientId == clientId)
            ?? throw new ArgumentException("Client does not exist.");

        client.ReceiveLock.Release();
    }

    public (Guid ClientId, string Username)[] GetClients()
        => _clientConnections.Select(x => (x.ClientId, x.Username ?? "Unknown")).ToArray();

    public void AssignUsername(Guid clientId, string username)
    {
        var client = _clientConnections.FirstOrDefault(x => x.ClientId == clientId)
            ?? throw new ArgumentException("Client does not exist.");

        client.Username = username;
    }

    public Task ShutdownAsync(CancellationToken cancellationToken)
    {
        if (_tcpListener is null)
            return Task.CompletedTask;

        _tcpListener.Stop();

        return Task.CompletedTask;
    }
}
