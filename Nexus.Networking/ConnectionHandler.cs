using Microsoft.Extensions.Logging;
using Nexus.Framework.Abstraction;
using System.Net;
using System.Net.Sockets;

namespace Nexus.Networking;

internal class ConnectionHandler(ILogger<ConnectionHandler> logger, ClientConnectionFactory connectionFactory) : IInitializeAndRunAsync, IShutdownAsync
{
    private TcpListener? _tcpListener;

    private readonly List<ClientConnection> _clientConnections = [];

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        _tcpListener = new TcpListener(IPAddress.Any, 25565);
        _tcpListener.Start();

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
            _clientConnections.Add(clientConnection);

            logger.LogInformation("Accepted new client {id} from IP address {ip}", clientConnection.ClientId, ((IPEndPoint?) client.Client.RemoteEndPoint)?.Address.ToString() ?? "Unknown");

            _ = clientConnection.ListenAsync(cancellationToken).ContinueWith((_) => Task.Run(() => _clientConnections.Remove(clientConnection)));
        }
    }

    public Task ShutdownAsync(CancellationToken cancellationToken)
    {
        if (_tcpListener is null)
            return Task.CompletedTask;

        _tcpListener.Stop();

        return Task.CompletedTask;
    }
}
