using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Nexus.Networking.Data;
using Nexus.Networking.Packets;
using System.Net.Sockets;

namespace Nexus.Networking.Connections;

internal class ClientConnection(
    TcpClient tcpClient,
    PacketManager packetManager,
    ILogger<ClientConnection> logger)
{
    public Guid ClientId { get; } = Guid.NewGuid();

    private readonly byte[] _buffer = new byte[4096];
    private int _currentLength;

    private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        using var networkStream = tcpClient.GetStream();

        while (tcpClient.Connected)
        {
            var bytesRead = await networkStream.ReadAsync(new Memory<byte>(_buffer), cancellationToken);
            _currentLength += bytesRead;

            if (bytesRead == 0)
                break;

            var stream = _memoryStreamManager.GetStream(_buffer);
            var reader = new MinecraftBinaryReader(stream);
            var length = reader.ReadVarInt();

            if (length > _currentLength)
                continue;

            var packetData = reader.ReadBytes(length);
            packetManager.EnqueuePacket(packetData, ClientId);

            _currentLength = 0;

            logger.LogTrace("Received packet from client {id}", ClientId);
        }

        logger.LogInformation("Client {id} disconnected", ClientId);
    }
}