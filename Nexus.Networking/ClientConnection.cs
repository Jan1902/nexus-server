using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;

namespace Nexus.Networking;

internal class ClientConnection(TcpClient tcpClient, ILogger<ClientConnection> logger)
{
    public Guid ClientId { get; } = Guid.NewGuid();

    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        using var networkStream = tcpClient.GetStream();

        while (tcpClient.Connected)
        {
            var buffer = new byte[4096];
            var bytesRead = await networkStream.ReadAsync(new Memory<byte>(buffer), cancellationToken);

            if (bytesRead == 0)
                break;

            var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            logger.LogInformation("Received message from client {id}: {message}", ClientId, message);
        }

        logger.LogInformation("Client {id} disconnected", ClientId);
    }
}